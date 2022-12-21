using eKitap.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace eKitap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Student", AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class KitaplarController : ControllerBase
    {
        private readonly eKitapDbContext _db;

        public KitaplarController(eKitapDbContext db)
        {
            _db = db;
        }
        [HttpGet, Route("TumKitaplar")]
        public async Task<IActionResult> GetBooks()
        {
            var userTc = User.Identity?.Name ?? "";
            int studentId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.Id ?? -1;
            int sinifId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.ClassRoomId ?? -1;
            return Ok(await _db.Kitaplar.Include(c => c.BookStudentConnections).Where(c => c.ClassRoomId == sinifId && !c.IsDeleted)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.PdfName,
                    Rate = c.BookStudentConnections.Where(d => d.Rate != null).Average(d => d.Rate),
                    RaterCount = c.BookStudentConnections.Count(d => d.Rate != null),
                    CurrentPage = c.BookStudentConnections.Any(d => d.StudentId == studentId) ? c.BookStudentConnections.FirstOrDefault(d => d.StudentId == studentId)!.CurrentPage : 1
                })
                .ToListAsync());
        }

        [HttpPost, Route("KitapGetir")]
        public async Task<IActionResult> Books(int Id)
        {
            var userTc = User.Identity?.Name ?? "";
            int userId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.Id ?? -1;
            var book = await _db.Kitaplar.FirstOrDefaultAsync(c => c.Id == Id && !c.IsDeleted);
            if (book == null)
                return NotFound();
            var bsConnection = await _db.BookStudentConnections.FirstOrDefaultAsync(c => c.BookId == Id && c.StudentId == userId && !c.IsDeleted);
            if (bsConnection == null)
            {
                await _db.BookStudentConnections.AddAsync(new BookStudentConnection
                {
                    BookId = Id,
                    StudentId = userId,
                    CreateDate = DateTime.Now,
                    LastUpdateDate = DateTime.Now,
                    CurrentPage = 1,
                    IsDeleted = false
                });
                await _db.SaveChangesAsync();
            }
            return Ok(book.PdfName);
        }
        [HttpPost, Route("KitapPuanla")]
        public async Task<IActionResult> Review([FromBody] PostData data)
        {
            var userTc = User.Identity?.Name ?? "";
            int userId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.Id ?? -1;
            var bsConnection = await _db.BookStudentConnections.FirstOrDefaultAsync(c => c.BookId == data.Id && c.StudentId == userId && !c.IsDeleted);
            if (bsConnection == null)
                return NotFound();
            bsConnection.Rate = data.Rate;
            bsConnection.Comment = data.Comment;
            await _db.SaveChangesAsync();
            return Ok();
        }
    }

    public class PostData
    {
        public int Id { get; set; }
        public string Comment { get; set; }
        public short Rate { get; set; }
    }
}
