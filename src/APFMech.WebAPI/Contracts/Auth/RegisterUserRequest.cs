namespace APFMech.WebAPI.Contracts.Auth;

public record RegisterUserRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName);