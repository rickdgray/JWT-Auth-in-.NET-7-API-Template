using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace App.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly BearerTokenOptions _bearerTokenOptions;
        private readonly ILogger<LoginController> _logger;

        public LoginController(UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            IOptions<BearerTokenOptions> bearerTokenOptions,
            ILogger<LoginController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _bearerTokenOptions = bearerTokenOptions.Value;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult> RegisterUser(RegisterRequest registerRequest)
        {
            if (registerRequest == null)
            {
                return BadRequest("Invalid register request");
            }

            var result = await _userManager.CreateAsync(registerRequest.User, registerRequest.Password);

            if (result.Succeeded)
            {
                return Ok();
            }

            return BadRequest("Invalid register request");
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login(LoginRequest loginRequest)
        {
            var user = await _userManager.FindByNameAsync(loginRequest.Email);
            if (user == null)
            {
                return BadRequest("Invalid credentials");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginRequest.Password, false);
            if (result.Succeeded)
            {
                //unix timestamps
                var iat = (int)(DateTime.UtcNow - DateTime.UnixEpoch).TotalSeconds;
                var exp = (int)(DateTime.UtcNow.AddMinutes(60) - DateTime.UnixEpoch).TotalSeconds;

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_bearerTokenOptions.SecretKey));
                var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
                var header = new JwtHeader(signingCredentials);

                var payload = new JwtPayload
                {
                    { "iss", _bearerTokenOptions.ValidIssuer },
                    { "sub", $"{user.UserName}" },
                    { "aud", _bearerTokenOptions.ValidAudience },
                    { "exp", exp },
                    { "iat", iat }
                };

                var token = new JwtSecurityToken(header, payload);
                var handler = new JwtSecurityTokenHandler();
                var tokenString = handler.WriteToken(token);

                return Ok(tokenString);
            }

            return BadRequest("Invalid credentials");
        }
    }
}