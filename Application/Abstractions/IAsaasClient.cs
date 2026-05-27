namespace RudFitAI.Application.Abstractions;

public interface IAsaasClient
{
    Task<string> CreateCustomerAsync(
        string name,
        string email,
        string? cpfCnpj,
        CancellationToken cancellationToken);
}
