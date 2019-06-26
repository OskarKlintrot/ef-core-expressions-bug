using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace BusinessLogic
{
    public class PostService
    {
        private BloggingContext _context;

        public PostService(BloggingContext context)
        {
            _context = context;
        }

        public void Add(int blogId, string title)
        {
            var post = new Post
            {
                BlogId = blogId,
                Title = title,
                Created = DateTimeOffset.UtcNow.AddDays(-1),
                Publish = DateTimeOffset.UtcNow.AddDays(1),
            };

            _context.Posts.Add(post);
            _context.SaveChanges();
        }

        public IEnumerable<Post> GetAllUnpublishedPostsNOTWORKING()
        {
            var predicate = Post.GenerateIsNotYetPublishedExpression(DateTimeOffset.UtcNow);

            return _context.Posts
                .Where(predicate)
                .ToList();
        }

        public IEnumerable<Post> GetAllUnpublishedPosts()
        {
            Expression<Func<Post, bool>> predicate = x => x.Created <= DateTimeOffset.UtcNow && DateTimeOffset.UtcNow <= x.Publish;

            return _context.Posts
                .Where(predicate)
                .ToList();
        }
    }

}
