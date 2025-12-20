using Blaga_Alexia_Denisa_Lab4.Data;
using Blaga_Alexia_Denisa_Lab4.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.ML;
using static Blaga_Alexia_Denisa_Lab4.PricePredictionModel;

namespace Blaga_Alexia_Denisa_Lab4.Controllers
{
    public class PredictionController : Controller
    {
        private readonly AppDbContext _context;

        public PredictionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Price()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Price(ModelInput input)
        {
            MLContext mlContext = new MLContext();

            ITransformer mlModel = mlContext.Model.Load(
                @"PricePredictionModel.mlnet",
                out var modelInputSchema
            );

            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);

            ModelOutput result = predEngine.Predict(input);
            ViewBag.Price = result.Score;

            var history = new PredictionHistory
            {
                PassengerCount = input.Passenger_count,
                TripTimeInSecs = input.Trip_time_in_secs,
                TripDistance = input.Trip_distance,
                PaymentType = input.Payment_type,
                PredictedPrice = result.Score,
                CreatedAt = DateTime.Now
            };

            _context.PredictionHistories.Add(history);
            await _context.SaveChangesAsync();

            return View(input);
        }

        [HttpGet]
        public async Task<IActionResult> History()
        {
            var history = await _context.PredictionHistories
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return View(history);
        }
    }
}