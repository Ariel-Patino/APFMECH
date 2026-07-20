namespace APFMech.WebAPI.Contracts.Auth;

public record AuthMeResponse(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles,
    Guid? EmployeeId,
    bool HasEmployeeProfile);