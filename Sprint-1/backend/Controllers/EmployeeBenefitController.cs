using backend.Interfaces;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class EmployeeBenefitController : ControllerBase
{
    private readonly IEmployeeBenefitService _service;

    public EmployeeBenefitController(IEmployeeBenefitService service)
    {
        _service = service;
    }

    [HttpGet("{employeeId}")]
    public async Task<IActionResult> GetSelected(int employeeId)
    {
        var selectedIds = await _service.GetSelectedBenefitIdsAsync(employeeId);
        return Ok(new { success = true, selectedBenefitIds = selectedIds });
    }

    [HttpPost]
    public async Task<IActionResult> Save([FromBody] EmployeeBenefit benefit)
    {
        if (benefit == null) return BadRequest(new { success = false, message = "Payload inválido" });
        var success = await _service.SaveSelectionAsync(benefit);
        return success ? Ok(new { success = true }) : BadRequest(new { success = false, message = "No puede seleccionar este beneficio" });
    }

    // GET api/EmployeeBenefit/can-select?employeeId=1&benefitId=2
    [HttpGet("can-select")]
    public async Task<IActionResult> CanSelect([FromQuery] int employeeId, [FromQuery] int benefitId)
    {
        if (employeeId <= 0 || benefitId <= 0)
            return BadRequest(new { success = false, message = "employeeId y benefitId son requeridos" });

        var can = await _service.CanEmployeeSelectBenefitAsync(employeeId, benefitId);
        return Ok(new { success = true, canSelect = can });
    }
}
