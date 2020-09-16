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
            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Values.SelectMany(x => x.Errors.Select(xx => xx.ErrorMessage));
                
                return BadRequest(new AuthFailureResponse
                {
                    ErrorMessages = errorList
                });
            }

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

        [HttpPost(ApiRoutes.Identity.Login)]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginReq)
        {
            var authResponse = await _identityService.LoginUserAsync(loginReq.Email, loginReq.Password);
            if (!authResponse.IsSuccess)
            {
                return BadRequest(new AuthFailureResponse
                {
                    ErrorMessages = authResponse.ErrorMessage
                });
            }


            return Ok(new AuthSuccessResponse
            {
                Token = authResponse.Token
            });
        }
    }
}
