using CSDL.AppModel;
using CSDL.DTO;
using CSDL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

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
                        var reverseRelation = _context.Relations.FirstOrDefault(r => r.UserID == otherUser.userId && r.OtherUserId == currentUser.userId);

                        if (reverseRelation != null && reverseRelation.isLike)
                        {
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

                var relation1 = _context.Relations.FirstOrDefault(r => r.UserID == currentAccountId && r.OtherUserId == request.otherUserId);
                var relation2 = _context.Relations.FirstOrDefault(r => r.UserID == request.otherUserId && r.OtherUserId == currentAccountId);

                if (relation1 != null && relation2 != null)
                {
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

                var matchedUserIds = await _context.Relations
                    .Where(r => r.UserID == currentAccountId && r.isMatch)
                    .Select(r => r.OtherUserId)
                    .ToListAsync();

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

                var matchedLikedDislikedUsers = await _context.Relations
                    .Where(r => r.UserID == currentAccountId && (r.isMatch || r.isLike || !r.isLike))
                    .Select(r => r.OtherUserId)
                    .ToListAsync();

                matchedLikedDislikedUsers.Add(currentAccountId);

                // Lấy danh sách người dùng chưa từng được lấy và chưa từng bị loại trước đó.
                var unmatchedUsers = await _context.Users
                    .Include(u => u.Account)
                    .Where(u => !matchedLikedDislikedUsers.Contains(u.userId))
                    .ToListAsync();

                //if (unmatchedUsers.Count < 3)
                //{
                //    return NotFound("Không đủ người dùng chưa từng match, like, dislike để lấy.");
                //}

                var randomUnmatchedUsersWithAge = unmatchedUsers
                    .OrderBy(u => Guid.NewGuid())
                    .Take(3)
                    .Select(u => new
                    {
                        userId = u.userId,
                        UserAccountId = u.Account?.accountId,
                        gender = u.gender,
                        imageURL = u.ImageURL,
                        bio = u.bio,
                        birthday = CalculateAge(u.birthday),
                        lastName = u.lastName,
                        firstName = u.firstName,
                        location = u.location,
                    })
                    .ToList();

                return Ok(randomUnmatchedUsersWithAge);
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
