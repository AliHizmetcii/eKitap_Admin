using eKitap.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace eKitap.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
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
            if ((!User.Identity?.IsAuthenticated ?? false))
                return NotFound();
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
                    CurrentPage = c.BookStudentConnections.Any(d => d.StudentId == studentId) ? c.BookStudentConnections.FirstOrDefault(d => d.StudentId == studentId)!.CurrentPage : 1,

                })
                .ToListAsync());
        }

        [HttpPost, Route("KitapGetir")]
        public async Task<IActionResult> Books(int Id)
        {
            if ((!User.Identity?.IsAuthenticated ?? false))
                return NotFound();
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


        [HttpPost, Route("KitapYorumla")]
        public async Task<IActionResult> UserComment([FromBody] PostComment data)
        {
            if ((!User.Identity?.IsAuthenticated ?? false))
                return NotFound();
            var userTc = User.Identity?.Name ?? "";
            int userId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.Id ?? -1;
            var bsConnection = await _db.BookStudentConnections.FirstOrDefaultAsync(c => c.BookId == data.Id && c.StudentId == userId && !c.IsDeleted);
            if (bsConnection == null)
                return NotFound();
            if (_db.Comment.Any(c => c.PageNo == data.PageNo && c.ConnectionId == data.Id))
            {
                var yorum = await _db.Comment.FirstOrDefaultAsync(c => c.PageNo == data.PageNo && c.ConnectionId == data.Id);
                if (yorum != null) yorum.Text = data.UserComment;
            }
            else
            {
                var yorum = new Comment
                {
                    IsDeleted = false,
                    LastUpdateDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    PageNo = data.PageNo,
                    Text = data.UserComment ?? "",
                    ConnectionId = bsConnection.Id,
                };
                await _db.Comment.AddAsync(yorum);
            }
            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, Route("KitapPuanla")]
        public async Task<IActionResult> UserRate([FromBody] PostRate data)
        {
            if ((!User.Identity?.IsAuthenticated ?? false))
                return NotFound();
            var userTc = User.Identity?.Name ?? "";
            int userId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.Id ?? -1;
            var bsConnection = await _db.BookStudentConnections.FirstOrDefaultAsync(c => c.BookId == data.Id && c.StudentId == userId && !c.IsDeleted);
            if (bsConnection == null)
                return NotFound();
            bsConnection.Rate = data.Rate;

            await _db.SaveChangesAsync();
            return Ok();
        }

        [HttpPost, Route("SaveLastPage")]
        public async Task<IActionResult> SaveLastPage(ChapterandPage cp)
        {
            if ((!User.Identity?.IsAuthenticated ?? false))
                return NotFound();
            var userTc = User.Identity?.Name ?? "";
            int userId = _db.Student.FirstOrDefault(c => c.TcId == userTc)?.Id ?? -1;
            var bsConnection = await _db.BookStudentConnections.FirstOrDefaultAsync(c => c.BookId == cp.Id && c.StudentId == userId && !c.IsDeleted);
            if (bsConnection == null)
                return NotFound();
            bsConnection.CurrentPage = cp.Page;
            await _db.SaveChangesAsync();

            return Ok((await _db.Kitaplar.Include(c => c.BookStudentConnections).ThenInclude(c=>c.Comments)
                    .Where(c => c.Id == cp.Id && !c.IsDeleted).ToListAsync())
                .Select(c => new
                {
                    c.ChapterName,
                    Comment = c.BookStudentConnections.FirstOrDefault(x => x.StudentId == userId)?.Comments
                        .FirstOrDefault(x => x.PageNo == cp.Page)?.Text ?? "",

                }).FirstOrDefault());
        }
        [HttpPost, Route("SaveNewStudent")]
        public async Task<IActionResult> SaveNewStudent([FromBody] NewStudent st)
        {
            if (await _db.Student.AnyAsync(c => c.TcId == st.TcId))
            {
                return NotFound(new { Success = false, Message = "Bu T.C numarası ile daha önce sisteme kayıt yapılmıştır. Lütfen giriş yapınız." });
            }
            var student = new Student();

            student.ApproveStatus = false;
            student.ClassRoomId = st.ClassRoomId;
            student.Name = st.Name;
            student.Password = st.Password;
            student.TcId = st.TcId;

            await _db.Student.AddAsync(student);
            await _db.SaveChangesAsync();

            return Ok(new { Success = false, Message = "Kaydınız yapılmıştır , lütfen onaylama işleminin tamamlanmasını bekleyiniz."});
        }

        [HttpGet, Route("returnClass")]
        public async Task<IActionResult> ReturnClass()
        {
            return Ok(await _db.ClassRoom.ToListAsync());
        }



    }

    public class PostComment
    {
        public int Id { get; set; }
        public int PageNo { get; set; }
        public string? UserComment { get; set; }
    }

    public class PostRate
    {
        public int Id { get; set; }
        public short Rate { get; set; }
    }
    public class ChapterandPage
    {
        public int Id { get; set; }
        public int Page { get; set; }
    }
    public class NewStudent
    {
        public string Name { get; set; }
        public string TcId { get; set; }
        public string Password { get; set; }
        public int ClassRoomId { get; set; }
        public bool ApproveStatus { get; set; }
    }




}
