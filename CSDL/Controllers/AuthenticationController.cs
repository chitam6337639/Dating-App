using CSDL.DTO;
using CSDL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CSDL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthenticationController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }
        //[HttpPost("register")]
        //public async Task<IActionResult> Register(UserRegister request)
        //{
        //    if (_context.Accounts.Any(u => u.email == request.email))
        //    {
        //        return BadRequest("User already exists. ");
        //    }
        //    CreatePasswordHash(request.password,
        //         out byte[] passwordHash,
        //         out byte[] passwordSalt);

        //    var Account = new Account
        //    {
        //        email = request.email,
        //        passwordHash = passwordHash,
        //        passwordSalt = passwordSalt,
        //        verificationToken = CreateRandomToken(),
        //        status = "newUser"
        //    };

        //    _context.Accounts.Add(Account);
        //    await _context.SaveChangesAsync();

        //    return Ok("User successfully created!");
        //}
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister request)
        {
            // Kiểm tra xem địa chỉ email có đúng định dạng không
            if (!IsValidEmail(request.email))
            {
                return BadRequest("Invalid email format.");
            }

            // Kiểm tra xem địa chỉ email có thuộc tên miền cụ thể không
            string allowedDomain = "sinhvien.hoasen.edu.vn";
            if (!IsEmailInDomain(request.email, allowedDomain))
            {
                return BadRequest("Invalid email domain.");
            }

            // Kiểm tra xem người dùng đã tồn tại hay chưa
            if (_context.Accounts.Any(u => u.email == request.email))
            {
                return BadRequest("User already exists.");
            }

            // Tiếp tục với quá trình đăng ký nếu thông tin hợp lệ
            CreatePasswordHash(request.password,
                out byte[] passwordHash,
                out byte[] passwordSalt);

            var Account = new Account
            {
                email = request.email,
                passwordHash = passwordHash,
                passwordSalt = passwordSalt,
                verificationToken = CreateRandomToken(),
                status = "newUser"
            };

            _context.Accounts.Add(Account);
            await _context.SaveChangesAsync();

            return Ok("User successfully created!");
        }
        //[HttpPost("register")]
        //public async Task<IActionResult> Register(UserRegister request)
        //{
        //    // Kiểm tra xem địa chỉ email có đúng định dạng không
        //    if (!IsValidEmail(request.email))
        //    {
        //        return BadRequest("Invalid email format.");
        //    }

        //    // Kiểm tra xem địa chỉ email có thuộc tên miền cụ thể không
        //    string allowedDomain = "sinhvien.hoasen.edu.vn";
        //    if (!IsEmailInDomain(request.email, allowedDomain))
        //    {
        //        return BadRequest("Invalid email domain.");
        //    }

        //    // Kiểm tra xem người dùng đã tồn tại hay chưa
        //    var existingAccount = await _context.Accounts.FirstOrDefaultAsync(u => u.email == request.email);
        //    if (existingAccount != null)
        //    {
        //        // Automatically verify the user if the email is not duplicated
        //        if (existingAccount.verifiedAt == null)
        //        {
        //            existingAccount.verifiedAt = DateTime.Now;
        //            await _context.SaveChangesAsync();
        //            return Ok("User automatically verified!");
        //        }
        //        else
        //        {
        //            return BadRequest("User already exists.");
        //        }
        //    }

        //    // Tiếp tục với quá trình đăng ký nếu thông tin hợp lệ
        //    CreatePasswordHash(request.password,
        //        out byte[] passwordHash,
        //        out byte[] passwordSalt);

        //    var Account = new Account
        //    {
        //        email = request.email,
        //        passwordHash = passwordHash,
        //        passwordSalt = passwordSalt,
        //        verificationToken = CreateRandomToken(),
        //        status = "newUser"
        //    };

        //    _context.Accounts.Add(Account);
        //    await _context.SaveChangesAsync();

        //    return Ok("User successfully created!");
        //}


        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.email == request.email);
            if (account == null)
            {
                return BadRequest("User not found.");
            }

            if (!VerifyPasswordHash(request.password, account.passwordHash, account.passwordSalt))
            {
                return BadRequest("Password is incorrect.");
            }

            //if (account.verifiedAt == null)
            //{
            //    return BadRequest("Not verified!");
            //}
            if (account.verifiedAt == null)
            {
                account.verifiedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }
            var token = GenerateJwtToken(account);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.accountId == account.accountId);
            if (user != null)
            {
                user.accessToken = token;
                await _context.SaveChangesAsync();

                //check age
                int? userAge = null;
                if (user.birthday != null)
                {
                    userAge = CalculateAge(user.birthday);
                }

                var response = new
                {
                    accountId = account.accountId,
                    userInfo = new UserInfo
                    {
                        userId = user.userId,
                        gender = user.gender,
                        ImageURL = user.ImageURL,
                        bio = user.bio,
                        birthday = user.birthday,
                        age = userAge,
                        lastName = user.lastName,
                        firstName = user.firstName,
                        location = user.location,
                        accessToken = user.accessToken
                    }
                };

                return Ok(response);
            }

            return Ok(new { accountId = account.accountId, status = account.status });
        }



        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.verificationToken == token);
            if (account == null)
            {
                return BadRequest("Invalid token.");
            }

            account.verifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok("User verified! :)");
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.email == email);
            if (account == null)
            {
                return BadRequest("User not found.");
            }

            account.passwordResetToken = CreateRandomToken();
            account.ResetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            return Ok("You may now reset your password.");
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResettPassword(ResetPassword request)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.passwordResetToken == request.token);
            if (account == null || account.ResetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token.");
            }

            CreatePasswordHash(request.password, out byte[] passwordHash, out byte[] passwordSalt);

            account.passwordHash = passwordHash;
            account.passwordSalt = passwordSalt;
            account.passwordResetToken = null;
            account.ResetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Password successfully reset.");
        }

        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser(CreateUser user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.accountId == user.accountId);
            if (account == null)
            {
                return NotFound("Account not found.");
            }


            var newUser = new User
            {
                accountId = user.accountId,
                gender = user.gender,
                lastName = user.lastName,
                firstName = user.firstName,
                birthday = user.birthday,
                location = user.location
            };
            newUser.accessToken = GenerateJwtToken(account);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            var response = new
            {
                accountId = newUser.accountId,
                gender = newUser.gender,
                lastName = newUser.lastName,
                firstName = newUser.firstName,
                birthday = newUser.birthday,
                location = newUser.location,
                accessToken = newUser.accessToken
            };

            return CreatedAtAction("CreateUser", new { id = newUser.userId }, response);
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.userId == id);
        }


        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computedHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computedHash.SequenceEqual(passwordHash);
            }
        }
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac
                    .ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
        private string CreateRandomToken()
        {
            return Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
        }

        private string GenerateJwtToken(Account account)
        {
            var claims = new[]
            {
                new Claim("AccountId", account.accountId.ToString()),
                //new Claim("Status", account.status),
                new Claim(ClaimTypes.Email, account.email),
                // Add additional claims as needed
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1), // Token expiration time
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // Hàm kiểm tra định dạng của email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Hàm kiểm tra xem email có thuộc tên miền cụ thể không
        private bool IsEmailInDomain(string email, string domain)
        {
            return email.EndsWith("@" + domain);
        }
        public static int? CalculateAge(DateTime? birthday)
        {
            if (birthday.HasValue)
            {
                var today = DateTime.Today;
                var age = today.Year - birthday.Value.Year;

                if (birthday.Value > today.AddYears(-age))
                {
                    age--;
                }

                return age;
            }

            return null;
        }







    }
}
