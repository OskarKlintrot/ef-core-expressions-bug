using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Util
{
    public static class IntervalExpressionFactory
    {
        /// <summary>
        /// Generate an expression tree that can be used ie for creating a db query in EF to find entities within the interval. The interval will be from and including the start to but exluding the end. 
        /// </summary>
        /// <typeparam name="TEntity">Entity the expression tree will be built for.</typeparam>
        /// <param name="dateTimeOffset">Timestamp to verify if it exists within the interval.</param>
        /// <param name="startDateExp">Expression that should return the property to use as start.</param>
        /// <param name="endDateExp">Expression that should return the property to use as end.</param>
        /// <returns>Expression tree for Linq-to-SQL</returns>
        public static Expression<Func<TEntity, bool>> GenerateIntervalContainsExpression<TEntity>(
            DateTimeOffset dateTimeOffset,
            Expression<Func<TEntity, DateTimeOffset?>> startDateExp,
            Expression<Func<TEntity, DateTimeOffset?>> endDateExp)
            where TEntity : class
        {
            return GenerateIntervalContainsExpression(dateTimeOffset, startDateExp, endDateExp, includeStart: true, includeEnd: false);
        }

        /// <summary>
        /// Generate an expression tree that can be used ie for creating a db query in EF to find entities within the interval.
        /// Primarly built to be used for quering for <code>NodaTime.LocalDate</code>.
        /// The interval will be from and including the start to and including the end.
        /// </summary>
        /// <typeparam name="TEntity">Entity the expression tree will be built for.</typeparam>
        /// <param name="dateTime">Date to verify if it exists within the interval.</param>
        /// <param name="startDateExp">Expression that should return the property to use as start.</param>
        /// <param name="endDateExp">Expression that should return the property to use as end.</param>
        /// <returns>Expression tree for Linq-to-SQL</returns>
        public static Expression<Func<TEntity, bool>> GenerateIntervalContainsExpression<TEntity>(
            DateTime dateTime,
            Expression<Func<TEntity, DateTime?>> startDateExp,
            Expression<Func<TEntity, DateTime?>> endDateExp)
            where TEntity : class
        {
            return GenerateIntervalContainsExpression(dateTime, startDateExp, endDateExp, includeStart: true, includeEnd: true);
        }

        /// <summary>
        /// Generate an expression tree that can be used ie for creating a db query in EF to find entities within the interval.
        /// </summary>
        /// <typeparam name="TEntity">Entity the expression tree will be built for.</typeparam>
        /// <typeparam name="TComp">Type to be compared between.</typeparam>
        /// <param name="compareTo"></param>
        /// <param name="startPropertyExp">Expression that should return the property to use as start.</param>
        /// <param name="endPropretyExp">Expression that should return the property to use as end.</param>
        /// <param name="includeStart">Include or exlude start.</param>
        /// <param name="includeEnd">Include or exlude end.</param>
        /// <returns>Expression tree for Linq-to-SQL</returns>
        public static Expression<Func<TEntity, bool>> GenerateIntervalContainsExpression<TEntity, TComp>(
            TComp compareTo,
            Expression<Func<TEntity, TComp?>> startPropertyExp,
            Expression<Func<TEntity, TComp?>> endPropretyExp,
            bool includeStart = true,
            bool includeEnd = true)
            where TEntity : class
            where TComp : struct, IComparable<TComp>
        {
            var param = Expression.Parameter(typeof(TEntity), "entity");

            Expression<Func<TComp?, TComp?, bool, bool, bool>> intervalContainsExp = (start, end, inclStart, inclEnd) =>
                (start == null || (start.Value.CompareTo(compareTo) <= 0 && inclStart) || (start.Value.CompareTo(compareTo) < 0 && !inclStart))
                && (end == null || (end.Value.CompareTo(compareTo) >= 0 && inclEnd) || (end.Value.CompareTo(compareTo) > 0 && !inclEnd));

            var args = new Dictionary<string, ConstantExpression>
            {
                [intervalContainsExp.Parameters[2].ToString()] = Expression.Constant(includeStart, typeof(bool)),
                [intervalContainsExp.Parameters[3].ToString()] = Expression.Constant(includeEnd, typeof(bool)),
            };

            var foo = Compose(intervalContainsExp, startPropertyExp, endPropretyExp, args);

            Expression<Func<bool, bool>> bar = dummy => dummy;

            return Compose(bar, foo, param);
        }

        private static Expression<Func<TEntity, TEntity, B>> Compose<TEntity, A, B>(
            Expression<Func<A, A, B, B, B>> f,
            Expression<Func<TEntity, A>> startExp,
            Expression<Func<TEntity, A>> endExp,
            IDictionary<string, ConstantExpression> constants)
        {
            var ex = ReplaceExpressions(f.Body, f.Parameters[0], startExp.Body);
            ex = ReplaceExpressions(ex, f.Parameters[1], endExp.Body);

            var foo = Expression.Lambda<Func<TEntity, TEntity, B>>(ex, startExp.Parameters[0], endExp.Parameters[0]);

            return (Expression<Func<TEntity, TEntity, B>>)new ParametersTransformToConstantVisitor(constants)
                .Visit(foo);
        }

        private static Expression<Func<A, C>> Compose<A, B, C>(Expression<Func<B, C>> f, Expression<Func<A, A, B>> g, ParameterExpression param)
        {
            var ex = ReplaceExpressions(f.Body, f.Parameters[0], g.Body);

            ex = new ParameterReplacer(param)
               .Visit(ex);

            return Expression.Lambda<Func<A, C>>(ex, param);
        }

        private static TExpr ReplaceExpressions<TExpr>(TExpr expression, Expression orig, Expression replacement)
            where TExpr : Expression
        {
            var replacer = new ExpressionReplacer(orig, replacement);

            return replacer.VisitAndConvert(expression, nameof(ReplaceExpressions));
        }

        private class ParametersTransformToConstantVisitor : ExpressionVisitor
        {
            private readonly IDictionary<string, ConstantExpression> _parameters;

            public ParametersTransformToConstantVisitor(IDictionary<string, ConstantExpression> parameters)
            {
                _parameters = parameters;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => _parameters.TryGetValue(node.Name, out var ce) ? (Expression)ce : node;

            protected override Expression VisitLambda<T>(Expression<T> node)
                => Expression.Lambda(Visit(node.Body), node.Parameters); // don't visit the parameters
        }

        private class ParameterReplacer : ExpressionVisitor
        {
            private readonly ParameterExpression _parameter;

            internal ParameterReplacer(ParameterExpression parameter)
            {
                _parameter = parameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
                => base.VisitParameter(_parameter);
        }

        private class ExpressionReplacer : ExpressionVisitor
        {
            private readonly Expression _from;
            private readonly Expression _to;

            public ExpressionReplacer(Expression from, Expression to)
            {
                _from = from;
                _to = to;
            }

            public override Expression Visit(Expression node)
                => node == _from
                    ? _to
                    : base.Visit(node);
        }
    }
}
