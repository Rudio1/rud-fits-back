using Microsoft.Extensions.Options;
using RudFitAI.Application.DTOs.Friendships.Requests;
using RudFitAI.Application.DTOs.Friendships.Responses;
using RudFitAI.Application.DTOs.Meals.Responses;
using RudFitAI.Application.Options;
using RudFitAI.Application.Services.Interfaces.Friendships;
using RudFitAI.Application.Services.Interfaces.Meals;
using RudFitAI.Application.Time;
using RudFitAI.Domain.DomainServices;
using RudFitAI.Domain.Entities;
using RudFitAI.Domain.Enums;
using RudFitAI.Domain.Repositories;

namespace RudFitAI.Application.Services.Friendships;

public sealed class FriendshipService : IFriendshipService
{
    private const int ProgressScale = 2;

    private readonly FriendshipDomainService _friendshipDomainService;
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IMealLogService _mealLogService;
    private readonly IProfileRepository _profileRepository;
    private readonly FriendshipOptions _friendshipOptions;
    private readonly PersistenceOptions _persistenceOptions;

    public FriendshipService(
        FriendshipDomainService friendshipDomainService,
        IFriendshipRepository friendshipRepository,
        IMealLogService mealLogService,
        IProfileRepository profileRepository,
        IOptions<FriendshipOptions> friendshipOptions,
        IOptions<PersistenceOptions> persistenceOptions)
    {
        _friendshipDomainService = friendshipDomainService;
        _friendshipRepository = friendshipRepository;
        _mealLogService = mealLogService;
        _profileRepository = profileRepository;
        _friendshipOptions = friendshipOptions.Value;
        _persistenceOptions = persistenceOptions.Value;
    }

    public async Task<FriendInviteLinkResponseDto> GetOrCreateInviteLinkAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        FriendInviteToken? inviteToken =
            await _friendshipRepository.GetInviteTokenByUserIdAsync(userId, cancellationToken);

        if (inviteToken is null)
        {
            inviteToken = new FriendInviteToken(
                Guid.NewGuid(),
                userId,
                _friendshipDomainService.GenerateToken());

            await _friendshipRepository.AddInviteTokenAsync(inviteToken, cancellationToken);
            await _friendshipRepository.SaveChangesAsync(cancellationToken);
        }

