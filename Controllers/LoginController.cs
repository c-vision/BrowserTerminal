using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/login")]
public class LoginController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public IActionResult GetLoginInfo()
    {
        return Ok(new
        {
            message = "Use the POST method to authenticate",
            example = new { Username = "your-username", Password = "your-password" }
        });
    }

    [HttpPost]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        try
        {
            // Validazione input
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username and password are required." });
            }

            // Verifica se il file degli utenti esiste
            const string usersFile = "users.json";
            if (!System.IO.File.Exists(usersFile))
            {
                Console.WriteLine("Users file not found");
                return StatusCode(500, new { message = "Users file not found" });
            }

            // Leggi il file JSON
            var jsonContent = System.IO.File.ReadAllText(usersFile);
            var usersData = JsonSerializer.Deserialize<List<User>>(jsonContent);

            if (usersData == null || usersData.Count == 0)
            {
                Console.WriteLine("No users found in the file");
                return StatusCode(500, new { message = "No users found in the file" });
            }

            // Trova l'utente corrispondente (case-sensitive)
            var user = usersData.FirstOrDefault(u =>
                string.Equals(u.Username, request.Username, StringComparison.Ordinal) &&
                string.Equals(u.Password, request.Password, StringComparison.Ordinal)
            );

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Genera il token JWT
            var token = GenerateJwtToken(user);

            Console.WriteLine($"Login successful for user: {user.Name}");
            Console.WriteLine($"Generated Token: {token}");

            return Ok(new
            {
                message = "Login successful",
                token,
                name = user.Name,
                code = user.Code
            });
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing users file: {ex.Message}");
            return StatusCode(500, new { message = "Error deserializing users file", error = ex.Message });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
        }
    }

    private string GenerateJwtToken(User user)
    {
        var secretKey = configuration["Jwt:SecretKey"];
        var audience = configuration["Jwt:Audience"];
        var issuer = configuration["Jwt:Issuer"];

        if (string.IsNullOrEmpty(secretKey))
        {
            throw new Exception("Secret key is missing from configuration");
        }

        if (string.IsNullOrEmpty(audience))
        {
            throw new Exception("Audience is missing from configuration");
        }

        if (string.IsNullOrEmpty(issuer))
        {
            throw new Exception("Issuer is missing from configuration");
        }

        var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var key = System.Text.Encoding.ASCII.GetBytes(secretKey);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("Code", user.Code)
            ]),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Audience = audience, // Imposta l' audience
            Issuer = issuer, // Imposta l' issuer
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}