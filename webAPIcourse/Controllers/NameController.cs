using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class NameController : ControllerBase
{
    private readonly NameService _nameService;

    public NameController(NameService nameService)
    {
        _nameService = nameService;
    }
    
    [HttpGet("RndName")]
    public async Task<IActionResult> GetRandomNameData(int? count = 1, string ipAddress = null)
    {
        ipAddress ??= HttpContext.Connection.RemoteIpAddress?.ToString();
        var result = await _nameService.GetRandomNameDataAsync(count.Value, ipAddress);
        return new JsonResult(result);
    }
}