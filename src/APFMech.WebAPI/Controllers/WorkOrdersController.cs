using APFMech.Application.WorkOrders.Commands.AssignMechanic;
using APFMech.Application.WorkOrders.Commands.CompleteWorkOrder;
using APFMech.Application.WorkOrders.Commands.CreateWorkOrder;
using APFMech.Application.WorkOrders.Queries.GetAllWorkOrders;
using APFMech.Application.WorkOrders.Queries.GetWorkOrderById;
using APFMech.WebAPI.Contracts.WorkOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APFMech.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class WorkOrdersController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<WorkOrderResponse>> Create([FromBody] CreateWorkOrderRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateWorkOrderCommand(request.Description), cancellationToken);
        var response = ToResponse(result);

        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<WorkOrderResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<WorkOrderResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllWorkOrdersQuery(), cancellationToken);
        var response = result.Select(ToResponse).ToList();

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetWorkOrderByIdQuery(id), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(result));
    }

    [HttpPut("{id:guid}/assign")]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponse>> AssignMechanic(Guid id, [FromBody] AssignMechanicRequest request, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new AssignMechanicCommand(id, request.MechanicId), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(result));
    }

    [HttpPut("{id:guid}/complete")]
    [ProducesResponseType(typeof(WorkOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WorkOrderResponse>> Complete(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteWorkOrderCommand(id), cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(ToResponse(result));
    }

    private static WorkOrderResponse ToResponse(WorkOrderDto dto) => new(
        dto.Id,
        dto.TrackingNumber,
        dto.Description,
        dto.Status,
        dto.AssignedMechanicId,
        dto.CreatedAtUtc);
}