using CSDL.DTO;
using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CSDL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RelationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public RelationsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        [HttpPost("unmatch")]
        [Authorize]
        public IActionResult UnmatchUsers([FromBody] LikeDislikeRequestDto request)
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                // Tìm mối quan hệ giữa người dùng hiện tại và người dùng khác
                var relation1 = _context.Relations.FirstOrDefault(r => r.UserID == currentAccountId && r.OtherUserId == request.otherUserId);
                var relation2 = _context.Relations.FirstOrDefault(r => r.UserID == request.otherUserId && r.OtherUserId == currentAccountId);

                if (relation1 != null && relation2 != null)
                {
                    // Xóa mối quan hệ cả hai chiều
                    _context.Relations.Remove(relation1);
                    _context.Relations.Remove(relation2);
                    _context.SaveChanges();

                    return Ok("Đã unmatch người dùng thành công.");
                }
                else
                {
                    return NotFound("Mối quan hệ không tồn tại.");
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

        //[HttpGet("random-unmatched-users")]
        //[Authorize]
        //public async Task<IActionResult> GetRandomUnmatchedUsers()
        //{
        //    try
        //    {
        //        var accountIdClaim = User.FindFirst("AccountId");
        //        if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
        //        {
        //            return Unauthorized("Token không hợp lệ.");
        //        }

        //        // Lấy danh sách tất cả người dùng.
        //        var allUsers = await _context.Users.ToListAsync();

        //        // Lấy danh sách các người dùng đã "match," "like," hoặc "dislike" bởi người dùng hiện tại.
        //        var matchedLikedDislikedUsers = await _context.Relations
        //            .Where(r => r.UserID == currentAccountId && (r.isMatch || r.isLike || !r.isLike))
        //            .Select(r => r.OtherUserId)
        //            .ToListAsync();

        //        // Loại bỏ người dùng hiện tại khỏi danh sách người dùng đã từng "match," "like," hoặc "dislike.
        //        matchedLikedDislikedUsers.Add(currentAccountId);

        //        // Loại bỏ những người dùng đã từng "match," "like," hoặc "dislike" bởi người dùng hiện tại khỏi danh sách tất cả người dùng.
        //        var unmatchedUsers = allUsers.Where(u => !matchedLikedDislikedUsers.Contains(u.userId)).ToList();

        //        if (unmatchedUsers.Count < 3)
        //        {
        //            return NotFound("Không đủ người dùng chưa từng match, like, dislike để lấy.");
        //        }

        //        // Sử dụng hàm NEWID() trong truy vấn SQL để lấy ngẫu nhiên
        //        var randomUnmatchedUsers = unmatchedUsers
        //            .OrderBy(u => Guid.NewGuid())
        //            .Take(3)
        //            .ToList();

        //        return Ok(randomUnmatchedUsers);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("Lỗi: " + ex.Message);
        //        if (ex.InnerException is not null)
        //        {
        //            Console.WriteLine("Inner Exception: " + ex.InnerException.Message);
        //        }

        //        return StatusCode(500, "Đã xảy ra lỗi trong quá trình xử lý yêu cầu.");
        //    }
        //}
        [HttpGet("random-unmatched-users")]
        [Authorize]
        public async Task<IActionResult> GetRandomUnmatchedUsers()
        {
            try
            {
                var accountIdClaim = User.FindFirst("AccountId");
                if (accountIdClaim == null || !int.TryParse(accountIdClaim.Value, out int currentAccountId))
                {
                    return Unauthorized("Token không hợp lệ.");
                }

                // Lấy danh sách các người dùng đã "match," "like," hoặc "dislike" bởi người dùng hiện tại.
                var matchedLikedDislikedUsers = await _context.Relations
                    .Where(r => r.UserID == currentAccountId && (r.isMatch || r.isLike || !r.isLike))
                    .Select(r => r.OtherUserId)
                    .ToListAsync();

                // Loại bỏ người dùng hiện tại khỏi danh sách người dùng đã từng "match," "like," hoặc "dislike."
                matchedLikedDislikedUsers.Add(currentAccountId);

                // Lấy danh sách các người dùng chưa được like, match, dislike bởi người dùng hiện tại.
                var unmatchedUsers = await _context.Users
                    .Where(u => !matchedLikedDislikedUsers.Contains(u.userId))
                    .ToListAsync();

                if (unmatchedUsers.Count < 3)
                {
                    return NotFound("Không đủ người dùng chưa từng match, like, dislike để lấy.");
                }

                // Sử dụng hàm NEWID() trong truy vấn SQL để lấy ngẫu nhiên
                var randomUnmatchedUsers = unmatchedUsers
                    .OrderBy(u => Guid.NewGuid())
                    .Take(3)
                    .ToList();

                return Ok(randomUnmatchedUsers);
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
