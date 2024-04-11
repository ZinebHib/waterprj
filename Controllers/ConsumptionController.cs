using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
            // Récupérer toutes les consommations depuis la base de données
            var consumptions = await _context.Consumption.ToListAsync();

            // Calculer la moyenne de consommation par utilisateur pour chaque mois
            var averageConsumptionPerUserPerMonth = consumptions
                .GroupBy(c => new { c.UserId, c.Date.Year, c.Date.Month })
                .Select(grp => new
                {
                    UserId = grp.Key.UserId,
                    Year = grp.Key.Year,
                    Month = grp.Key.Month,
                    AverageConsumption = grp.Average(c => c.Volume)
                });

            // Calculer la moyenne générale de la consommation par mois
            var generalAverageConsumptionPerMonth = consumptions
                .GroupBy(c => new { c.Date.Year, c.Date.Month })
                .Select(grp => new
                {
                    Year = grp.Key.Year,
                    Month = grp.Key.Month,
                    AverageConsumption = grp.Average(c => c.Volume)
                });

            // Préparer les données pour les deux graphiques
            var userChartData = averageConsumptionPerUserPerMonth
                .ToDictionary(
                    grp => $"{grp.UserId}_{grp.Year}_{grp.Month}",
                    grp => grp.AverageConsumption);

            var generalChartData = generalAverageConsumptionPerMonth
                .ToDictionary(
                    grp => $"{grp.Year}_{grp.Month}",
                    grp => grp.AverageConsumption);

            // Passer les données au modèle pour affichage
            ViewData["UserChartData"] = userChartData;
            ViewData["GeneralChartData"] = generalChartData;

            return View(consumptions);
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
                _context.Add(consumption);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
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
            // Code de comparaison et de message d'avis ici

            return View();
        }


    }
}