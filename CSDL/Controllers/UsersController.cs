using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            return Ok($"Welcome back, {account.email}! :)");
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

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            var users = await _context.Users
                .Include(u => u.Histories)
                .Include(u => u.Images)
                .Include(u => u.Matches)
                .Include(u => u.Messages)
                .ToListAsync();

            if (users == null || users.Count == 0)
            {
                return NotFound();
            }
            Console.WriteLine(Ok(users));
            return Ok(users);
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }
            Console.WriteLine(user);
            return user;
        }

        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.userId }, user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.userId)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

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

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.userId == id);
        }

    }


}
