using CSDL.DTO;
using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSDL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public MessagesController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
        [HttpGet("matched-users-with-latest-message")]
        [Authorize]
        public async Task<IActionResult> GetMatchedUsersWithLatestMessage()
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                var currentUser = _context.Users.FirstOrDefault(u => u.accountId == currentAccountId);

                // Lấy danh sách các người dùng đã "match" với người dùng hiện tại
                var matchedUserIds = await _context.Relations
                    .Where(r => r.UserID == currentUser.userId && r.isMatch)
                    .Select(r => r.OtherUserId)
                    .ToListAsync();

                // Lấy thông tin chi tiết của các người dùng đã "match"
                var matchedUsers = await _context.Users
                    .Where(u => matchedUserIds.Contains(u.userId))
                    .ToListAsync();

                // Tạo một danh sách để lưu trữ thông tin về người dùng đã "match" bao gồm accountId
                var matchedUsersWithLatestMessages = new List<MatchedUserWithLatestMessageDTO>();
                foreach (var otherUser in matchedUsers)
                {
                    var latestMessage = await _context.Messages
                        .Where(m => (m.UserIdFrom == currentUser.userId && m.UserIdTo == otherUser.userId) || (m.UserIdFrom == otherUser.userId && m.UserIdTo == currentUser.userId))
                        .OrderByDescending(m => m.timeSent)
                        .FirstOrDefaultAsync();

                    // Lấy UserId của người gửi tin nhắn cuối cùng
                    int? latestMessageUserIdFrom = latestMessage?.UserIdFrom;

                    // Lấy accountId từ đối tượng Account tương ứng của otherUser
                    int? otherUserAccountId = await _context.Users
                        .Where(u => u.userId == otherUser.userId)
                        .Select(u => u.Account.accountId)
                        .FirstOrDefaultAsync();

                    // Tạo một đối tượng DTO cho thông tin người dùng đã "match" bao gồm accountId
                    var matchedUserWithLatestMessage = new MatchedUserWithLatestMessageDTO
                    {
                        userId = otherUser.userId,
                        accountId = (int)otherUserAccountId, // accountId từ đối tượng Account
                        latestMessageContent = latestMessage?.content,
                        latestMessageTimeSent = latestMessage?.timeSent,
                        latestMessageUserIdFrom = latestMessageUserIdFrom,
                        gender = otherUser.gender,
                        imageURL = otherUser.ImageURL,
                        bio = otherUser.bio,
                        birthday = otherUser.birthday,
                        lastName = otherUser.lastName,
                        firstName = otherUser.firstName,
                        location = otherUser.location,
                        accessToken = otherUser.accessToken
                    };

                    matchedUsersWithLatestMessages.Add(matchedUserWithLatestMessage);
                }

                return Ok(matchedUsersWithLatestMessages);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi: " + ex.Message);
                if (ex.InnerException is not null)
                {
                    Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
                }

                return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
            }
        }


    }
}
