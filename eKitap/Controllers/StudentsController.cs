using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eKitap.Models;
using Microsoft.AspNetCore.Authorization;
using System.Data;

namespace eKitap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class StudentsController : Controller
    {
        private readonly eKitapDbContext _context;

        public StudentsController(eKitapDbContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index()
        {
            var eKitapDbContext = _context.Student.Include(s => s.ClassRoom);
            return View(await eKitapDbContext.ToListAsync());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Student == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
            if (student == null)
            {
                return NotFound();
            }

            return View(await _context.BookStudentConnections.Where(c => c.StudentId == id && !c.IsDeleted).Include(c => c.Book).ToListAsync());
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Id");
            return View();
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TcId,Password,ClassRoomId,ApproveStatus,Id,CreateDate,LastUpdateDate,IsDeleted")] Student student)
        {
            ModelState.Remove("ClassRoom");
            ModelState.Remove("BookStudentConnections");
            if (ModelState.IsValid)
            {

                student.CreateDate = DateTime.Now;
                student.LastUpdateDate = DateTime.Now;
                student.IsDeleted = false;
                _context.Add(student);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Title", student.ClassRoomId);
            return View(student);
        }

        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Student == null)
            {
                return NotFound();
            }

            var student = await _context.Student.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Id", student.ClassRoomId);
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,TcId,Password,ClassRoomId,ApproveStatus,Id,CreateDate,LastUpdateDate,IsDeleted")] Student student)
        {
            if (id != student.Id)
            {
                return NotFound();
            }

            ModelState.Remove("password");
            if (ModelState.IsValid)
            {
                try
                {
                    var item = _context.Student.FirstOrDefault(c => c.Id == id);
                    if (item != null)
                    {
                        item.Name = student.Name;
                        if (!string.IsNullOrEmpty(student.Password))
                            item.Password = student.Password;
                        item.LastUpdateDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StudentExists(student.Id))
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
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Id", student.ClassRoomId);
            return View(student);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Student == null)
            {
                return NotFound();
            }

            var student = await _context.Student
                .Include(s => s.ClassRoom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Student == null)
            {
                return Problem("Entity set 'eKitapDbContext.Student'  is null.");
            }
            var student = await _context.Student.FindAsync(id);
            if (student != null)
            {
                student.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Student.Any(e => e.Id == id);
        }
    }
}
