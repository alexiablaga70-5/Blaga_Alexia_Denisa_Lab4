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

[HttpGet]
    public async Task<IActionResult> Dashboard(DateTime? fromDate, DateTime? toDate)
    {
        // interogare de bază
        var query = _context.PredictionHistories.AsQueryable();

        // filtrare după interval
        if (fromDate.HasValue)
            query = query.Where(p => p.CreatedAt.Date >= fromDate.Value.Date);

        if (toDate.HasValue)
            query = query.Where(p => p.CreatedAt.Date <= toDate.Value.Date);

        // 1. Numărul total de predicții (DOAR din interval)
        var totalPredictions = await query.CountAsync();

        // 2. Preț mediu per tip de plată + count (DOAR din interval)
        var paymentTypeStats = await query
            .GroupBy(p => p.PaymentType)
            .Select(g => new PaymentTypeStat
            {
                PaymentType = g.Key,
                AveragePrice = g.Average(x => x.PredictedPrice),
                Count = g.Count()
            })
            .ToListAsync();

        // 3. Lista de prețuri (DOAR din interval)
        var allPredictions = await query
            .Select(p => p.PredictedPrice)
            .ToListAsync();

        // buckets
        var buckets = new List<PriceBucketStat>
    {
        new PriceBucketStat { Label = "0 - 10" },
        new PriceBucketStat { Label = "10 - 20" },
        new PriceBucketStat { Label = "20 - 30" },
        new PriceBucketStat { Label = "30 - 50" },
        new PriceBucketStat { Label = "> 50" }
    };

        foreach (var price in allPredictions)
        {
            if (price < 10) buckets[0].Count++;
            else if (price < 20) buckets[1].Count++;
            else if (price < 30) buckets[2].Count++;
            else if (price < 50) buckets[3].Count++;
            else buckets[4].Count++;
        }

        // ViewModel (include intervalul curent)
        var vm = new DashboardViewModel
        {
            TotalPredictions = totalPredictions,
            PaymentTypeStats = paymentTypeStats,
            PriceBuckets = buckets,
            FromDate = fromDate,
            ToDate = toDate
        };

        return View(vm);
    }

}

}

