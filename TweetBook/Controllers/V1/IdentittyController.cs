using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Contracts.V1;
using TweetBook.Contracts.V1.Requests;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Services;

namespace TweetBook.Controllers.V1
{
    public class IdentittyController : Controller
    {

        private readonly IIdentityService _identityService;

        public IdentittyController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost(ApiRoutes.Identity.Register)]
        public async Task<IActionResult> Register([FromBody]UserRegistrationRequest registrationReq)
        {
            var authResponse = await _identityService.RegisterUserAsync(registrationReq.Email, registrationReq.Password);
            if(!authResponse.IsSuccess)
            {
               return BadRequest( new AuthFailureResponse
                {
                    ErrorMessages = authResponse.ErrorMessage
                });
            }


            return Ok(new AuthSuccessResponse { 
                Token = authResponse.Token
            });
        } 
    }
}
