using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/validate-token")]
[Authorize] // Richiede che l'utente sia autenticato tramite JWT
public class TokenController : ControllerBase
{
    [HttpPost]
    public IActionResult ValidateToken()
    {
        try
        {
            // Estrai informazioni dal contesto HTTP
            var name = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var code = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Code")?.Value;

            // Log dettagliati per i claim
            Console.WriteLine($"Extracted Claims: Name = {name}, Code = {code}");

            // Verifica che i claim richiesti siano presenti
            if (string.IsNullOrEmpty(name))
            {
                Console.WriteLine("Missing 'name' claim in token");
                return Unauthorized(new { message = "Missing 'name' claim in token" });
            }

            if (string.IsNullOrEmpty(code))
            {
                Console.WriteLine("Missing 'code' claim in token");
                return Unauthorized(new { message = "Missing 'code' claim in token" });
            }

            Console.WriteLine($"Token validated for user: {name}");
            return Ok(new { name, code });
        }
        catch (Exception ex)
        {
            // Gestione errori generici
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
        }
    }
}