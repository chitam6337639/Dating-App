//using AuthenticationAndAuthorization.AppModel;
//using AuthenticationAndAuthorization.DBModels;
//using CSDL.AppModel;
//using CSDL.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.IdentityModel.Tokens;
//using System.IdentityModel.Tokens.Jwt;
//using System.Security.Claims;
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



//        [HttpPost]
//        [Route("PostLoginDetails")]
//        public async Task<IActionResult> PostLoginDetails(String email, String password)
//        {   
//            if (email != null && password !=null)
//            {
//                var resultLoginCheck = _context.Accounts
//                    .Where(e => e.email == email && e.password == password)
//                    .FirstOrDefault();
//                if (resultLoginCheck == null)
//                {
//                    return BadRequest("Invalid Credentials");
//                }
//                else
//                {

//                    var claims = new[] {
//                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
//                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
//                        new Claim("Email", email)
//                    };


//                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
//                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//                    var token = new JwtSecurityToken(
//                        _configuration["Jwt:Issuer"],
//                        _configuration["Jwt:Audience"],
//                        claims,
//                        expires: DateTime.UtcNow.AddDays(7),
//                        signingCredentials: signIn);

//                    var query = from user in _context.Users
//                                join account in _context.Accounts on user.userId equals account.User.userId
//                                where account.email == email
//                                select new
//                                {
//                                    User = user,
//                                    Email = account.email,
//                                    AccessToken = account.accessToken,
//                                    AccountId = account.accountId
//                                };

//                    var result = query.FirstOrDefault(); // Lấy kết quả đầu tiên nếu có

//                    if (result != null)
//                    {
//                        // Truy cập dữ liệu từ các thuộc tính result tại đây
//                        int userId = result.User.userId;
//                        string email = result.Email;
//                        string accessToken = result.AccessToken;
//                        int accountId = result.AccountId;

//                        // Xử lý dữ liệu ở đây
//                    }

//                    _userData.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);

//                    return Ok(_userData);
//                }
//            }
//            else
//            {
//                return BadRequest("No Data Posted");
//            }
//        }



//    }
//}
//using CSDL.AppModel;
//using CSDL.Models;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.IdentityModel.Tokens;
//using System;
//using System.IdentityModel.Tokens.Jwt;
//using System.Linq;
//using System.Security.Claims;
//using System.Text;
//using System.Threading.Tasks;

//namespace CSDL.Controllers
//{
//    [ApiController]
//    public class LoginController : ControllerBase
//    {
//        ApplicationDbContext _context;
//        IConfiguration _configuration;

//        public LoginController(ApplicationDbContext context, IConfiguration configuration)
//        {
//            _context = context;
//            _configuration = configuration;
//        }

//        [HttpPost]
//        [Route("PostLoginDetails")]
//        public async Task<IActionResult> PostLoginDetails(string email, string password)
//        {
//            if (email != null && password != null)
//            {
//                var resultLoginCheck = _context.Accounts
//                    .Where(e => e.email == email && e.password == password)
//                    .FirstOrDefault();
//                if (resultLoginCheck == null)
//                {
//                    return BadRequest("Invalid Credentials");
//                }
//                else
//                {
//                    var claims = new[]
//                    {
//                        new Claim(JwtRegisteredClaimNames.Sub, _configuration["Jwt:Subject"]),
//                        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
//                        new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString()),
//                        new Claim("Email", email.ToString())
                       
//                    };


//                    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
//                    var signIn = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//                    var token = new JwtSecurityToken(
//                        _configuration["Jwt:Issuer"],
//                        _configuration["Jwt:Audience"],
//                        claims,
//                        expires: DateTime.UtcNow.AddDays(7),
//                        signingCredentials: signIn);


                   
//                    var query = from user in _context.Users
//                                join account in _context.Accounts on user.userId equals account.User.userId
//                                where account.email == email
//                                select new
//                                {
//                                    User = user,
//                                    Email = account.email,
//                                    AccessToken = account.accessToken,
//                                    AccountId = account.accountId
//                                };

//                    var result = query.FirstOrDefault(); // Lấy kết quả đầu tiên nếu có

//                    if (result != null)
//                    {
//                        // Truy cập dữ liệu từ các thuộc tính result tại đây
//                        int userId = result.User.userId;
//                        string userEmail = result.Email;
//                        string accessToken = new JwtSecurityTokenHandler().WriteToken(token);


//                        // Tạo đối tượng để chứa thông tin người dùng và token
//                        var userData = new
//                        {
//                            UserId = userId,
//                            Email = userEmail,
//                            AccessToken = accessToken,
//                        };

//                        // Trả về thông tin người dùng và token trong phản hồi
//                        return Ok(userData);
//                    }
//                    else
//                    {
//                        return BadRequest("User not found");
//                    }
//                }
//            }
//            else
//            {
//                return BadRequest("No Data Posted");
//            }
//        }
//    }
//}

