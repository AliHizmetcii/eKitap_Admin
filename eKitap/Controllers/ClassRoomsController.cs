using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using eKitap.Models;
using Microsoft.AspNetCore.Authorization;

namespace eKitap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClassRoomsController : Controller
    {
        private readonly eKitapDbContext _context;

        public ClassRoomsController(eKitapDbContext context)
        {
            _context = context;
        }

        // GET: ClassRooms
        public async Task<IActionResult> Index()
        {
            return View(await _context.ClassRoom.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.ClassRoom == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRoom
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (classRoom == null)
            {
                return NotFound();
            }

            return View((await _context.ClassRoom.Include(c => c.Books).Where(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync())?.Books.Where(c=>!c.IsDeleted).ToList()??new List<Book>());
        }

        // GET: ClassRooms/Details/5
        public async Task<IActionResult> Books(int? id)
        {
            if (id == null || _context.ClassRoom == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRoom
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classRoom == null)
            {
                return NotFound();
            }

            return View(classRoom);
        }

        public async Task<IActionResult> Students(int? id)
        {
            if (id == null || _context.ClassRoom == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRoom
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classRoom == null)
            {
                return NotFound();
            }

            return View(classRoom);
        }

        // GET: ClassRooms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ClassRooms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Id")] ClassRoom classRoom)
        {
            ModelState.Remove("Books");
            ModelState.Remove("Students");
            if (ModelState.IsValid)
            {
                classRoom.CreateDate = DateTime.Now;
                classRoom.LastUpdateDate = DateTime.Now;
                classRoom.IsDeleted = false;
                _context.Add(classRoom);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(classRoom);
        }

        // GET: ClassRooms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.ClassRoom == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRoom.FindAsync(id);
            if (classRoom == null)
            {
                return NotFound();
            }
            return View(classRoom);
        }

        // POST: ClassRooms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Title,Id,CreateDate,LastUpdateDate,IsDeleted")] ClassRoom classRoom)
        {
            if (id != classRoom.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Books");
            ModelState.Remove("Students");
            if (ModelState.IsValid)
            {
                try
                {
                    var item = _context.ClassRoom
                        .FirstOrDefault(c => c.Id == id);
                    if (item != null)
                    {
                        item.Title = classRoom.Title;
                        item.LastUpdateDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClassRoomExists(classRoom.Id))
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
            return View(classRoom);
        }

        // GET: ClassRooms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.ClassRoom == null)
            {
                return NotFound();
            }

            var classRoom = await _context.ClassRoom
                .FirstOrDefaultAsync(m => m.Id == id);
            if (classRoom == null)
            {
                return NotFound();
            }

            return View(classRoom);
        }

        // POST: ClassRooms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.ClassRoom == null)
            {
                return Problem("Entity set 'eKitapDbContext.ClassRoom'  is null.");
            }
            var item = await _context.ClassRoom.FirstOrDefaultAsync(c => c.Id == id);
            if (item != null)
            {
                item.IsDeleted = true;
                await _context.SaveChangesAsync();
            }


            return RedirectToAction(nameof(Index));
        }

        private bool ClassRoomExists(int id)
        {
            return _context.ClassRoom.Any(e => e.Id == id);
        }
    }
}
