using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ApiPlugin
{
    [Route("/")]
    public class HomeController : Controller
    {
        [HttpGet]
        public string Get()
        {
            return "Hello world! I'm B2B Admin API.";
        }
    }
}
