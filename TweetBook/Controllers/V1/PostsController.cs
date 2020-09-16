using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.V3.Pages.Internal.Account.Manage;
using Microsoft.AspNetCore.Mvc;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Requests;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Domain;
using TweetBook.Extensions;
using TweetBook.Services;

namespace TweetBook.Controllers.V1
{   
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class PostsController : Controller
    {
        private IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpGet(ApiRoutes.Posts.GetAll)]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _postService.GetAllPostAsync();
            return Ok(posts);
        }

        [HttpGet(ApiRoutes.Posts.Get)]
        public async Task<IActionResult> Get([FromRoute] Guid postId)
        {
            var post = await _postService.GetPostByIdAsync(postId);
            
            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [HttpDelete(ApiRoutes.Posts.Delete)]
        public async Task<IActionResult> Delete([FromRoute] Guid postId)
        {
            var ifUserOwnsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!ifUserOwnsPost)
            {
                return Unauthorized(
                    new
                    {
                        error = "You don't own the post"
                    });
            }

            var isPostDeleted = await _postService.DeletePostAsync(postId);

            if (isPostDeleted)
                return NoContent();
            return NotFound();
        }

        [HttpPut(ApiRoutes.Posts.Update)]
        public async Task<IActionResult> Update([FromRoute] Guid postId, [FromBody] UpdatePostRequest postRequest)
        {
            var ifUserOwnsPost = await _postService.UserOwnsPostAsync(postId, HttpContext.GetUserId());

            if (!ifUserOwnsPost)
            {
                return Unauthorized(
                    new
                    {
                        error = "You don't own the post"
                    });
            }

            var post = await _postService.GetPostByIdAsync(postId);
            post.Name = postRequest.Name;
                                  
            var isUpdated = await _postService.UpdatePostAsync(post);

            if (isUpdated)
                return Ok(post);
            return NotFound();
        }

        [HttpPost(ApiRoutes.Posts.Create)]
        public async Task<IActionResult> Create([FromBody] CreatePostRequest postRequest)
        {
            var post = new Post
            {
                Name = postRequest.Name,
                UserId = HttpContext.GetUserId()
            };

            await _postService.CreatePostAsync(post);

            var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.ToUriComponent()}";
            var locationUri = baseUrl + "/" + ApiRoutes.Posts.Get.Replace("{postId}", post.Id.ToString());

            var postResponse = new PostResponses
            {
                Id = post.Id
            };

            return Created(locationUri, postResponse);
        }
    }
}
