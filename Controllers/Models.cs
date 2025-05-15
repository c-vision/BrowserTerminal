// ReSharper disable PropertyCanBeMadeInitOnly.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
namespace BackEnd.Controllers;

// Modello per la richiesta di login
public class LoginRequest(string username, string password)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}

// Modello per gli utenti
// ReSharper disable once ClassNeverInstantiated.Global
public class User(string username, string password, string name, string code)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
    public string Name { get; set; } = name;
    public string Code { get; set; } = code;
}
