using System.Collections;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using eKitap.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;



namespace eKitap.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BooksController : Controller
    {
        private readonly eKitapDbContext _context;
        private readonly IWebHostEnvironment _env;
        public BooksController(eKitapDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Books
        public async Task<IActionResult> Index()
        {
            var eKitapDbContext = _context.Kitaplar.
                Include(b => b.ClassRoom).
                Include(b => b.BookStudentConnections);
            return View(await eKitapDbContext.Where(c => !c.IsDeleted).ToListAsync());
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Kitaplar == null)
            {
                return NotFound();
            }

            var book = await _context.Kitaplar
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(await _context.BookStudentConnections.Include(c => c.Comments).Include(c => c.Book).Include(c => c.Student).Where(c => c.Id == id && !c.IsDeleted).FirstOrDefaultAsync());

        }

        // GET: Books/Create
        public IActionResult Create()
        {
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Title");
            return View();
        }

        // POST: Books/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,ClassRoomId,DownlaodCount,PdfName,Id")] Book book)
        {
            if (Request.Form.Files.Count == 1)
                ModelState.Remove("PdfName");

            ModelState.Remove("ClassRoom");
            ModelState.Remove("BookStudentConnections");

            if (ModelState.IsValid)
            {
                var file = Request.Form.Files.FirstOrDefault();
                book.PdfName = await SavePdfFile(file, Path.Combine(_env.WebRootPath, "Books"));
                book.CreateDate = DateTime.Now;
                book.LastUpdateDate = DateTime.Now;
                book.DownlaodCount = 0;
                book.IsDeleted = false;
                _context.Add(book);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Title", book.ClassRoomId);
            return View(book);
        }

        // GET: Books/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Kitaplar == null)
            {
                return NotFound();
            }

            var book = await _context.Kitaplar.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Title", book.ClassRoomId);
            return View(book);
        }

        // POST: Books/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Name,ClassRoomId,DownlaodCount,PdfName,Id,CreateDate,LastUpdateDate,IsDeleted")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }
            ModelState.Remove("PdfName");
            ModelState.Remove("ClassRoom");
            ModelState.Remove("BookStudentConnections");
            if (ModelState.IsValid)
            {
                try
                {
                    var item = _context.Kitaplar.FirstOrDefault(c => c.Id == id);
                    if (item != null)
                    {
                        if (Request.Form.Files.Count == 1)
                        {
                            if (System.IO.File.Exists(Path.Combine(_env.WebRootPath, "Books", item.PdfName)))
                                System.IO.File.Delete(Path.Combine(_env.WebRootPath, "Books", item.PdfName));
                            item.PdfName = await SavePdfFile(Request.Form.Files.First(), Path.Combine(_env.WebRootPath, "Books"));
                        }
                        item.Name = book.Name;
                        item.ClassRoomId = book.ClassRoomId;
                        item.LastUpdateDate = DateTime.Now;
                        await _context.SaveChangesAsync();
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
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
            ViewData["ClassRoomId"] = new SelectList(_context.ClassRoom, "Id", "Id", book.ClassRoomId);
            return View(book);
        }

        // GET: Books/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Kitaplar == null)
            {
                return NotFound();
            }

            var book = await _context.Kitaplar
                .Include(b => b.ClassRoom)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        // POST: Books/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Kitaplar == null)
            {
                return Problem("Entity set 'eKitapDbContext.Kitaplar'  is null.");
            }
            var book = await _context.Kitaplar.FirstOrDefaultAsync(c => c.Id == id);
            if (book != null)
            {
                book.IsDeleted = true;
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookExists(int id)
        {
            return _context.Kitaplar.Any(e => e.Id == id);
        }

        private async Task<string> SavePdfFile(IFormFile file, string path)
        {
            string fileName;
            do
            {
                fileName = Path.ChangeExtension(Guid.NewGuid().ToString("D"), Path.GetExtension(file.FileName));
            } while (System.IO.File.Exists(Path.Combine(path, fileName)));
            await using (var inputStream = new FileStream(Path.Combine(path, fileName), FileMode.Create))
            {
                // read file to stream
                await file.CopyToAsync(inputStream);
            }

            return fileName;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddChapter(string chapterName, int bookId)
        {
            var model = await _context.Kitaplar.FirstOrDefaultAsync(c => c.Id == bookId);
            if (model != null)
            {
                model.ChapterName = !string.IsNullOrEmpty(chapterName) ? chapterName : "[]";
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> EditChapter(int Id)
        {
            var book = await _context.Kitaplar.FirstOrDefaultAsync(c => c.Id == Id);
            if (book == null)
            {
                return NotFound();
            }
            return View("Chapter", book);
        }
    }
}
