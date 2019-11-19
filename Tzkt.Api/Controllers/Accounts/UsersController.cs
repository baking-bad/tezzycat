﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

using Tzkt.Api.Models;
using Tzkt.Api.Repositories;

namespace Tzkt.Api.Controllers
{
    [ApiController]
    [Route("v1/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly UserRepository Users;
        public UsersController(UserRepository users)
        {
            Users = users;
        }

        [HttpGet]
        public Task<IEnumerable<User>> Get([Min(0)] int p = 0, [Range(0, 1000)] int n = 100)
        {
            return Users.Get(n, p * n);
        }

        [HttpGet("{address}")]
        public Task<User> Get([TzAddress] string address)
        {
            return Users.Get(address);
        }
    }
}