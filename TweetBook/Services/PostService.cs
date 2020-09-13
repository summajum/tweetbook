using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TweetBook.Data;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public class PostService : IPostService
    {
        private readonly DataContext _dataContext;

        public PostService(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<List<Post>> GetAllPostAsync()
        {
            return await _dataContext.Posts.ToListAsync();
        }

        public async Task<Post> GetPostByIdAsync(Guid postId)
        {
            return await _dataContext.Posts.SingleOrDefaultAsync(x => x.Id == postId);
        }

        public async Task<bool> UpdatePostAsync(Post postToUpdate)
        {
            _dataContext.Posts.Update(postToUpdate);
            var updated = await _dataContext.SaveChangesAsync();
            return updated > 0;
        }

        public async Task<bool> DeletePostAsync(Guid postId)
        {
            var post = await this.GetPostByIdAsync(postId);
            if (post != null)
            {
                _dataContext.Posts.Remove(post);
                var deleted = await _dataContext.SaveChangesAsync();
                return deleted > 0;
            }
            return false;            
        }

        public async Task<bool> CreatePostAsync(Post createPost)
        {
            await _dataContext.Posts.AddAsync(createPost);
            var created = await _dataContext.SaveChangesAsync();

            return created > 0;
        }
    }
}
