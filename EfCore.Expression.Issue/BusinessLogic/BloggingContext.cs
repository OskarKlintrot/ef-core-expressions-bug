using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static Util.IntervalExpressionFactory;

namespace BusinessLogic
{
    public class BloggingContext : DbContext
    {
        public BloggingContext()
        { }

        public BloggingContext(DbContextOptions<BloggingContext> options)
            : base(options)
        { }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Post> Posts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder
                    .UseLoggerFactory(DebugLoggerFactory)
                    .ConfigureWarnings(warnings => warnings.Throw(RelationalEventId.QueryClientEvaluationWarning))
                    .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=IssueDemo;Trusted_Connection=True;MultipleActiveResultSets=true");
            }
        }

        public static ILoggerFactory DebugLoggerFactory =>
            new ServiceCollection()
                .AddLogging(loggingBuilder =>
                    loggingBuilder
                        .AddDebug()
                        .AddFilter(category: DbLoggerCategory.Database.Command.Name,
                                   level: LogLevel.Debug))
                    .BuildServiceProvider()
                    .GetService<ILoggerFactory>();
    }

    public class Blog
    {
        public int BlogId { get; set; }
        public string Url { get; set; }

        public ICollection<Post> Posts { get; set; }
    }

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