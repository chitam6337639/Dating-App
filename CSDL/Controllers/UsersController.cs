using CSDL.DTO;
using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CSDL.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public UsersController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister request)
        {
            if (_context.Accounts.Any(u => u.email == request.email))
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
            var token = GenerateJwtToken(account);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.accountId == account.accountId);
            if (user != null)
            {
                user.accessToken = token;
                await _context.SaveChangesAsync();


                var response = new
                {
                    accountId = account.accountId, // Thêm accountId
                    userInfo = new UserInfo
                    {
                        userId = user.userId,
                        gender = user.gender,
                        ImageURL = user.ImageURL,
                        bio = user.bio,
                        birthday = user.birthday,
                        lastName = user.lastName,
                        firstName = user.firstName,
                        location = user.location,
                        accessToken = user.accessToken
                    }
                };

                return Ok(response);
            }

            return Ok(new { accountId = account.accountId,status = account.status });
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


        [HttpPost("like-or-dislike")]
        [Authorize]
        public IActionResult LikeOrDislikeUser([FromBody] LikeDislikeRequestDto request)
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var currentUser = _context.Users.FirstOrDefault(u => u.accountId == currentAccountId);
                if (currentUser == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                var otherUser = _context.Users.FirstOrDefault(u => u.userId == request.otherUserId);
                if (otherUser == null)
                {
                    return NotFound("Người dùng khác không tồn tại.");
                }

                // Kiểm tra xem đã tồn tại mối quan hệ giữa người dùng hiện tại và người dùng khác hay chưa
                var relation = _context.Relations.FirstOrDefault(r => r.UserID == currentUser.userId && r.OtherUserId == otherUser.userId);

                if (relation == null)
                {
                    var newRelation = new Relation
                    {
                        UserID = currentUser.userId,
                        OtherUserId = otherUser.userId,
                        isLike = request.isLike
                    };
                    _context.Relations.Add(newRelation);

                    if (request.isLike)
                    {
                        // Kiểm tra xem người dùng khác đã thích người dùng hiện tại hay chưa
                        var reverseRelation = _context.Relations.FirstOrDefault(r => r.UserID == otherUser.userId && r.OtherUserId == currentUser.userId);

                        if (reverseRelation != null && reverseRelation.isLike)
                        {
                            // Nếu cả hai đã thích nhau, đánh dấu là đã "match"
                            newRelation.isMatch = true;
                            reverseRelation.isMatch = true;
                        }
                    }

                    _context.SaveChanges();

                    return Ok(request.isLike ? "Đã like người dùng thành công." : "Đã dislike người dùng thành công.");
                }
                else
                {
                    relation.isLike = request.isLike;
                    _context.SaveChanges();

                    return Ok(request.isLike ? "Đã like người dùng thành công." : "Đã dislike người dùng thành công.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
        }

        [HttpPost("send-message")]
        [Authorize]
        public async Task<IActionResult> SendMessage(MessageDto messageRequest)
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.accountId == currentAccountId);
                if (currentUser == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.userId == messageRequest.OtherUserId);
                if (otherUser == null)
                {
                    return NotFound("Người dùng khác không tồn tại.");
                }

                var message = new Message
                {
                    content = messageRequest.Content,
                    timeSent = DateTime.Now,
                    status = "sent", 
                    UserTo = otherUser,
                    UserFrom = currentUser
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                return Ok("Tin nhắn đã được gửi.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
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

        [HttpPut("update-status")]
        public async Task<IActionResult> UpdateAccountStatus(int accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.accountId == accountId);

            if (account == null)
            {
                return NotFound("Account not found.");
            }

            account.status = "oldUser";

            // Kiểm tra xem trạng thái đã được cập nhật thành công hay chưa
            try
            {
                await _context.SaveChangesAsync();
                return Ok($"Account status updated to: oldUser");
            }
            catch (DbUpdateConcurrencyException)
            {
                return StatusCode(500, "Đã xảy ra lỗi trong quá trình cập nhật trạng thái.");
            }
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

        [HttpGet("get-messages")]
        [Authorize]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.accountId == currentAccountId);
                if (currentUser == null)
                {
                    return NotFound("Không tìm thấy người dùng.");
                }

                var otherUser = await _context.Users.FirstOrDefaultAsync(u => u.userId == otherUserId);
                if (otherUser == null)
                {
                    return NotFound("Người dùng khác không tồn tại.");
                }

                var messages = await _context.Messages
                    .Where(m =>
                        (m.UserIdFrom == currentUser.userId && m.UserIdTo == otherUser.userId) ||
                        (m.UserIdFrom == otherUser.userId && m.UserIdTo == currentUser.userId))
                    .OrderBy(m => m.timeSent)
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
        }

        [HttpGet("matched-users")]
        [Authorize]
        public async Task<IActionResult> GetMatchedUsers()
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                // Lấy danh sách các người dùng đã "match" với người dùng hiện tại
                var matchedUserIds = await _context.Relations
                    .Where(r => r.UserID == currentAccountId && r.isMatch)
                    .Select(r => r.OtherUserId)
                    .ToListAsync();

                // Lấy thông tin chi tiết của các người dùng đã "match"
                var matchedUsers = await _context.Users
                    .Where(u => matchedUserIds.Contains(u.userId))
                    .ToListAsync();

                return Ok(matchedUsers);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
        }

        [HttpGet("get-user/{accountId}")]
        public async Task<IActionResult> GetUser(int accountId)
        {
            var account = await _context.Accounts.FirstOrDefaultAsync(u => u.accountId == accountId);
            if (account == null)
            {
                return NotFound("Account not found.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.accountId == account.accountId);
            if (user == null)
            {
                return NotFound("User not found for the provided accountId.");
            }

            var accessToken = user.accessToken;

            var userInfo = new UserInfo
            {
                userId = user.userId,
                gender = user.gender,
                ImageURL = user.ImageURL,
                bio = user.bio,
                birthday = user.birthday,
                lastName = user.lastName,
                firstName = user.firstName,
                location = user.location,
                accessToken = accessToken,
            };

            return Ok(userInfo);
        }

        //[HttpGet("latest-message/{user1Id}/{user2Id}")]
        //[Authorize]
        //public IActionResult GetLatestMessage(int user1Id, int user2Id)
        //{
        //    var accountIdClaim = User.FindFirst("AccountId");
        //    if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
        //    {
        //        return Unauthorized("Token không hợp lệ.");
        //    }

        //    // Kiểm tra xem user1Id hoặc user2Id có khớp với accountId hiện tại hay không
        //    if (user1Id != currentAccountId && user2Id != currentAccountId)
        //    {
        //        return Unauthorized("Không có quyền truy cập tin nhắn này.");
        //    }

        //    // Lấy tin nhắn cuối cùng giữa hai người dùng
        //    var latestMessage = _context.Messages
        //        .Where(m => (m.UserIdFrom == user1Id && m.UserIdTo == user2Id) || (m.UserIdFrom == user2Id && m.UserIdTo == user1Id))
        //        .OrderByDescending(m => m.timeSent)
        //        .FirstOrDefault();

        //    if (latestMessage == null)
        //    {
        //        return NotFound("Không có tin nhắn nào giữa hai người dùng.");
        //    }

        //    // Trả về content, timeSent và UserId của người gửi tin nhắn cuối cùng
        //    var response = new
        //    {
        //        content = latestMessage.content,
        //        timeSent = latestMessage.timeSent,
        //        userId = latestMessage.UserIdFrom // UserId của người gửi tin nhắn cuối cùng
        //    };

        //    return Ok(response);
        //}

        [HttpGet("latest-message/{otherUserId}")]
        [Authorize]
        public IActionResult GetLatestMessage(int otherUserId)
        {
            var accountIdClaim = User.FindFirst("AccountId");
            if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
            {
                return Unauthorized("Token không hợp lệ.");
            }

            // Lấy tin nhắn cuối cùng giữa 2 người dùng
            var latestMessage = _context.Messages
                .Where(m => (m.UserIdFrom == currentAccountId && m.UserIdTo == otherUserId) || (m.UserIdFrom == otherUserId && m.UserIdTo == currentAccountId))
                .OrderByDescending(m => m.timeSent)
                .FirstOrDefault();

            if (latestMessage == null)
            {
                return NotFound("Không có tin nhắn nào giữa hai người dùng.");
            }

            var response = new
            {
                content = latestMessage.content,
                timeSent = latestMessage.timeSent,
                userId = latestMessage.UserIdFrom // UserId của người gửi cuối cùng
            };

            return Ok(response);
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
    }
}