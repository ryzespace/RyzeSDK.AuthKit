using Application.Services;
using Application.Services.Key;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Restful.Controllers;

[ApiController]
[Route(".well-known/jwks.json")]
public class JwksController : ControllerBase
{
    private readonly JwtKeyStore _keyStore;

    public JwksController(JwtKeyStore keyStore)
    {
        _keyStore = keyStore;
    }

    [HttpGet]
    public IActionResult GetJwks()
    {
        var jwks = new
        {
            keys = _keyStore.GetPublicJwks()
        };
        return Ok(jwks);
    }
}