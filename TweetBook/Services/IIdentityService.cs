﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TweetBook.Contracts.V1.Responses;
using TweetBook.Domain;

namespace TweetBook.Services
{
    public interface IIdentityService
    {
        Task<AuthenticationResult> RegisterUserAsync(string email, string password);
    }
}