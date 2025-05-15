``` markdown
# BackEnd API - Auth System

Un'API REST realizzata in ASP.NET Core per la gestione dell'autenticazione tramite credenziali. L'API consente agli utenti di autenticarsi e ricevere un token JWT per l'accesso ai servizi protetti.

## Funzionalità

- **GET /api/login**: Fornisce informazioni sull'autenticazione.
- **POST /api/login**: Permette di autenticarsi fornendo username e password.
- Generazione di token JWT per gli utenti autenticati.

## Struttura del progetto

### LoginController

- Gestisce tutte le funzionalità dedicate all'autenticazione degli utenti.
- **Metodo `GetLoginInfo`**: Guida gli utenti sull'uso corretto del metodo di autenticazione.
- **Metodo `Login`**:
  - Effettua la validazione delle credenziali.
  - Legge e verifica i dati degli utenti da un file JSON (`users.json`).
  - Genera un token JWT in caso di successo.
  - Gestisce eventuali errori, come file non trovati, errori nel file JSON o altri problemi.

### Configurazione JWT

La generazione del token JWT richiede:
- Una **chiave segreta** (`Jwt:SecretKey`).
- Un ** audience** (`Jwt:Audience`).
- Un **issuer** (`Jwt:Issuer`).

Questi valori devono essere configurati nel file di configurazione dell'applicazione (ad esempio, `appsettings.json`).

Esempio di configurazione in `appsettings.json`:
```
json { "Jwt": { "SecretKey": "la-tua-chiave-segreta", "Issuer": "il-tuo-issuer", "Audience": "il-tuo-audience" } }
``` 

## Requisiti

- .NET 9.0
- File `users.json` contenente gli utenti in formato JSON.

Esempio del file `users.json`:
```
json [ {
latex_unknown_tag
``` 

## Installazione e utilizzo

1. Clona il repository:
   ```bash
   git clone <repository-url>
   cd <repository-directory>
   ```

2. Configura il file `appsettings.json` con le tue impostazioni JWT.

3. Crea un file `users.json` nella directory principale e inserisci i dati degli utenti.

4. Compila ed esegui l'applicazione:
   ```bash
   dotnet build
   dotnet run
   ```

5. Testa l'API usando strumenti come **Postman** o **cURL**.

## Esempi di richieste

### 1. Ottenere informazioni sull'autenticazione
**Richiesta:**
```
http GET /api/login HTTP/1.1 Host: localhost:5000
``` 

**Risposta:**
```
json { "message": "Use the POST method to authenticate", "example": { "Username": "your-username", "Password": "your-password" } }
``` 

### 2. Autenticazione con credenziali
**Richiesta:**
```
http POST /api/login HTTP/1.1 Host: localhost:5000 Content-Type: application/json
{ "Username": "admin", "Password": "password123" }
``` 

**Risposta - Successo:**
```
json { "message": "Login successful", "token": "eyJhbGciOiJIUzI1NiIsInR5c...<token_troncato>", "name": "Admin User", "code": "001" }
``` 

**Risposta - Credenziali non valide:**
```
json { "message": "Invalid username or password" }
``` 

**Risposta - File utenti mancante:**
```
json { "message": "Users file not found" }
``` 

## Strumenti utilizzati

- **ASP.NET Core**: Framework utilizzato per realizzare l'applicazione web.
- **JWT (JSON Web Token)**: Per la generazione dei token di autenticazione.
- **C# 13.0**: Linguaggio utilizzato per scrivere il codice backend.
- **File JSON**: Per salvare le informazioni degli utenti.

## Possibili miglioramenti futuri

1. **Sicurezza migliorata**:
    - Hashing delle password degli utenti.
    - Crittografia del file `users.json`.

2. **Funzionalità estese**:
    - Endpoint per registrazione nuovi utenti.
    - Integrazione con un database anziché con un file JSON.

3. **Logger**:
    - Integrazione di un sistema di logging come Serilog per tracciare errori e attività dell'utente.

## Contributi

Contributi e suggerimenti sono i benvenuti! Apri pure una pull request o crea una issue per discuterne.

## Licenza

Questo progetto è sotto licenza MIT. Consulta il file [LICENSE](LICENSE) per ulteriori informazioni.
```
