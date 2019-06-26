using System;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Util.IntervalExpressionFactory;

namespace BusinessLogic
{

    public class Post
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTimeOffset Created { get; set; }
        public DateTimeOffset Publish { get; set; }

        public static Expression<Func<Post, bool>> GenerateIsNotYetPublishedExpression(DateTimeOffset dateTimeOffset)
            => GenerateIntervalContainsExpression<Post>(dateTimeOffset, startDateExp: x => x.Created, endDateExp: x => x.Publish);

        public int BlogId { get; set; }
        public Blog Blog { get; set; }
    }
}