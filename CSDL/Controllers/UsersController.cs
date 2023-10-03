using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NuGet.Common;
using System.Security.Cryptography;

namespace CSDL.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // Đảm bảo thay thế "YourDbContext" bằng tên DbContext thực sự của bạn.

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister request)
        {
            if(_context.Accounts.Any(u =>u.email == request.email)) 
            {
                return BadRequest("User already exists. ");
            }
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

            if (account.verifiedAt == null)
            {
                return BadRequest("Not verified!");
            }
            var user = await _context.Users
        .Where(u => u.accountId == account.accountId)
        .Select(u => new
        {
            u.userId,
            u.gender,
            u.ImageURL,
            u.bio,
            u.birthday,
            u.lastName,
            u.firstName,
            u.location
        })
        .FirstOrDefaultAsync();

            //return Ok($"Welcome back, {account.email}! :)");
            return Ok(user);
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

            var newUser = new User
            {
                accountId = user.accountId,
                gender = user.gender,
                lastName = user.lastName,
                firstName = user.firstName,
                birthday = user.birthday,
                location = user.location
                // Assign other properties as needed
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction("CreateUser", new { id = newUser.userId }, user);
        }

        [HttpPut("update-image-url/{id}")]
        public async Task<IActionResult> UpdateImageUrl(int id, [FromBody] string imageUrl)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.ImageURL = imageUrl;
            await _context.SaveChangesAsync();

            return Ok("Image URL updated successfully.");
        }

        [HttpPut("update-bio/{id}")]
        public async Task<IActionResult> UpdateBio(int id, [FromBody] string bio)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.bio = bio;
            await _context.SaveChangesAsync();

            return Ok("Bio updated successfully.");
        }

        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateInfoUser(int id, UserInfo updatedUser)
        {
            if (id != updatedUser.userId)
            {
                return BadRequest();
            }

            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.userId == id);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Cập nhật thông tin người dùng
            existingUser.gender = updatedUser.gender;
            existingUser.birthday = updatedUser.birthday;
            existingUser.lastName = updatedUser.lastName;
            existingUser.firstName = updatedUser.firstName;
            existingUser.location = updatedUser.location;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpGet("randomusers")]
        public async Task<IActionResult> GetRandomUsers()
        {
            // Sử dụng hàm NEWID() trong truy vấn SQL để lấy ngẫu nhiên
            var randomUsers = await _context.Users
                .OrderBy(u => Guid.NewGuid())
                .Take(3)
                .ToListAsync();

            return Ok(randomUsers);
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






      

    }


}
