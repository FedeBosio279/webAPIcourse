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
public async Task<IActionResult> GetRandomName(int? count = 1, string ipAddress = null)
{
    // Se ipAddress è nullo, usa l'IP remoto dell'utente dalla richiesta
    ipAddress = ipAddress ?? HttpContext.Connection.RemoteIpAddress?.ToString();
    
    // Passa ipAddress al metodo del servizio
    var result = await _nameService.GetRandomNameAsync(count.Value, ipAddress);
    
    return Content(result, "text/html");
}

}
/*HttpGet]
public IActionResult GetInfo(int count, string ipAddress = null)
{
    if (string.IsNullOrEmpty(ipAddress))
    {
        // Ottieni l'IP dell'utente che ha effettuato la richiesta
        ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    // Logica della tua API per usare count e ipAddress
    return Ok(new { Count = count, IpAddress = ipAddress });
}
}
*/
