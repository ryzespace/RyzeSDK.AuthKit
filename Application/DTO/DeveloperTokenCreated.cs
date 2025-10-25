using Domain.Entities;

namespace Application.DTO;

public class DeveloperTokenCreated
{
    public string Jwt { get; set; } = null!;
    public DeveloperToken Token { get; set; } = null!;
}