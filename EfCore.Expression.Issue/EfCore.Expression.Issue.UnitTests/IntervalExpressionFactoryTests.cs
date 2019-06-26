using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NodaTime;
using NodaTime.Text;
using static Util.IntervalExpressionFactory;

namespace EfCore.Issue.UnitTests
{
    [TestClass]
    public class IntervalExpressionFactoryTests
    {
        [DataTestMethod]
        [DataRow("[ -> ]", "2018-07-15 00:00:00 +00:00", true, DisplayName = "Null start and end")]
        [DataRow("[ -> 2018-07-31 00:00:00 +00:00]", "2018-07-15 00:00:00 +00:00", true, DisplayName = "Null start, date before end")]
        [DataRow("[2018-07-01 00:00:00 +00:00 -> ", "2018-07-15 00:00:00 +00:00", true, DisplayName = "Null end, date after start")]
        [DataRow("[ -> 2018-07-31 00:00:00 +00:00]", "2018-08-01 00:00:00 +00:00", false, DisplayName = "Null start, date after end")]
        [DataRow("[2018-07-01 00:00:00 +00:00 -> ", "2018-06-30 00:00:00 +00:00", false, DisplayName = "Null end, date before start")]
        [DataRow("[2018-07-01 00:00:00 +00:00 -> 2018-07-31 00:00:00 +00:00]", "2018-07-15 00:00:00 +00:00", true, DisplayName = "Date smack in the middle")]
        [DataRow("[2018-07-01 00:00:00 +00:00 -> 2018-07-31 00:00:00 +00:00]", "2018-07-01 00:00:00 +00:00", true, DisplayName = "Date at start")]
        [DataRow("[2018-07-01 00:00:00 +00:00 -> 2018-07-31 00:00:00 +00:00]", "2018-07-31 00:00:00 +00:00", false, DisplayName = "Date at end")]
        public void IntervalContainsExpressionDateTimeOffset(string intervalsText, string dateTimeOffset, bool expected)
        {
            // Arrange
            var bar = TestDateTimeOffsetInterval.GenerateIsActiveExpression(DateTimeOffset.Parse(dateTimeOffset));
            var intervals = ParseDateTimeOffsetIntervals(intervalsText);

            // Act
            var active = intervals
                .AsQueryable()
                .Any(bar);

            // Assert
            Assert.AreEqual(active, expected);
        }

        [DataTestMethod]
        [DataRow("[ -> ]", "2018-07-15", true, DisplayName = "Null start and end")]
        [DataRow("[ -> 2018-07-31]", "2018-07-15", true, DisplayName = "Null start, date before end")]
        [DataRow("[2018-07-01 -> ", "2018-07-15", true, DisplayName = "Null end, date after start")]
        [DataRow("[ -> 2018-07-31]", "2018-08-01", false, DisplayName = "Null start, date after end")]
        [DataRow("[2018-07-01 -> ", "2018-06-30", false, DisplayName = "Null end, date before start")]
        [DataRow("[2018-07-01 -> 2018-07-31]", "2018-07-15", true, DisplayName = "Date smack in the middle")]
        [DataRow("[2018-07-01 -> 2018-07-31]", "2018-07-01", true, DisplayName = "Date at start")]
        [DataRow("[2018-07-01 -> 2018-07-31]", "2018-07-31", true, DisplayName = "Date at end")]
        public void IntervalContainsExpressionLocalDate(string intervalsText, string date, bool expected)
        {
            // Arrange
            var bar = TestLocalDateInterval.GenerateIsActiveExpression(LocalDatePattern.Iso.Parse(date).GetValueOrThrow());
            var intervals = ParseLocalDateIntervals(intervalsText);

            // Act
            var active = intervals
                .AsQueryable()
                .Any(bar);

            // Assert
            Assert.AreEqual(active, expected);
        }

