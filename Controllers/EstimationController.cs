using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using waterprj.Data;
using waterprj.Models;

namespace waterprj.Controllers
{
    [Authorize]
    public class EstimationController : Controller
    {
        private readonly ApplicationDbContext _context;
        //added for userid
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EstimationController(ApplicationDbContext context , IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            //added for userid
            _httpContextAccessor = httpContextAccessor;
        }

        // GET: Estimation
          public async Task<IActionResult> Index()
           {//added for displaying the msg after editing parameters related to estimated volume 
             if (TempData["SuccessMessage"] != null)
    {
        ViewBag.SuccessMessage = TempData["SuccessMessage"];
    }
                 return _context.Estimation != null ? 
                             View(await _context.Estimation.ToListAsync()) :
                             Problem("Entity set 'ApplicationDbContext.Estimation'  is null.");
           }
        //to calculate the estimation based on user's behavior :
        // POST: Estimation/CalculateAndSaveEstimation

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateAndSaveEstimation([Bind("NumberOfPeople,HasPool,UsesDishwasher,LaundryFrequency,ShowerDuration,LeakDetection")] Estimation estimation)
        {
            if (ModelState.IsValid)
            {
                

                estimation.Date = DateTime.Now; // Set the current date and time

                // Perform estimation calculation
                double estimatedVolume = CalculateEstimation(estimation);

                // Set the calculated estimation volume
                estimation.EstimatedVolume = estimatedVolume;

                // Save the estimation to the database
                _context.Estimation.Add(estimation);
                await _context.SaveChangesAsync();

                // Pass the estimation result to TempData
                TempData["EstimationResult"] = estimatedVolume.ToString();

                // Redirect to the index page
                return RedirectToAction(nameof(Index));
            }

            // If ModelState is not valid, return to the Index view to correct errors
            return View("Create",estimation);
        }


       



        // Method to calculate the estimation
        private double CalculateEstimation(Estimation estimation)
        {
            // Implement your estimation calculation logic here
           
            double baseVolume = 100; // Base consumption volume
            double poolFactor = estimation.HasPool ? 50 : 0; // Additional volume if the user has a pool
            double dishwasherFactor = estimation.UsesDishwasher ? 30 : 0; // Additional volume if the user uses a dishwasher
                                                                        
            double totalVolume = baseVolume + (estimation.NumberOfPeople * 50) + poolFactor + dishwasherFactor;
            return totalVolume;
        }

        // GET: Estimation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Estimation == null)
            {
                return NotFound();
            }

            var estimation = await _context.Estimation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (estimation == null)
            {
                return NotFound();
            }

            return View(estimation);
        }

        // GET: Estimation/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Estimation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Date,EstimatedVolume,NumberOfPeople,HasPool,UsesDishwasher,LaundryFrequency,ShowerDuration,LeakDetection")] Estimation estimation)
        {
            if (ModelState.IsValid)
            { // Calculez automatiquement la valeur de EstimatedVolume s'il est null
                if (estimation.EstimatedVolume == null)
                {
                    double estimatedVolume = CalculateEstimation(estimation);
                    estimation.EstimatedVolume = estimatedVolume;
                }

                _context.Add(estimation);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Estimation created successfully. Estimated volume: " + estimation.EstimatedVolume + " Liters";

                return RedirectToAction(nameof(Index));

            }


            return View(estimation);
        }

        // GET: Estimation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Estimation == null)
            {
                return NotFound();
            }

            var estimation = await _context.Estimation.FindAsync(id);
            if (estimation == null)
            {
                return NotFound();
            }
            return View(estimation);
        }

        // POST: Estimation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,Date,EstimatedVolume,NumberOfPeople,HasPool,UsesDishwasher,LaundryFrequency,ShowerDuration,LeakDetection")] Estimation estimation)
        {
            if (id != estimation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Charger l'estimation originale depuis la base de données
                    var originalEstimation = await _context.Estimation.AsNoTracking().FirstOrDefaultAsync(e => e.Id == estimation.Id);

                    if (originalEstimation == null)
                    {
                        return NotFound();
                    }

                    // Si EstimatedVolume est null, recalculer l'estimation
                    if (estimation.EstimatedVolume == null)
                    {
                        double newEstimatedVolume = CalculateEstimation(estimation);
                        estimation.EstimatedVolume = newEstimatedVolume;
                    }

                    // Comparer les valeurs actuelles avec les valeurs originales
                    if (EstimationValuesAreChanged(estimation, originalEstimation))
                    {
                        // Recalculer l'estimation
                        double newEstimatedVolume = CalculateEstimation(estimation);
                        estimation.EstimatedVolume = newEstimatedVolume;
                    }


                    // Mettre à jour l'estimation dans la base de données
                    _context.Update(estimation);
                    await _context.SaveChangesAsync();

                    // Store the new estimated volume in TempData
                    TempData["SuccMessage"] = "Estimation updated successfully. Estimated volume: " + estimation.EstimatedVolume+" Litters";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EstimationExists(estimation.Id))
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
            return View(estimation);
        }

        //function to compare original values and detect changes
        private bool EstimationValuesAreChanged(Estimation currentEstimation, Estimation originalEstimation)
        {
            
            return currentEstimation.NumberOfPeople != originalEstimation.NumberOfPeople ||
                   currentEstimation.HasPool != originalEstimation.HasPool ||
                   currentEstimation.UsesDishwasher != originalEstimation.UsesDishwasher ||
                   currentEstimation.LaundryFrequency != originalEstimation.LaundryFrequency ||
                   currentEstimation.ShowerDuration != originalEstimation.ShowerDuration ||
                   currentEstimation.LeakDetection != originalEstimation.LeakDetection;
        }

        // GET: Estimation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Estimation == null)
            {
                return NotFound();
            }

            var estimation = await _context.Estimation
                .FirstOrDefaultAsync(m => m.Id == id);
            if (estimation == null)
            {
                return NotFound();
            }

            return View(estimation);
        }

        // POST: Estimation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Estimation == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Estimation'  is null.");
            }
            var estimation = await _context.Estimation.FindAsync(id);
            if (estimation != null)
            {
                _context.Estimation.Remove(estimation);
            }
            
            await _context.SaveChangesAsync();
            // Après avoir supprimé tous les enregistrements,  la méthode  réinitialise l'auto-incrémentation si nécessaire
            return RedirectToAction(nameof(ResetAutoIncrementIfEmpty));
            return RedirectToAction(nameof(Index));

        }

        private bool EstimationExists(int id)
        {
          return (_context.Estimation?.Any(e => e.Id == id)).GetValueOrDefault();
        }
/*       method  to reset id after clearing the database from estimation
*/        public IActionResult ResetAutoIncrementIfEmpty()
        {
            // Vérifier si la table Estimation est vide
            if (!_context.Estimation.Any())
            {
                // Réinitialiser l'auto-incrémentation de l'identifiant à 0
                _context.Database.ExecuteSqlRaw("DBCC CHECKIDENT ('Estimation', RESEED, 0)");
            }

            return RedirectToAction(nameof(Index)); // Rediriger vers l'action Index ou toute autre action après la réinitialisation
        }
    }
}
