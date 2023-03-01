using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebAPI.DAL;
using WebApiTestDb.Model;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserContext _context;
        private static readonly string _secretKey = "SuperSecretKeySuperSecretKeySuperSecretKey";
        private static readonly string _issuer = "AzharSyed";
        private static readonly string _audience = "CuriBio";

        public UserController(UserContext context)
        {
            _context = context;
            _context.Database.EnsureCreated();
        }

        //// GET: api/User
        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(int id, [FromHeader] string authorization)
        {
            // Validate the token from the Authorization header
            if (!ValidateToken(authorization))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRecord([FromBody] User user)
        {
            // Generate a token for the new record
            string token = GenerateToken(user);

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Return the token
            return Ok(new { Token = token });
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, User user, [FromHeader] string authorization)
        {
            // Validate the token from the Authorization header
            if (!ValidateToken(authorization))
            {
                return Unauthorized();
            }

            if (id != user.Id)
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

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id, [FromHeader] string authorization)
        {
            // Validate the token from the Authorization header
            if (!ValidateToken(authorization))
            {
                return Unauthorized();
            }

            var user = await _context.Users.FindAsync(id);
            user.DeletedAt = DateTimeOffset.Now;

            if (user == null)
            {
                return NotFound();
            }

            //permanently removes the record
            //_context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        private string GenerateToken(User user)
        {
            // Create claims for the record ID and a unique identifier
            Claim[] claims = new[]
            {
                new Claim("RecordId", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // Create a symmetric security key using the secret key
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));

            // Create a signing credential using the security key
            SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Create a JWT token with the claims and signing credentials
            JwtSecurityToken token = new JwtSecurityToken(
                issuer: _issuer,
                audience: _audience,
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials
            );

            // Serialize the token to a string and return it
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private bool ValidateToken(string authorization)
        {
            // Extract the token from the Authorization header
            string token = authorization?.Replace("Bearer ", string.Empty);

            // Return false if the token is null or empty
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            try
            {
                // Validate the token using the secret key
                TokenValidationParameters parameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = _issuer,
                    ValidateAudience = true,
                    ValidAudience = _audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                new JwtSecurityTokenHandler().ValidateToken(token, parameters, out SecurityToken validatedToken);
            }
            catch (Exception)
            {
                // Return false if the token is invalid
                return false;
            }

            // Return true if the token is valid
            return true;
        }
    }
}
