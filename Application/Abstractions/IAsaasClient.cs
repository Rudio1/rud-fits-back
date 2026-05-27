namespace RudFitAI.Application.Abstractions;

public interface IAsaasClient
{
    Task<string> CreateCustomerAsync(
        string name,
        string email,
        string? cpfCnpj,
        CancellationToken cancellationToken);

    Task UpdateCustomerCpfCnpjAsync(
        string customerId,
        string cpfCnpj,
        CancellationToken cancellationToken);

    Task<AsaasCheckoutResult> CreateCreditCardSubscriptionAsync(
        string customerId,
        decimal value,
        string description,
        DateOnly nextDueDate,
        AsaasCreditCardDto creditCard,
        AsaasCreditCardHolderInfoDto holderInfo,
        CancellationToken cancellationToken);

    Task<AsaasCheckoutResult> CreatePixSubscriptionAsync(
        string customerId,
        decimal value,
        string description,
        DateOnly nextDueDate,
        CancellationToken cancellationToken);

    Task<string?> GetFirstSubscriptionPaymentIdAsync(
        string subscriptionId,
        CancellationToken cancellationToken);

    Task<AsaasCheckoutResult> CreatePaymentAsync(
        string customerId,
        decimal value,
        string description,
        DateOnly dueDate,
        string billingType,
        AsaasCreditCardDto? creditCard,
        AsaasCreditCardHolderInfoDto? holderInfo,
        CancellationToken cancellationToken);

    Task CancelSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken);
}
