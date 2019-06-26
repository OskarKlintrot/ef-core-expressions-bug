using System;
using System.Linq;
using BusinessLogic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EfCore.Expression.Issue.FunctionalTests
{
    [TestClass]
    public class PostServiceTests
    {
        [TestMethod]
        public void Get_All_Unpublished_Posts_Not_Working()
        {
            // Insert seed data into the database using one instance of the context
            using (var context = new BloggingContext())
            {
                context.Blogs.Add(new Blog
                {
                    Url = "http://sample.com/cats",
                    Posts = new Post[]
                    {
                            new Post
                            {
                                Created = DateTimeOffset.UtcNow.AddDays(-1),
                                Publish = DateTimeOffset.UtcNow.AddDays(1),
                            }
                    }
                });
                context.SaveChanges();
            }

            // Use a clean instance of the context to run the test
            using (var context = new BloggingContext())
            {
                var service = new PostService(context);
                var result = service.GetAllUnpublishedPostsNOTWORKING();

                Assert.IsTrue(result.Any());
            }
        }

        [TestMethod]
        public void Get_All_Unpublished_Posts_Working()
        {
            // Insert seed data into the database using one instance of the context
            using (var context = new BloggingContext())
            {
                context.Blogs.Add(new Blog
                {
                    Url = "http://sample.com/cats",
                    Posts = new Post[]
                    {
                            new Post
                            {
                                Created = DateTimeOffset.UtcNow.AddDays(-1),
                                Publish = DateTimeOffset.UtcNow.AddDays(1),
                            }
                    }
                });
                context.SaveChanges();
            }

            // Use a clean instance of the context to run the test
            using (var context = new BloggingContext())
            {
                var service = new PostService(context);
                var result = service.GetAllUnpublishedPosts();

                Assert.IsTrue(result.Any());
            }
        }
    }
}
