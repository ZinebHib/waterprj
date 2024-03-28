using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using waterprj.Data;
using waterprj.Models;

namespace waterprj.Controllers
{
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
           {
                 return _context.Estimation != null ? 
                             View(await _context.Estimation.ToListAsync()) :
                             Problem("Entity set 'ApplicationDbContext.Estimation'  is null.");
           }
        //to calculate the estimation based on user's behavior :
        // POST: Estimation/CalculateAndSaveEstimation
        // POST: Estimation/CalculateAndSaveEstimation
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

            // If ModelState is not valid, return to the Create view to correct errors
            return View("Create", estimation);
        }


        // Method to get the current user's ID
       



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
        public async Task<IActionResult> Create([Bind("Id,UserId,Date,EstimatedVolume,NumberOfPeople,HasPool,UsesDishwasher,LaundryFrequency,ShowerDuration,LeakDetection")] Estimation estimation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(estimation);
                await _context.SaveChangesAsync();
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
                    _context.Update(estimation);
                    await _context.SaveChangesAsync();
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
            return RedirectToAction(nameof(Index));
        }

        private bool EstimationExists(int id)
        {
          return (_context.Estimation?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
