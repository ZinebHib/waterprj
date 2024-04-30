
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using waterprj.Data;
using waterprj.Models;



namespace waterprj.Controllers
{
    [Authorize]
    public class ConsumptionController : Controller
    {
        private readonly ApplicationDbContext _context;

        private readonly UserManager<IdentityUser> _userManager;
        public ConsumptionController(UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }





        // GET: Consumption

        public async Task<IActionResult> Index()
        {
            // Récupérer l'ID de l'utilisateur connecté
            string userId = _userManager.GetUserId(User);

            // Récupérer le rôle de l'utilisateur connecté
            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);
            var isAdmin = roles.Contains("Admin");

            IQueryable<Consumption> userConsumptionsQuery;

            if (isAdmin)
            {
                // Si l'utilisateur est un administrateur, récupérez toutes les données de consommation
                userConsumptionsQuery = _context.Consumption;
            }
            else
            {
                // Sinon, récupérez les données de consommation de l'utilisateur actuel seulement
                userConsumptionsQuery = _context.Consumption.Where(c => c.UserId == userId);
            }

            // Convertir la requête en liste
            var userConsumptions = await userConsumptionsQuery.ToListAsync();

            //pour extraire les données de consommation mensuelles de l'utilisateur
            var userMonthlyData = userConsumptions
                .GroupBy(c => new { Year = c.Date.Year, Month = c.Date.Month })
                .SelectMany(g => g.Select(c => new { Month = $"{g.Key.Month}/{g.Key.Year}", Volume = c.Volume }))
                .OrderBy(g => g.Month)
                .ToList();

            var userMonthlyDatabar = userConsumptions
                .GroupBy(c => new { Year = c.Date.Year, Month = c.Date.Month })
                .Select(g => new
                {
                    Month = $"{g.Key.Month}/{g.Key.Year}",
                    Volume = g.Sum(c => c.Volume) // Utilisez la somme plutôt que la moyenne
                })
                .OrderBy(g => g.Month)
                .ToList();

            // Calculer la moyenne de consommation générale par mois
            var generalMonthlyAverage = await _context.Consumption
                .GroupBy(c => new { c.Date.Year, c.Date.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    AverageVolume = g.Average(c => c.Volume)
                })
                .ToDictionaryAsync(x => x.Month.ToString("MMM yyyy"), x => Math.Round(x.AverageVolume, 2));

            // Passer les données à la vue
            ViewData["UserMonthlyData"] = userMonthlyData;
            ViewData["GeneralChartData"] = generalMonthlyAverage;
            ViewData["UserMonthlyDatabar"] = JsonConvert.SerializeObject(userMonthlyDatabar);

            return View(userConsumptions);
        }







        // GET: Consumption/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Consumption == null)
            {
                return NotFound();
            }

            var consumption = await _context.Consumption
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consumption == null)
            {
                return NotFound();
            }

            return View(consumption);
        }

        // GET: Consumption/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Consumption/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Date,Volume")] Consumption consumption)
        {
            if (ModelState.IsValid)
            {
                // Obtenez l'ID de l'utilisateur actuellement connecté
                string userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // Assignez l'ID de l'utilisateur à la consommation
                consumption.UserId = userId;


                // Ajoutez la consommation à la base de données
                _context.Add(consumption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
        

            foreach (var modelState in ModelState.Values)
            {
                foreach (var error in modelState.Errors)
                {
                    // Log ou traitement des erreurs ici
                    Console.WriteLine(error.ErrorMessage);
                }
            }
            return View(consumption);
        }

        // GET: Consumption/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Consumption == null)
            {
                return NotFound();
            }

            var consumption = await _context.Consumption.FindAsync(id);
            if (consumption == null)
            {
                return NotFound();
            }
            return View(consumption);
        }

        // POST: Consumption/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Date,Volume")] Consumption consumption)
        {
            if (id != consumption.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(consumption);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ConsumptionExists(consumption.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(consumption);
        }

        // GET: Consumption/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Consumption == null)
            {
                return NotFound();
            }

            var consumption = await _context.Consumption
                .FirstOrDefaultAsync(m => m.Id == id);
            if (consumption == null)
            {
                return NotFound();
            }

            return View(consumption);
        }

        // POST: Consumption/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Consumption == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Consumption'  is null.");
            }
            var consumption = await _context.Consumption.FindAsync(id);
            if (consumption != null)
            {
                _context.Consumption.Remove(consumption);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ConsumptionExists(int id)
        {
            return (_context.Consumption?.Any(e => e.Id == id)).GetValueOrDefault();
        }


        // GET: Consumption/Advice
        public IActionResult Advice()
        {
           

            return View();

        }


        public async Task<IActionResult> MonthlyConsumptionChart()
        {
            var userId = _userManager.GetUserId(User);
            var userMonthlyData = _context.Consumption
                .Where(c => c.UserId == userId)
                .GroupBy(c => new { c.Date.Year, c.Date.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new { Month = $"{g.Key.Month}/{g.Key.Year}", Volume = g.Sum(c => c.Volume) });

            return Json(userMonthlyData);
        }

        [HttpPost]
        public IActionResult Compare()
        {
            // Récupérer l'ID de l'utilisateur connecté
            string userId = _userManager.GetUserId(User);

            // Récupérer les données de consommation de l'utilisateur
            var userConsumptions = _context.Consumption
                .Where(c => c.UserId == userId)
                .ToList();

            // Calculer la moyenne de consommation générale par mois
            var generalMonthlyAverage = _context.Consumption
                .GroupBy(c => new { c.Date.Year, c.Date.Month })
                .Select(g => new
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1),
                    AverageVolume = g.Average(c => c.Volume)
                })
                .ToDictionary(x => x.Month.ToString("MMM yyyy"), x => Math.Round(x.AverageVolume, 2));

            // Comparaison de la consommation avec la moyenne et génération du message
            var exceedingMonths = userConsumptions
                .Where(c => generalMonthlyAverage.ContainsKey(c.Date.ToString("MMM yyyy")) && c.Volume > generalMonthlyAverage[c.Date.ToString("MMM yyyy")])
                .Select(c => c.Date.ToString("MMM yyyy"))
                .Distinct()
                .ToList();

            string message;
            if (exceedingMonths.Any())
            {
                message = $"Your consumption has exceeded the average for the following months: {string.Join(", ", exceedingMonths)}!! We recommend that you pay attention to your water usage and visit our advice page for more tips on water conservation.";
            }
            else
            {
                message = "Congratulations! Your water consumption is average. Keep it up.";
            }

            bool showAdviceButton = exceedingMonths.Any();

            return Json(new { message = message, showAdviceButton = showAdviceButton });
        }










    }
}