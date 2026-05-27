using Microsoft.Extensions.Options;
using RudFitAI.Application.Abstractions;
using RudFitAI.Application.DTOs.Subscriptions.Requests;
using RudFitAI.Application.DTOs.Subscriptions.Responses;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Subscriptions;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Subscriptions;

public sealed class SubscriptionService : ISubscriptionService
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IEntitlementService _entitlementService;
    private readonly IAsaasClient _asaasClient;
    private readonly SubscriptionDomainService _subscriptionDomainService;
    private readonly AsaasOptions _asaasOptions;

    public SubscriptionService(
        ISubscriptionRepository subscriptionRepository,
        IProfileRepository profileRepository,
        IEntitlementService entitlementService,
        IAsaasClient asaasClient,
        SubscriptionDomainService subscriptionDomainService,
        IOptions<AsaasOptions> asaasOptions)
    {
        _subscriptionRepository = subscriptionRepository;
        _profileRepository = profileRepository;
        _entitlementService = entitlementService;
        _asaasClient = asaasClient;
        _subscriptionDomainService = subscriptionDomainService;
        _asaasOptions = asaasOptions.Value;
    }

    public async Task<IReadOnlyList<SubscriptionPlanResponseDto>> GetActivePlansAsync(CancellationToken cancellationToken)
    {
        IReadOnlyList<SubscriptionPlan> plans =
            await _subscriptionRepository.GetActivePlansAsync(cancellationToken);

        List<SubscriptionPlanResponseDto> result = new(plans.Count);
        foreach (SubscriptionPlan plan in plans)
        {
            result.Add(MapPlan(plan));
        }

        return result;
    }

    public async Task<SubscriptionStatusResponseDto> GetStatusForUserAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        bool hasPremium = await _entitlementService.HasPremiumAsync(userId, cancellationToken);

        UserSubscription? subscription =
            await _subscriptionRepository.GetCurrentUserSubscriptionByUserIdAsync(userId, cancellationToken);

        if (subscription is null)
        {
            User? user = await _profileRepository.GetByIdWithProfileAsync(userId, cancellationToken);
            int freeUses = user?.UserProfile?.FreeScannerUsesCount ?? 0;

            return new SubscriptionStatusResponseDto
            {
                HasPremium = false,
                Status = SubscriptionStatus.None.ToString(),
                FreeScannerUsesCount = freeUses,
                FreeScannerUsesRemaining = Math.Max(
                    0,
                    SubscriptionDomainService.FreeScannerLifetimeLimit - freeUses)
            };
        }

        return new SubscriptionStatusResponseDto
        {
            HasPremium = hasPremium,
            Status = subscription.Status.ToString(),
            PlanCode = subscription.SubscriptionPlan.Code,
            PlanName = subscription.SubscriptionPlan.Name,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            BillingType = subscription.BillingType == BillingType.None
                ? null
                : subscription.BillingType.ToString()
        };
    }

    public async Task<StartCheckoutResponseDto> StartCardSubscriptionAsync(
        Guid userId,
        StartCardSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        (User user, SubscriptionPlan plan) = await ResolveUserAndPlanAsync(
            userId,
            request.PlanCode,
            SubscriptionPlanCodes.PremiumMonthly,
            cancellationToken);

        _subscriptionDomainService.EnsureRecurringPlan(plan);
        EnsureCardRequest(request);

        string customerId = await ResolveAsaasCustomerIdAsync(user, request.HolderCpfCnpj, cancellationToken);
        decimal value = plan.PriceCents / 100m;
        DateOnly nextDueDate = ResolveNextDueDate();

        UserSubscription userSubscription = new(Guid.NewGuid(), userId, plan.Id);
        await _subscriptionRepository.AddUserSubscriptionAsync(userSubscription, cancellationToken);

        AsaasCreditCardDto creditCard = new()
        {
            HolderName = request.HolderName.Trim(),
            Number = request.CardNumber.Trim(),
            ExpiryMonth = request.ExpiryMonth.Trim(),
            ExpiryYear = request.ExpiryYear.Trim(),
            Ccv = request.Ccv.Trim()
        };

        AsaasCreditCardHolderInfoDto holderInfo = new()
        {
            Name = request.HolderName.Trim(),
            Email = request.HolderEmail.Trim(),
            CpfCnpj = request.HolderCpfCnpj.Trim(),
            PostalCode = request.HolderPostalCode.Trim(),
            AddressNumber = request.HolderAddressNumber.Trim(),
            Phone = request.HolderPhone.Trim()
        };

        AsaasCheckoutResult checkout = await _asaasClient.CreateCreditCardSubscriptionAsync(
            customerId,
            value,
            plan.Name,
            nextDueDate,
            creditCard,
            holderInfo,
            cancellationToken);

        userSubscription.SetPendingCheckout(
            BillingType.CreditCard,
            customerId,
            checkout.SubscriptionId,
            checkout.PaymentId);

        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        return MapCheckoutResponse(userSubscription, plan.Code, checkout);
    }

    public async Task<StartCheckoutResponseDto> StartPixSubscriptionAsync(
        Guid userId,
        StartPixSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        (User user, SubscriptionPlan plan) = await ResolveUserAndPlanAsync(
            userId,
            request.PlanCode,
            SubscriptionPlanCodes.PremiumMonthly,
            cancellationToken);

        _subscriptionDomainService.EnsureRecurringPlan(plan);

        string customerId = await ResolveAsaasCustomerIdAsync(user, request.CpfCnpj, cancellationToken);
        decimal value = plan.PriceCents / 100m;
        DateOnly nextDueDate = ResolveNextDueDate();

        UserSubscription userSubscription = new(Guid.NewGuid(), userId, plan.Id);
        await _subscriptionRepository.AddUserSubscriptionAsync(userSubscription, cancellationToken);

        AsaasCheckoutResult checkout = await _asaasClient.CreatePixSubscriptionAsync(
            customerId,
            value,
            plan.Name,
            nextDueDate,
            cancellationToken);

        userSubscription.SetPendingCheckout(
            BillingType.Pix,
            customerId,
            checkout.SubscriptionId,
            checkout.PaymentId);

        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        return MapCheckoutResponse(userSubscription, plan.Code, checkout);
    }

    public async Task<StartCheckoutResponseDto> StartLifetimeSubscriptionAsync(
        Guid userId,
        StartLifetimeSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        (User user, SubscriptionPlan plan) = await ResolveUserAndPlanAsync(
            userId,
            SubscriptionPlanCodes.PremiumLifetime,
            SubscriptionPlanCodes.PremiumLifetime,
            cancellationToken);

        _subscriptionDomainService.EnsureOneTimePlan(plan);

        string billingType = string.IsNullOrWhiteSpace(request.BillingType)
            ? "PIX"
            : request.BillingType.Trim().ToUpperInvariant();

        if (billingType is not "PIX" and not "CREDIT_CARD")
        {
            throw new ArgumentException("BillingType deve ser PIX ou CREDIT_CARD.");
        }

        if (billingType == "PIX" && string.IsNullOrWhiteSpace(request.HolderCpfCnpj))
        {
            throw new ArgumentException("CPF ou CNPJ é obrigatório para pagamento via PIX.");
        }

        string customerId = await ResolveAsaasCustomerIdAsync(user, request.HolderCpfCnpj ?? string.Empty, cancellationToken);
        decimal value = plan.PriceCents / 100m;
        DateOnly dueDate = ResolveNextDueDate();

        UserSubscription userSubscription = new(Guid.NewGuid(), userId, plan.Id);
        await _subscriptionRepository.AddUserSubscriptionAsync(userSubscription, cancellationToken);

        AsaasCreditCardDto? creditCard = null;
        AsaasCreditCardHolderInfoDto? holderInfo = null;

        if (billingType == "CREDIT_CARD")
        {
            EnsureLifetimeCardRequest(request);
            creditCard = new AsaasCreditCardDto
            {
                HolderName = request.HolderName!.Trim(),
                Number = request.CardNumber!.Trim(),
                ExpiryMonth = request.ExpiryMonth!.Trim(),
                ExpiryYear = request.ExpiryYear!.Trim(),
                Ccv = request.Ccv!.Trim()
            };
            holderInfo = new AsaasCreditCardHolderInfoDto
            {
                Name = request.HolderName!.Trim(),
                Email = request.HolderEmail!.Trim(),
                CpfCnpj = request.HolderCpfCnpj!.Trim(),
                PostalCode = request.HolderPostalCode!.Trim(),
                AddressNumber = request.HolderAddressNumber!.Trim(),
                Phone = request.HolderPhone!.Trim()
            };
        }

        AsaasCheckoutResult checkout = await _asaasClient.CreatePaymentAsync(
            customerId,
            value,
            plan.Name,
            dueDate,
            billingType,
            creditCard,
            holderInfo,
            cancellationToken);

        BillingType domainBillingType = billingType == "CREDIT_CARD"
            ? BillingType.CreditCard
            : BillingType.Pix;

        userSubscription.SetPendingCheckout(
            domainBillingType,
            customerId,
            asaasSubscriptionId: null,
            checkout.PaymentId);

        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        return MapCheckoutResponse(userSubscription, plan.Code, checkout);
    }

    public async Task CancelCurrentSubscriptionAsync(Guid userId, CancellationToken cancellationToken)
    {
        UserSubscription? subscription =
            await _subscriptionRepository.GetCurrentUserSubscriptionByUserIdAsync(userId, cancellationToken);

        if (subscription is null)
        {
            throw new InvalidOperationException("Nenhuma assinatura encontrada para cancelar.");
        }

        if (subscription.SubscriptionPlan.Kind != PlanKind.Recurring)
        {
            throw new InvalidOperationException("Plano permanente não possui renovação para cancelar.");
        }

        if (string.IsNullOrWhiteSpace(subscription.AsaasSubscriptionId))
        {
            throw new InvalidOperationException("Assinatura sem vínculo Asaas para cancelamento.");
        }

        await _asaasClient.CancelSubscriptionAsync(subscription.AsaasSubscriptionId, cancellationToken);
        subscription.Cancel(DateTime.UtcNow);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task<(User User, SubscriptionPlan Plan)> ResolveUserAndPlanAsync(
        Guid userId,
        string planCode,
        string defaultPlanCode,
        CancellationToken cancellationToken)
    {
        User? user = await _profileRepository.GetByIdWithProfileAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new InvalidOperationException("Usuário não encontrado.");
        }

        string code = string.IsNullOrWhiteSpace(planCode) ? defaultPlanCode : planCode.Trim();
        _subscriptionDomainService.EnsurePlanCode(code);

        SubscriptionPlan? plan = await _subscriptionRepository.GetPlanByCodeAsync(code, cancellationToken);
        if (plan is null)
        {
            throw new ArgumentException("Plano de assinatura inválido.");
        }

        return (user, plan);
    }

    private async Task<string> ResolveAsaasCustomerIdAsync(
        User user,
        string cpfCnpj,
        CancellationToken cancellationToken)
    {
        string normalizedCpfCnpj = _subscriptionDomainService.NormalizeCpfCnpj(cpfCnpj);

        string? existingCustomerId =
            await _subscriptionRepository.GetLatestAsaasCustomerIdByUserIdAsync(user.Id, cancellationToken);

        if (!string.IsNullOrWhiteSpace(existingCustomerId))
        {
            await _asaasClient.UpdateCustomerCpfCnpjAsync(
                existingCustomerId,
                normalizedCpfCnpj,
                cancellationToken);

            return existingCustomerId;
        }

        return await _asaasClient.CreateCustomerAsync(
            user.Name,
            user.Email,
            normalizedCpfCnpj,
            cancellationToken);
    }

    private static void EnsureCardRequest(StartCardSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HolderName)
            || string.IsNullOrWhiteSpace(request.CardNumber)
            || string.IsNullOrWhiteSpace(request.ExpiryMonth)
            || string.IsNullOrWhiteSpace(request.ExpiryYear)
            || string.IsNullOrWhiteSpace(request.Ccv)
            || string.IsNullOrWhiteSpace(request.HolderEmail)
            || string.IsNullOrWhiteSpace(request.HolderCpfCnpj)
            || string.IsNullOrWhiteSpace(request.HolderPostalCode)
            || string.IsNullOrWhiteSpace(request.HolderAddressNumber)
            || string.IsNullOrWhiteSpace(request.HolderPhone))
        {
            throw new ArgumentException("Dados do cartão e do titular são obrigatórios.");
        }
    }

    private static void EnsureLifetimeCardRequest(StartLifetimeSubscriptionRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.HolderName)
            || string.IsNullOrWhiteSpace(request.CardNumber)
            || string.IsNullOrWhiteSpace(request.ExpiryMonth)
            || string.IsNullOrWhiteSpace(request.ExpiryYear)
            || string.IsNullOrWhiteSpace(request.Ccv)
            || string.IsNullOrWhiteSpace(request.HolderEmail)
            || string.IsNullOrWhiteSpace(request.HolderCpfCnpj)
            || string.IsNullOrWhiteSpace(request.HolderPostalCode)
            || string.IsNullOrWhiteSpace(request.HolderAddressNumber)
            || string.IsNullOrWhiteSpace(request.HolderPhone))
        {
            throw new ArgumentException("Dados do cartão e do titular são obrigatórios para pagamento com cartão.");
        }
    }

    private static StartCheckoutResponseDto MapCheckoutResponse(
        UserSubscription userSubscription,
        string planCode,
        AsaasCheckoutResult checkout)
    {
        return new StartCheckoutResponseDto
        {
            UserSubscriptionId = userSubscription.Id,
            PlanCode = planCode,
            Status = userSubscription.Status.ToString(),
            AsaasSubscriptionId = checkout.SubscriptionId,
            AsaasPaymentId = checkout.PaymentId,
            InvoiceUrl = checkout.InvoiceUrl,
            PixQrCodeBase64 = checkout.PixQrCodeBase64,
            PixCopiaECola = checkout.PixCopiaECola
        };
    }

    private DateOnly ResolveNextDueDate()
    {
        int dueDays = _asaasOptions.DefaultDueDays <= 0 ? 3 : _asaasOptions.DefaultDueDays;
        return DateOnly.FromDateTime(DateTime.UtcNow.AddDays(dueDays));
    }

    private static SubscriptionPlanResponseDto MapPlan(SubscriptionPlan plan)
    {
        return new SubscriptionPlanResponseDto
        {
            Code = plan.Code,
            Name = plan.Name,
            PriceCents = plan.PriceCents,
            Interval = plan.Interval.ToString(),
            Kind = plan.Kind.ToString()
        };
    }
}
