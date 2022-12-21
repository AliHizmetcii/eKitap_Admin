using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using eKitap.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol;
using Microsoft.EntityFrameworkCore.Metadata.Internal;


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
        public IActionResult Index()
        {
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
                    new Claim(ClaimTypes.Role, "Admin"),
                    new Claim(ClaimTypes.Name, user.Name),
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
            var user = _db.Student.FirstOrDefault(c => c.TcId == lm.Username && c.Password == lm.Password&& !c.IsDeleted);
            if (user == null)
                return NotFound(new { Success = false, Message = "Kullanıcı Adı veya Şifre Hatalı" });
            if (!user.ApproveStatus)
                return NotFound(new { Success = false, Message = "Hesabınız onaylı değil. Daha sonra tekrar deneyin" });
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];
            var key = Encoding.ASCII.GetBytes
                (_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("Id", Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.Name),
                    new Claim(JwtRegisteredClaimNames.Jti,
                        Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(5),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials
                (new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha512Signature)
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtToken = tokenHandler.WriteToken(token);
            var stringToken = tokenHandler.WriteToken(token);
            return Ok(new { Success = true , DisplayName = user.Name, Token= stringToken });

        }
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("/");
        }
    }
}