        [DataTestMethod]
        [DataRow("[ -> ]", "2018-07-15", true, DisplayName = "Null start and end")]
        [DataRow("[ -> 2018-07-31]", "2018-07-15", true, DisplayName = "Null start, date before end")]
        [DataRow("[2018-07-01 -> ", "2018-07-15", true, DisplayName = "Null end, date after start")]
        [DataRow("[ -> 2018-07-31]", "2018-08-01", false, DisplayName = "Null start, date after end")]
        [DataRow("[2018-07-01 -> ", "2018-06-30", true, DisplayName = "Null end, date before start")]
        [DataRow("[2018-07-01 -> 2018-07-31]", "2018-07-15", true, DisplayName = "Date smack in the middle")]
        [DataRow("[2018-07-01 -> 2018-07-31]", "2018-07-01", true, DisplayName = "Date at start")]
        [DataRow("[2018-07-01 -> 2018-07-31]", "2018-07-31", true, DisplayName = "Date at end")]
        public void IntervalContainsExpressionLocalDateFuture(string intervalsText, string date, bool expected)
        {
            // Arrange
            var bar = TestLocalDateInterval.GenerateIsActiveNowOrInTheFutureExpression(LocalDatePattern.Iso.Parse(date).GetValueOrThrow());
            var intervals = ParseLocalDateIntervals(intervalsText);

            // Act
            var active = intervals
                .AsQueryable()
                .Any(bar);

            // Assert
            Assert.AreEqual(active, expected);
        }

        [DataTestMethod]
        [DataRow("[ -> ]", 5, true, true, true, DisplayName = "Null start and end")]
        [DataRow("[ -> 10]", 5, true, true, true, DisplayName = "Null start, int before end")]
        [DataRow("[1 -> ", 5, true, true, true, DisplayName = "Null end, int after start")]
        [DataRow("[ -> 10]", 15, true, true, false, DisplayName = "Null start, int after end")]
        [DataRow("[1 -> ", 0, true, true, false, DisplayName = "Null end, int before start")]
        [DataRow("[1 -> 10]", 5, true, true, true, DisplayName = "Int smack in the middle")]
        [DataRow("[1 -> 10]", 1, true, false, true, DisplayName = "Int at start, include start")]
        [DataRow("[1 -> 10]", 1, false, false, false, DisplayName = "Int at start, exclude start")]
        [DataRow("[1 -> 10]", 10, false, true, true, DisplayName = "Int at end, include end")]
        [DataRow("[1 -> 10]", 10, false, false, false, DisplayName = "Int at end, exlude end")]
        public void IntervalContainsExpressionInteger(string intervalsText, int i, bool includeStart, bool includeEnd, bool expected)
        {
            // Arrange
            var bar = TestIntInterval.GenerateIsActiveExpression(i, includeStart, includeEnd);
            var intervals = ParseIntIntervals(intervalsText);

            // Act
            var active = intervals
                .AsQueryable()
                .Any(bar);

            // Assert
            Assert.AreEqual(active, expected);
        }

        private class TestLocalDateInterval
        {
#pragma warning disable IDE1006 // Naming Styles to emulate backing field in EF 6
            private DateTime? _from { get; }
            private DateTime? _to { get; }
#pragma warning restore IDE1006 // Naming Styles

            public TestLocalDateInterval(LocalDate? from, LocalDate? to)
            {
                _from = from.HasValue ? from.Value.ToDateTimeUnspecified() : (DateTime?)null;
                _to = to.HasValue ? to.Value.ToDateTimeUnspecified() : (DateTime?)null;
            }

            public LocalDate? From => _from.HasValue ? LocalDate.FromDateTime(_from.Value) : (LocalDate?)null;

            public LocalDate? To => _to.HasValue ? LocalDate.FromDateTime(_to.Value) : (LocalDate?)null;

            public static Expression<Func<TestLocalDateInterval, bool>> GenerateIsActiveExpression(LocalDate date)
            {
                var dateTime = date.ToDateTimeUnspecified();

                return GenerateIntervalContainsExpression<TestLocalDateInterval>(dateTime, x => x._from, x => x._to);
            }

