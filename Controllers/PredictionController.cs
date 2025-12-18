using Microsoft.AspNetCore.Mvc;
using static Blaga_Alexia_Denisa_Lab4.PricePredictionModel;

namespace Blaga_Alexia_Denisa_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Price(ModelInput input)
        {
            // simulare predicție (mock)
            ViewBag.Price = 123.45f;
            return View(input);
        }
    }
}