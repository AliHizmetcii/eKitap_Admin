using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eKitap.Models;
using Microsoft.AspNetCore.Authorization;

namespace eKitap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminsController : Controller
    {
        private readonly eKitapDbContext _context;

        public AdminsController(eKitapDbContext context)
        {
            _context = context;
        }

        // GET: Admins
        public async Task<IActionResult> Index()
        {
            return View(await _context.AdminUsers.Where(c => !c.IsDeleted).ToListAsync());
        }

        // GET: Admins/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admins/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Password,Id,CreateDate,LastUpdateDate,IsDeleted")] Admin admin)
        {
            if (ModelState.IsValid)
            {
                admin.CreateDate = DateTime.Now;
                admin.LastUpdateDate = DateTime.Now;
                admin.IsDeleted = false;
                _context.Add(admin);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(admin);
        }

        // GET: Admins/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.AdminUsers == null)
            {
                return NotFound();
            }

            var admin = await _context.AdminUsers.FindAsync(id);
            if (admin == null)
            {
                return NotFound();
            }
            return View(admin);
        }

        // POST: Admins/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,Password,Id")] Admin admin)
        {
            if (id != admin.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Password");
            if (ModelState.IsValid)
            {
                try
                {
                    var item = _context.AdminUsers.FirstOrDefault(c => c.Id == id);
                    if (item != null)
                    {
                        item.Name = admin.Name;
                        if (!string.IsNullOrEmpty(admin.Password))
                            item.Password = admin.Password;
                        item.LastUpdateDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AdminExists(admin.Id))
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
            return View(admin);
        }

        // GET: Admins/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.AdminUsers == null)
            {
                return NotFound();
            }

            var admin = await _context.AdminUsers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (admin == null)
            {
                return NotFound();
            }

            return View(admin);
        }

        // POST: Admins/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.AdminUsers == null)
            {
                return Problem("Entity set 'eKitapDbContext.AdminUsers'  is null.");
            }
            var admin = await _context.AdminUsers.FirstOrDefaultAsync(c => c.Id == id);
            if (admin != null)
            {
                admin.IsDeleted = true;
                await _context.SaveChangesAsync();
            }


            return RedirectToAction(nameof(Index));
        }

        private bool AdminExists(int id)
        {
            return _context.AdminUsers.Any(e => e.Id == id);
        }
    }
}