            public static Expression<Func<TestLocalDateInterval, bool>> GenerateIsActiveNowOrInTheFutureExpression(LocalDate date)
            {
                var dateTime = date.ToDateTimeUnspecified();

                return GenerateIntervalContainsExpression<TestLocalDateInterval>(dateTime, _ => null, x => x._to);
            }
        }

        private class TestDateTimeOffsetInterval
        {
#pragma warning disable IDE1006 // Naming Styles to emulate backing field in EF 6
            private DateTimeOffset? _from { get; }
            private DateTimeOffset? _to { get; }
#pragma warning restore IDE1006 // Naming Styles

            public TestDateTimeOffsetInterval(DateTimeOffset? from, DateTimeOffset? to)
            {
                _from = from;
                _to = to;
            }

            public static Expression<Func<TestDateTimeOffsetInterval, bool>> GenerateIsActiveExpression(DateTimeOffset dateTimeOffset)
            {
                return GenerateIntervalContainsExpression<TestDateTimeOffsetInterval>(dateTimeOffset, x => x._from, x => x._to);
            }
        }

        private class TestIntInterval
        {
#pragma warning disable IDE1006 // Naming Styles to emulate backing field in EF 6
            private int? _from { get; }
            private int? _to { get; }
#pragma warning restore IDE1006 // Naming Styles

            public TestIntInterval(int? from, int? to)
            {
                _from = from;
                _to = to;
            }

            public static Expression<Func<TestIntInterval, bool>> GenerateIsActiveExpression(int @int, bool includeStart, bool includeEnd)
            {
                return GenerateIntervalContainsExpression<TestIntInterval, int>(@int, x => x._from, x => x._to, includeStart, includeEnd);
            }
        }

        private static IEnumerable<TestIntInterval> ParseIntIntervals(string intervalsText)
        {
            return intervalsText
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(intervalText =>
                {
                    var parts = intervalText
                        .Trim(' ', '[', ']')
                        .Split(new[] { "->" }, StringSplitOptions.None)
                        .Select(x => x.Trim())
                        .Select(x => int.TryParse(x, out int result) ? result : (int?)null)
                        .ToArray();

                    return new TestIntInterval(parts[0], parts[1]);
                });
        }

        private static IEnumerable<TestDateTimeOffsetInterval> ParseDateTimeOffsetIntervals(string intervalsText)
        {
            return intervalsText
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(intervalText =>
                {
                    var parts = intervalText
                        .Trim(' ', '[', ']')
                        .Split(new[] { "->" }, StringSplitOptions.None);

                    return new TestDateTimeOffsetInterval(DateTimeOffsetHelpers.ParseNullable(parts[0]), DateTimeOffsetHelpers.ParseNullable(parts[1]));
                });
        }

        private static IEnumerable<TestLocalDateInterval> ParseLocalDateIntervals(string intervalsText)
        {
            return intervalsText
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(intervalText =>
                {
                    var localDates = intervalText
                        .Trim(' ', '[', ']')
                        .Split(new[] { "->" }, StringSplitOptions.None)
                        .Select(x => x.Trim())
                        .Select(LocalDateHelpers.ParseNullable)
                        .ToArray();

                    return new TestLocalDateInterval(from: localDates[0], to: localDates[1]);
                });
        }
    }

    #region Helpers
    public static class DateTimeOffsetHelpers
    {
        public static DateTimeOffset? ParseNullable(string dateTimeOffset)
        {
            return String.IsNullOrEmpty(dateTimeOffset)
                ? (DateTimeOffset?)null
                : DateTimeOffset.Parse(dateTimeOffset);
        }
    }

    public static class LocalDateHelpers
    {
        public static LocalDate? ParseNullable(string localDate)
        {
            return String.IsNullOrEmpty(localDate)
                ? (LocalDate?)null
                : LocalDatePattern.Iso.Parse(localDate).GetValueOrThrow();
        }
    }
    #endregion
}
