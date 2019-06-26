using System;
using System.Linq;
using BusinessLogic;
using Microsoft.EntityFrameworkCore;

namespace EfCore.Issue
{
    class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            using (var db = new BloggingContext())
            {
                try
                {
                    db.Database.Migrate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("An error occurred migrating the DB.");
                    Console.WriteLine(ex);
                }
            }

            using (var db = new BloggingContext())
            {
                var blogs = await db.Blogs.ToListAsync();
                var posts = await db.Posts.ToListAsync();

                db.Blogs.RemoveRange(blogs);
                db.Posts.RemoveRange(posts);

                await db.SaveChangesAsync();
            }

            var blogId = 0;

            using (var db = new BloggingContext())
            {
                var blogService = new BlogService(db);

                blogService.Add("http://blogs.msdn.com/adonet");

                blogId = blogService
                    .Find(string.Empty)
                    .Single()
                    .BlogId;

                Console.WriteLine("{0} records saved to database", 1);
            }

            using (var db = new BloggingContext())
            {
                var postService = new PostService(db);

                postService.Add(blogId, "Hello world!");

                Console.WriteLine("{0} records saved to database", 1);
            }

            Console.WriteLine();

            using (var db = new BloggingContext())
            {
                var blogService = new BlogService(db);

                var blogs = blogService.Find(string.Empty);

                Console.WriteLine("All blogs in database:");

                foreach (var blog in blogs)
                {
                    Console.WriteLine(" - {0}", blog.Url);
                    Console.WriteLine("   - {0}", blog.Posts.First().Title);
                }
            }
        }
    }
}
