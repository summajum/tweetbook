using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public interface IPostService
    {
        List<Post> GetAllPost();
        Post GetPostById(Guid postId);
        bool UpdatePost(Post postToUpdate);
    }
}