        return ToInviteLinkResponse(inviteToken);
    }

    public async Task<FriendInviteLinkResponseDto> RegenerateInviteLinkAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        FriendInviteToken inviteToken = await GetOrCreateInviteTokenEntityAsync(userId, cancellationToken);
        inviteToken.RegenerateToken(_friendshipDomainService.GenerateToken());
        await _friendshipRepository.SaveChangesAsync(cancellationToken);

        return ToInviteLinkResponse(inviteToken);
    }

    public async Task<FriendInviteLinkResponseDto> UpdateInviteLinkAsync(
        Guid userId,
        UpdateInviteLinkRequest request,
        CancellationToken cancellationToken)
    {
        FriendInviteToken inviteToken = await GetOrCreateInviteTokenEntityAsync(userId, cancellationToken);
        inviteToken.SetActive(request.IsActive);
        await _friendshipRepository.SaveChangesAsync(cancellationToken);

        return ToInviteLinkResponse(inviteToken);
    }

    public async Task<FriendInvitePreviewResponseDto?> GetInvitePreviewAsync(
        string token,
        CancellationToken cancellationToken)
    {
        FriendInviteToken? inviteToken =
            await _friendshipRepository.GetInviteTokenByTokenAsync(token, cancellationToken);

        if (inviteToken is null)
        {
            return null;
        }

        if (!inviteToken.IsActive)
        {
            throw new InvalidOperationException("Este link de convite está desativado.");
        }

        User inviter = inviteToken.User;
        _friendshipDomainService.EnsureInviterIsActive(inviter);

        return new FriendInvitePreviewResponseDto
        {
            UserId = inviter.Id,
            Name = inviter.Name,
            Username = inviter.Username,
            ProfileImageUrl = inviter.ProfileImageUrl
        };
    }

    public async Task<FriendshipResponseDto> AcceptInviteAsync(
        Guid userId,
        string token,
        CancellationToken cancellationToken)
    {
        FriendInviteToken? inviteToken =
            await _friendshipRepository.GetInviteTokenByTokenAsync(token, cancellationToken);

        if (inviteToken is null)
        {
            throw new KeyNotFoundException("Link de convite inválido.");
        }

        Guid inviterUserId = inviteToken.UserId;
        _friendshipDomainService.EnsureCanAcceptInvite(inviterUserId, userId, inviteToken);
        _friendshipDomainService.EnsureInviterIsActive(inviteToken.User);

        (Guid userLowId, Guid userHighId) =
            _friendshipDomainService.NormalizePair(inviterUserId, userId);

        Friendship? existingFriendship =
            await _friendshipRepository.GetFriendshipByPairAsync(userLowId, userHighId, cancellationToken);

        _friendshipDomainService.EnsureNotAlreadyFriends(existingFriendship);

        DateTime establishedAt = PersistenceClock.GetWallClockNow(_persistenceOptions);

        if (existingFriendship is null)
        {
            Friendship friendship = new(
                Guid.NewGuid(),
                userLowId,
                userHighId,
                userId,
                establishedAt);

            await _friendshipRepository.AddFriendshipAsync(friendship, cancellationToken);
            await _friendshipRepository.SaveChangesAsync(cancellationToken);

            User? friendUser = await _friendshipRepository.GetUserByIdAsync(inviterUserId, cancellationToken);
            return ToFriendshipResponse(friendship, userId, friendUser!);
        }

        existingFriendship.Activate(userId, establishedAt);
        await _friendshipRepository.SaveChangesAsync(cancellationToken);

        User? reactivatedFriendUser =
            await _friendshipRepository.GetUserByIdAsync(inviterUserId, cancellationToken);

        return ToFriendshipResponse(existingFriendship, userId, reactivatedFriendUser!);
    }

    public async Task<IReadOnlyCollection<FriendshipResponseDto>> ListFriendsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        IReadOnlyCollection<Friendship> friendships =
            await _friendshipRepository.ListActiveFriendshipsAsync(userId, cancellationToken);

        List<FriendshipResponseDto> response = friendships
            .Select(friendship => ToFriendshipResponse(
                friendship,
                userId,
                ResolveFriendUser(friendship, userId)))
            .ToList();

        return response;
    }

    public async Task<FriendshipResponseDto?> GetFriendAsync(
        Guid userId,
        Guid friendUserId,
        CancellationToken cancellationToken)
    {
        Friendship? friendship =
            await _friendshipRepository.GetActiveFriendshipAsync(userId, friendUserId, cancellationToken);

        if (friendship is null)
        {
            return null;
        }

        User? friendUser = await _friendshipRepository.GetUserByIdAsync(friendUserId, cancellationToken);
        if (friendUser is null || !friendUser.IsActive)
        {
            return null;
        }

        return ToFriendshipResponse(friendship, userId, friendUser);
    }

    public async Task<bool> RemoveFriendAsync(
        Guid userId,
        Guid friendUserId,
        CancellationToken cancellationToken)
    {
        _friendshipDomainService.EnsureNotSelf(userId, friendUserId);

        Friendship? friendship =
            await _friendshipRepository.GetActiveFriendshipAsync(userId, friendUserId, cancellationToken);

        if (friendship is null)
        {
            return false;
        }

        friendship.Remove();
        await _friendshipRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<FriendDailyComparisonResponseDto?> GetDailyComparisonAsync(
        Guid userId,
        Guid friendUserId,
        DateOnly? date,
        CancellationToken cancellationToken)
    {
        DateOnly comparisonDate = date ?? DateOnly.FromDateTime(
            PersistenceClock.GetWallClockNow(_persistenceOptions));

        _friendshipDomainService.EnsureNotSelf(userId, friendUserId);

        Friendship? friendship =
            await _friendshipRepository.GetActiveFriendshipAsync(userId, friendUserId, cancellationToken);

        _friendshipDomainService.EnsureActiveFriendship(friendship);

        User? callerUser = await _profileRepository.GetByIdWithProfileAsync(userId, cancellationToken);
        User? friendUser = await _profileRepository.GetByIdWithProfileAsync(friendUserId, cancellationToken);

        if (callerUser is null || friendUser is null || !friendUser.IsActive)
        {
            return null;
        }

        FriendDaySnapshotDto meSnapshot = await BuildDaySnapshotAsync(callerUser, comparisonDate, cancellationToken);
        FriendDaySnapshotDto friendSnapshot = await BuildDaySnapshotAsync(friendUser, comparisonDate, cancellationToken);

        return new FriendDailyComparisonResponseDto
        {
            Date = comparisonDate,
            Me = meSnapshot,
            Friend = friendSnapshot
        };
    }

    public async Task<IReadOnlyCollection<MealLogResponseDto>?> GetFriendMealLogsByDateAsync(
        Guid userId,
        Guid friendUserId,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        _friendshipDomainService.EnsureNotSelf(userId, friendUserId);

        Friendship? friendship =
            await _friendshipRepository.GetActiveFriendshipAsync(userId, friendUserId, cancellationToken);

        _friendshipDomainService.EnsureActiveFriendship(friendship);

        User? friendUser = await _friendshipRepository.GetUserByIdAsync(friendUserId, cancellationToken);
        if (friendUser is null || !friendUser.IsActive)
        {
            return null;
        }

        return await _mealLogService.ListByDateAsync(friendUserId, date, cancellationToken);
    }

    private async Task<FriendInviteToken> GetOrCreateInviteTokenEntityAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        FriendInviteToken? inviteToken =
            await _friendshipRepository.GetInviteTokenByUserIdAsync(userId, cancellationToken);

        if (inviteToken is not null)
        {
            return inviteToken;
        }

        inviteToken = new FriendInviteToken(
            Guid.NewGuid(),
            userId,
            _friendshipDomainService.GenerateToken());

        await _friendshipRepository.AddInviteTokenAsync(inviteToken, cancellationToken);
        await _friendshipRepository.SaveChangesAsync(cancellationToken);

        return inviteToken;
    }

    private async Task<FriendDaySnapshotDto> BuildDaySnapshotAsync(
        User user,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        DailyGoalsSnapshotDto goals = BuildGoalsSnapshot(user.UserProfile);
        DailyMealConsumptionSummaryResponseDto consumption =
            await _mealLogService.GetDailyConsumptionSummaryAsync(user.Id, date, cancellationToken);

        return new FriendDaySnapshotDto
        {
            UserId = user.Id,
            Name = user.Name,
            ProfileImageUrl = user.ProfileImageUrl,
            Goals = goals,
            Consumption = consumption,
            Progress = BuildProgressSnapshot(goals, consumption)
        };
    }

    private static DailyGoalsSnapshotDto BuildGoalsSnapshot(UserProfile? profile)
    {
        return new DailyGoalsSnapshotDto
        {
            DailyCaloriesGoal = profile?.DailyCaloriesGoal ?? 0,
            DailyProteinGoal = profile?.DailyProteinGoal ?? 0,
            DailyCarbsGoal = profile?.DailyCarbsGoal ?? 0,
            DailyFatGoal = profile?.DailyFatGoal ?? 0
        };
    }

    private static DailyProgressSnapshotDto BuildProgressSnapshot(
        DailyGoalsSnapshotDto goals,
        DailyMealConsumptionSummaryResponseDto consumption)
    {
        return new DailyProgressSnapshotDto
        {
            CaloriesPercent = CalculatePercent(consumption.TotalCalories, goals.DailyCaloriesGoal),
            ProteinPercent = CalculatePercent(consumption.TotalProtein, goals.DailyProteinGoal),
            CarbsPercent = CalculatePercent(consumption.TotalCarbs, goals.DailyCarbsGoal),
            FatPercent = CalculatePercent(consumption.TotalFat, goals.DailyFatGoal)
        };
    }

    private static decimal CalculatePercent(decimal consumed, int goal)
    {
        if (goal <= 0)
        {
            return 0;
        }

        return decimal.Round(consumed / goal * 100, ProgressScale, MidpointRounding.AwayFromZero);
    }

    private FriendInviteLinkResponseDto ToInviteLinkResponse(FriendInviteToken inviteToken)
    {
        string baseUrl = _friendshipOptions.InviteBaseUrl.TrimEnd('/') + "/";

        return new FriendInviteLinkResponseDto
        {
            Token = inviteToken.Token,
            Url = baseUrl + inviteToken.Token,
            IsActive = inviteToken.IsActive
        };
    }

    private static FriendshipResponseDto ToFriendshipResponse(
        Friendship friendship,
        Guid currentUserId,
        User friendUser)
    {
        return new FriendshipResponseDto
        {
            FriendshipId = friendship.Id,
            FriendUserId = friendUser.Id,
            Name = friendUser.Name,
            Username = friendUser.Username,
            ProfileImageUrl = friendUser.ProfileImageUrl,
            EstablishedAt = friendship.EstablishedAt ?? friendship.CreatedAt
        };
    }

    private static User ResolveFriendUser(Friendship friendship, Guid currentUserId)
    {
        return friendship.UserLowId == currentUserId
            ? friendship.UserHigh
            : friendship.UserLow;
    }
}
