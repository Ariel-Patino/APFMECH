namespace APFMech.WebAPI.Contracts.WorkOrders;

public record WorkOrderResponse(
    Guid Id,
    string TrackingNumber,
    string Description,
    string Status,
    Guid? AssignedMechanicId,
    DateTime CreatedAtUtc);