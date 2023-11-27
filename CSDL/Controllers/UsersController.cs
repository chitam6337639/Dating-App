using CSDL.DTO;
using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using NuGet.Common;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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


        [HttpPut("update-image-url/{id}")]
        public async Task<IActionResult> UpdateImageUrl(int id, [FromBody] ImageURL imageURL)
        {
            if (imageURL == null)
            {
                return BadRequest("Invalid data format.");
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound("User not found.");
            }

            user.ImageURL = imageURL.ImageUrl;
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

        //[HttpPut("update-user/{id}")]
        //public async Task<IActionResult> UpdateInfoUser(int id, UserInfo updatedUser)
        //{
        //    if (id != updatedUser.userId)
        //    {
        //        return BadRequest();
        //    }

        //    var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.userId == id);

        //    if (existingUser == null)
        //    {
        //        return NotFound();
        //    }
        //    existingUser.gender = updatedUser.gender;
        //    existingUser.birthday = updatedUser.birthday;
        //    existingUser.lastName = updatedUser.lastName;
        //    existingUser.firstName = updatedUser.firstName;
        //    existingUser.location = updatedUser.location;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!UserExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}

        [HttpPut("update-user/{id}")]
        public async Task<IActionResult> UpdateInfoUser(int id, [FromBody] UserUpdate userUpdate)
        {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.userId == id);

            if (existingUser == null)
            {
                return NotFound();
            }

            // Cập nhật chỉ những trường bạn quan tâm
            existingUser.gender = userUpdate.gender;
            existingUser.birthday = userUpdate.birthday;
            existingUser.lastName = userUpdate.lastName;
            existingUser.firstName = userUpdate.firstName;
            existingUser.location = userUpdate.location;

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

        

        //[HttpGet("matched-users-with-latest-message")]
        //[Authorize]
        //public async Task<IActionResult> GetMatchedUsersWithLatestMessage()
        //{
        //    try
        //    {
        //        var accountIdClaim = User.FindFirst("AccountId");
        //        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
        //        {
        //            return Unauthorized("Token không hợp lệ.");
        //        }

        //        // Lấy danh sách các người dùng đã "match" với người dùng hiện tại
        //        var matchedUserIds = await _context.Relations
        //            .Where(r => r.UserID == currentAccountId && r.isMatch)
        //            .Select(r => r.OtherUserId)
        //            .ToListAsync();

        //        // Lấy thông tin chi tiết của các người dùng đã "match"
        //        var matchedUsers = await _context.Users
        //            .Where(u => matchedUserIds.Contains(u.userId))
        //            .ToListAsync();

        //        // Tạo một danh sách để lưu trữ thông tin về người dùng đã "match"
        //        var matchedUsersWithLatestMessages = new List<MatchedUserWithLatestMessageDTO>();
        //        foreach (var otherUser in matchedUsers)
        //        {
        //            var latestMessage = _context.Messages
        //                .Where(m => (m.UserIdFrom == currentAccountId && m.UserIdTo == otherUser.userId) || (m.UserIdFrom == otherUser.userId && m.UserIdTo == currentAccountId))
        //                .OrderByDescending(m => m.timeSent)
        //                .FirstOrDefault();

        //            // Lấy UserId của người gửi tin nhắn cuối cùng
        //            int? latestMessageUserIdFrom = null;
        //            if (latestMessage != null)
        //            {
        //                latestMessageUserIdFrom = latestMessage.UserIdFrom;
        //            }

        //            // Tạo một đối tượng DTO cho thông tin người dùng đã "match"
        //            var matchedUserWithLatestMessage = new MatchedUserWithLatestMessageDTO
        //            {
        //                userId = otherUser.userId,
        //                latestMessageContent = latestMessage?.content,
        //                latestMessageTimeSent = latestMessage?.timeSent,
        //                latestMessageUserIdFrom = latestMessageUserIdFrom,
        //                gender = otherUser.gender,
        //                imageURL = otherUser.ImageURL,
        //                bio = otherUser.bio,
        //                birthday = otherUser.birthday,
        //                lastName = otherUser.lastName,
        //                firstName = otherUser.firstName,
        //                location = otherUser.location,
        //                accessToken = otherUser.accessToken
        //            };

        //            matchedUsersWithLatestMessages.Add(matchedUserWithLatestMessage);
        //        }

        //        return Ok(matchedUsersWithLatestMessages);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Lỗi: " + ex.Message);
        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
        //        }

        //        return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
        //    }
        //}
        

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.userId == id);
        }


    }
}