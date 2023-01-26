using System.Security.Claims;
using eKitap.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace eKitap.Controllers
{
    public class HomeController : Controller
    {
        private readonly eKitapDbContext _db;
        private readonly IConfiguration _configuration;

        public HomeController(eKitapDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            ViewBag.BookCount = await _db.Kitaplar.Where(c => !c.IsDeleted).CountAsync();
            ViewBag.StudentsCount = await _db.Student.Where(c => !c.IsDeleted).CountAsync();
            ViewBag.NotApprovedStudents = await _db.Student.Where(c => !c.IsDeleted && !c.ApproveStatus).CountAsync();
            ViewBag.LastComments = await _db.Comment.Include(c=>c.Connection).ThenInclude(c=>c.Student).Include(c=>c.Connection).ThenInclude(c=>c.Book).Where(c => !c.IsDeleted).AsNoTracking().OrderByDescending(c=>c.LastUpdateDate).Take(5)
                .Select(c=>new
                {
                    StudentName=c.Connection.Student.Name,
                    BookName=c.Connection.Book.Name,
                    Comment=c.Text,
                    c.PageNo
                }).ToListAsync();
            return View();

        }
        public class LoginModel
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }
        [HttpGet]
        public async Task<IActionResult> Login()
        {

            return View(model: "");
        }

        [HttpPost]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> Login(string username, string password, string returnurl = "/")
        {
            try
            {
                var user = _db.AdminUsers.FirstOrDefault(c => c.Name == username && c.Password == password);
                if (user == null)
                    return View(model: "Kullanıcı Adı veya Şifre Hatalı");
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Role, "Admin"),
                    new(ClaimTypes.Name, user.Name),
                };

                var claimsIdentity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var authProperties = new AuthenticationProperties
                {
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);
            }
            catch (Exception e)
            {
                return Content(e.ToString());
            }
            return Redirect(returnurl);
        }

        [HttpPost]
        [Consumes("application/json")]
        public async Task<IActionResult> Login([FromBody] LoginModel lm)
        {
            var user = _db.Student.FirstOrDefault(c => c.TcId == lm.Username && c.Password == lm.Password && !c.IsDeleted);
            if (user == null)
                return NotFound(new { Success = false, Message = "Kullanıcı Adı veya Şifre Hatalı" });
            if (!user.ApproveStatus)
                return NotFound(new { Success = false, Message = "Hesabınız onaylı değil. Daha sonra tekrar deneyin" });
            var claims = new List<Claim>
            {
                new(ClaimTypes.Role, "Student"),
                new(ClaimTypes.Name, user.TcId),
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
            return Ok(new { Success = true, DisplayName = user.Name });

        }

        [HttpPost]
        public async Task<IActionResult> SignIn(Student student)
        {
            Student st = new Student();
            st.ClassRoom = student.ClassRoom;
            st.TcId = student.TcId;
            st.ApproveStatus = false;
            st.Name = student.Name;
            st.Password = student.Password;

            await _db.Student.AddAsync(st);
            await _db.SaveChangesAsync();



            return Ok(new { message = "Onay bekleniyor ..." });
        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}
