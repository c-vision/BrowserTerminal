using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers;

[ApiController]
[Route("api/terminal")]
[Authorize] // Richiede che l'utente sia autenticato
public class TerminalController : ControllerBase
{
    [HttpPost("execute")]
    public IActionResult ExecuteCommand([FromBody] CommandRequest request)
    {
        try
        {
            // Estrai informazioni sull'utente dal contesto HTTP
            var username = HttpContext.User.Identity?.Name;

            // Log dettagliati dei claim presenti nel token
            var claims = HttpContext.User.Claims.ToList();
            Console.WriteLine("Extracted Claims:");
            foreach (var claim in claims)
            {
                Console.WriteLine($"Type: {claim.Type}, Value: {claim.Value}");
            }

            // Verifica che il claim 'username' sia presente
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Missing 'username' claim in token");
                return Unauthorized(new { message = "Invalid token claims: missing username" });
            }

            Console.WriteLine($"Command execution requested by user: {username}");

            // Esegui il comando
            var result = ProcessCommand(request.Command);
            return Ok(new { output = result });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
        }
    }

    private static string ProcessCommand(string command)
    {
        // Logica per elaborare i comandi
        return command.ToLower() switch
        {
            "help" => "Comandi disponibili: help, clear, logout, echo [message]",
            "clear" => "Terminal cleared. (Client-side)",
            "logout" => "Effettua il logout per uscire dal terminale.",
            _ => command.StartsWith("echo ")
                ? command[5..] // Restituisce il messaggio dopo "echo"
                : $"Comando non riconosciuto: {command}"
        };
    }
}

// Modello per la richiesta del comando
public class CommandRequest(string command)
{
    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public string Command { get; set; } = command;
}