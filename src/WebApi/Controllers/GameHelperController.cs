﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SystemCommonLibrary.Json;
using WebApi.Components.Extension;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class GameHelperController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ContentResult New()
        {
            

            return new ContentResult();
        }

        [HttpPut]
        public async Task<bool> ModifyIntegral()
        {
            return true;
        }
    }
}
