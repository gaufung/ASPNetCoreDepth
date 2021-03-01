using Microsoft.AspNetCore.Mvc;

namespace helloworld.Controllers
{
    public class HelloController : Controller
    {
        [HttpGet("/hello/{name}")]
        public IActionResult SayHello(string name)
        {
            ViewBag.Name = name;
            return View();
        }
    }
}