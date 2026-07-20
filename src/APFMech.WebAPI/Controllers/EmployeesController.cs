using APFMech.Application.Employees.Queries.GetAllEmployees;
using APFMech.Application.Employees;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace APFMech.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EmployeesController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAllEmployeesQuery(), cancellationToken);
        return Ok(result);
    }
}