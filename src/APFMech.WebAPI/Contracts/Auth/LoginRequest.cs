namespace APFMech.WebAPI.Contracts.Auth;

public record LoginRequest(string Email, string Password, bool RememberMe);