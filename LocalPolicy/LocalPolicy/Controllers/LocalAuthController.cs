using LocalPolicy.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LocalPolicy.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class LocalAuthController : ControllerBase
    {
        [HttpGet("action1")]
        [Authorize(Roles = "admin,employee")]
        public int Action1()
        {
            return 1;
        }

        [HttpGet("action2")]
        [Authorize(Roles = "admin,employee")]
        public int Action2()
        {
            return 2;
        }

        [HttpGet("action3")]
        [Authorize(Roles = "admin,customer")]
        public int Action3()
        {
            return 3;
        }

        [HttpGet("action4")]
        [Authorize(Policy = "Action4Access")]
        public int Action4()
        {
            return 4;
        }
    }
}

