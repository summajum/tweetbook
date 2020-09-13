using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public class PostService : IPostService
    {
        private readonly List<Post> _posts;

        public PostService()
        {
            _posts = new List<Post>();
            for (int i = 0; i < 5; i++)
            {
                _posts.Add(new Post
                {
                    Id = Guid.NewGuid(),
                    Name = $"Mesage Id {i}"
                });
            }
        }

        public List<Post> GetAllPost()
        {
            return _posts;
        }

        public Post GetPostById(Guid postId)
        {
            return _posts.SingleOrDefault(x => x.Id == postId);
        }

        public bool UpdatePost(Post postToUpdate)
        {
            var postExists = this.GetPostById(postToUpdate.Id) != null;

            if (!postExists)
                return false;

            var index = _posts.FindIndex(x => x.Id == postToUpdate.Id);
            _posts[index] = postToUpdate;
            return true;
        }

        public bool DeletePost(Guid postId)
        {
            var post = this.GetPostById(postId);

            if (post == null)
                return false;
            return _posts.Remove(post);
        }
    }
}
