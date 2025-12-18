using Microsoft.AspNetCore.Mvc;

namespace Blaga_Alexia_Denisa_Lab4.Controllers
{
    public class DurationController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(double distance)
        {
            // viteza medie 30 km/h
            double duration = (distance / 30) * 60; // minute
            ViewBag.Duration = duration;
            return View();
        }
    }
}
