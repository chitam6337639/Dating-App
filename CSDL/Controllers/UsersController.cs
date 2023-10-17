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
        private readonly IConfiguration _configuration;// Đảm bảo thay thế "YourDbContext" bằng tên DbContext thực sự của bạn.

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
                    accessToken = user.accessToken
                };

                return Ok(userInfo);
            }

            return BadRequest("User information not found.");
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

        //[HttpPost("like-or-dislike")]
        //[Authorize]
        //public IActionResult LikeOrDislikeUser([FromBody] LikeDislikeRequestDto request)
        //{
        //    try
        //    {
        //        var accountIdClaim = User.FindFirst("AccountId");
        //        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
        //        {
        //            return Unauthorized("Token không hợp lệ.");
        //        }

        //        var currentUser = _context.Users.FirstOrDefault(u => u.accountId == currentAccountId);
        //        if (currentUser == null)
        //        {
        //            return NotFound("Không tìm thấy người dùng.");
        //        }

        //        var otherUser = _context.Users.FirstOrDefault(u => u.userId == request.otherUserId);
        //        if (otherUser == null)
        //        {
        //            return NotFound("Có lỗi rồi hihi");
        //        }

        //        // Kiểm tra xem đã tồn tại mối quan hệ giữa người dùng hiện tại và người dùng khác hay chưa
        //        var relation = _context.Relations.FirstOrDefault(r => r.UserID == currentUser.userId && r.OtherUserId == otherUser.userId);

        //        if (relation == null)
        //        {
        //            var newRelation = new Relation
        //            {
        //                UserID = currentUser.userId,
        //                OtherUserId = otherUser.userId,
        //                isLike = request.isLike
        //            };
        //            _context.Relations.Add(newRelation);
        //            _context.SaveChanges();

        //            return Ok(request.isLike ? "Đã like người dùng thành công." : "Đã dislike người dùng thành công.");
        //        }
        //        else
        //        {
        //            relation.isLike = request.isLike;
        //            _context.SaveChanges();

        //            return Ok(request.isLike ? "Đã like người dùng thành công." : "Đã dislike người dùng thành công.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Ghi log thông tin lỗi, bao gồm Inner Exception
        //        Console.WriteLine("Lỗi: " + ex.Message);
        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
        //        }

        //        return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
        //    }
        //}

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
                // Ghi log thông tin lỗi, bao gồm Inner Exception
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
        }


        [HttpPost("check-match")]
        [Authorize]
        public IActionResult CheckMatch([FromBody] MatchRequestDto request)
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

                var otherUser = _context.Users.FirstOrDefault(u => u.userId == request.OtherUserId);
                if (otherUser == null)
                {
                    return NotFound("Người dùng khác không tồn tại.");
                }

                // Kiểm tra xem đã tồn tại mối quan hệ Match giữa người dùng hiện tại và người dùng khác hay chưa
                var existingMatch = _context.Matches.FirstOrDefault(m =>
                    (m.UserId == currentUser.userId && m.TargetUserId == otherUser.userId) ||
                    (m.UserId == otherUser.userId && m.TargetUserId == currentUser.userId));

                if (existingMatch == null)
                {
                    // Nếu chưa tồn tại mối quan hệ Match, thêm mối quan hệ mới
                    var newMatch = new Match
                    {
                        UserId = currentUser.userId,
                        TargetUserId = otherUser.userId,
                        time = request.time,
                        IsMatch = false // Khởi tạo với giá trị IsMatch là false
                    };
                    _context.Matches.Add(newMatch);
                    _context.SaveChanges();

                    return Ok("Chưa có match giữa hai người dùng.");
                }
                else
                {
                    // Nếu mối quan hệ Match đã tồn tại, kiểm tra xem IsMatch có phải là true hay không
                    if (existingMatch.IsMatch)
                    {
                        return Ok("Hai người dùng đã match.");
                    }
                    else
                    {
                        return Ok("Chưa có match giữa hai người dùng.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi log thông tin lỗi, bao gồm Inner Exception
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException != null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
        }
        //[HttpPost("DislikeUser/{dislikedUserId}")]
        //[Authorize] // Yêu cầu xác thực bằng JWT Token
        //public async Task<IActionResult> DislikeUser(int dislikedUserId)
        //{
        //    try
        //    {
        //        // Lấy thông tin người dùng hiện tại từ JWT Token
        //        var accountIdClaim = User.FindFirst("AccountId");
        //        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
        //        {
        //            return Unauthorized("Token không hợp lệ.");
        //        }

        //        // Tìm người dùng hiện tại dựa trên accountId
        //        var currentUser = await _context.Users.FirstOrDefaultAsync(u => u.accountId == currentAccountId);
        //        if (currentUser == null)
        //        {
        //            return NotFound("Không tìm thấy người dùng.");
        //        }

        //        // Kiểm tra xem đã tồn tại mối quan hệ DislikedUser giữa người dùng hiện tại và dislikedUserId hay chưa
        //        var existingUserDislikedRelation = await _context.UserDislikedRelations
        //            .FirstOrDefaultAsync(udr => udr.UserID == currentUser.userId && udr.DislikedUserID == dislikedUserId);

        //        if (existingUserDislikedRelation == null)
        //        {
        //            // Nếu chưa tồn tại mối quan hệ DislikedUser, thêm mối quan hệ mới
        //            var userDislikedRelation = new UserDislikedRelation
        //            {
        //                UserID = currentUser.userId,
        //                DislikedUserID = dislikedUserId
        //            };
        //            _context.UserDislikedRelations.Add(userDislikedRelation);

        //            // Kiểm tra xem dislikedUserId đã tồn tại trong bảng DislikedUsers hay chưa
        //            var existingDislikedUser = await _context.DislikedUsers
        //                .FirstOrDefaultAsync(du => du.dislikedUserId == dislikedUserId);

        //            if (existingDislikedUser == null)
        //            {
        //                // Nếu dislikedUserId chưa tồn tại, thêm dislikedUserId vào bảng DislikedUsers
        //                var dislikedUser = new DislikedUser
        //                {
        //                    dislikedUserId = dislikedUserId
        //                };
        //                _context.DislikedUsers.Add(dislikedUser);
        //            }

        //            await _context.SaveChangesAsync();

        //            return Ok("Đã dislike người dùng thành công.");
        //        }
        //        else
        //        {
        //            // Nếu đã tồn tại mối quan hệ DislikedUser, trả về thông báo lỗi
        //            return BadRequest("Người dùng đã bị dislike trước đó.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log hoặc hiển thị thông báo lỗi cụ thể
        //        return StatusCode(500, $"Lỗi: {ex.Message}");
        //    }
        //}

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

        private string GenerateJwtToken(Account account)
        {
            var claims = new[]
            {
                new Claim("AccountId", account.accountId.ToString()),
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