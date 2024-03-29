﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
}