//using CSDL.DTO;
//using CSDL.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
//using System.Security.Cryptography;
//using System.Text;

//namespace AuthenticationAndAuthorization.Controllers
//{

//    [ApiController]
//    public class LoginController : ControllerBase
//    {
//        private readonly ApplicationDbContext _context;

//        IConfiguration _configuration;

//        public LoginController(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }



//        [HttpPost("register")]
//        public async Task<IActionResult> Register(UserRegister request)
//        {
//            if (_context.Accounts.Any(u => u.email == request.email))
//            {
//                return BadRequest("User already exists. ");
//            }
//            CreatePasswordHash(request.password,
//                 out byte[] passwordHash,
//                 out byte[] passwordSalt);

//            var Account = new Account
//            {
//                email = request.email,
//                passwordHash = passwordHash,
//                passwordSalt = passwordSalt,
//                verificationToken = CreateRandomToken(),
//                status = "newUser"
//            };

//            _context.Accounts.Add(Account);
//            await _context.SaveChangesAsync();

//            return Ok("User successfully created!");
//        }

//        [HttpPost("login")]
//        public async Task<IActionResult> Login(UserLogin request)
//        {
//            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.email == request.email);
//            if (account == null)
//            {
//                return BadRequest("User not found.");
//            }

//            if (!VerifyPasswordHash(request.password, account.passwordHash, account.passwordSalt))
//            {
//                return BadRequest("Password is incorrect.");
//            }

//            if (account.verifiedAt == null)
//            {
//                return BadRequest("Not verified!");
//            }
//            var user = await _context.Users
//            .Where(u => u.accountId == account.accountId)
//            .Select(u => new
//            {
//                u.userId,
//                u.gender,
//                u.ImageURL,
//                u.bio,
//                u.birthday,
//                u.lastName,
//                u.firstName,
//                u.location,

//            })
//            .FirstOrDefaultAsync();

//            //return Ok($"Welcome back, {account.email}! :)");
//            return Ok(user);
//        }

//        public string getToken(User userData)
//        {
//            var claims = new[] {
//                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
//                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
//                        new Claim("UserId", userData.userId.ToString())
//            };

//            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
//            var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//            var token = new JwtSecurityToken(
//                _configuration["Jwt:Issuer"],
//                _configuration["Jwt:Audience"],
//                claims,
//                expires: DateTime.UtcNow.AddMinutes(10),
//                signingCredentials: signIn);


//            string Token = new JwtSecurityTokenHandler().WriteToken(token);

//            return Token;
//        }

//        [HttpPost("verify")]
//        public async Task<IActionResult> Verify(string token)
//        {
//            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.verificationToken == token);
//            if (account == null)
//            {
//                return BadRequest("Invalid token.");
//            }

//            account.verifiedAt = DateTime.Now;
//            await _context.SaveChangesAsync();

//            return Ok("User verified! :)");
//        }
//        [HttpPost("forgot-password")]
//        public async Task<IActionResult> ForgotPassword(string email)
//        {
//            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.email == email);
//            if (account == null)
//            {
//                return BadRequest("User not found.");
//            }

//            account.passwordResetToken = CreateRandomToken();
//            account.ResetTokenExpires = DateTime.Now.AddDays(1);
//            await _context.SaveChangesAsync();

//            return Ok("You may now reset your password.");
//        }
//        [HttpPost("reset-password")]
//        public async Task<IActionResult> ResettPassword(ResetPassword request)
//        {
//            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.passwordResetToken == request.token);
//            if (account == null || account.ResetTokenExpires < DateTime.Now)
//            {
//                return BadRequest("Invalid Token.");
//            }

//            CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

//            account.passwordHash = passwordHash;
//            account.passwordSalt = passwordSalt;
//            account.passwordResetToken = null;
//            account.ResetTokenExpires = null;

//            await _context.SaveChangesAsync();

//            return Ok("Password successfully reset.");
//        }



//        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
//        {
//            using (var hmac = new HMACSHA512(passwordSalt))
//            {
//                var computedHash = hmac
//                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
//                return computedHash.SequenceEqual(passwordHash);
//            }
//        }
//        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
//        {
//            using (var hmac = new HMACSHA512())
//            {
//                passwordSalt = hmac.Key;
//                passwordHash = hmac
//                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
//            }
//        }
//        private string CreateRandomToken()
//        {
//            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
//        }



//    }
//}

