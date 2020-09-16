using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace TweetBook.Extensions
{
    public static class ExtensionsHelper
    {
        public static string GetUserId(this HttpContext httpcontext)
        {
            if (httpcontext.User == null)
            {
                return string.Empty;
            }

            return httpcontext.User.Claims.Single(x => x.Type == "userid").Value;
        }
    }
}
