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
        public async Task<IActionResult> History(
    string? paymentType,
    float? minPrice,
    float? maxPrice,
    DateTime? startDate,
    DateTime? endDate,
    string? sortOrder)
        {
            var query = _context.PredictionHistories.AsQueryable();

            // filtrare tip plata
            if (!string.IsNullOrEmpty(paymentType))
                query = query.Where(p => p.PaymentType == paymentType);

            // filtrare pret
            if (minPrice.HasValue)
                query = query.Where(p => p.PredictedPrice >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.PredictedPrice <= maxPrice.Value);

            // ✅ filtrare dupa data
            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate.Value);

            // ✅ sortare
            query = sortOrder switch
            {
                "date_asc" => query.OrderBy(p => p.CreatedAt),
                "date_desc" => query.OrderByDescending(p => p.CreatedAt),
                "price_asc" => query.OrderBy(p => p.PredictedPrice),
                "price_desc" => query.OrderByDescending(p => p.PredictedPrice),
                _ => query.OrderByDescending(p => p.CreatedAt)
            };

            // pastram valorile in view
            ViewBag.CurrentPaymentType = paymentType;
            ViewBag.CurrentMinPrice = minPrice;
            ViewBag.CurrentMaxPrice = maxPrice;
            ViewBag.CurrentStartDate = startDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentEndDate = endDate?.ToString("yyyy-MM-dd");
            ViewBag.CurrentSortOrder = sortOrder;

            return View(await query.ToListAsync());
        }
    }
}