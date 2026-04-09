 
namespace BloodManagement.Gamification.Domain.Interfaces
{
    public interface IGamificationProfileService
    {
        Task<UserGamificationProfile> GetOrCreateUserProfileAsync(string userId);
        Task AddPointsAsync(string userId, int points);
        Task<bool> TryRedeemRewardAsync(string userId, Guid rewardId);
        Task UpdateAchievementProgressAsync(string userId, Guid achievementId, int progressIncrement = 1);
    }

    public interface IAchievementService
    {
        Task<IEnumerable<Achievement>> GetAllAchievementsAsync();
        Task<Achievement> GetAchievementByIdAsync(Guid achievementId);
        Task UnlockAchievementAsync(string userId, Guid achievementId);
        Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId);
    }

    public interface IRewardService
    {
        Task<IEnumerable<Reward>> GetAllRewardsAsync();
        Task<Reward> GetRewardByIdAsync(Guid rewardId);
        Task<IEnumerable<UserReward>> GetUserRedeemedRewardsAsync(string userId);
    }

    public interface IEventPublisher
    {
        Task PublishAchievementUnlockedEvent(string userId, Guid achievementId);
        Task PublishPointsAwardedEvent(string userId, int points);
        Task PublishRewardRedeemedEvent(string userId, Guid rewardId);
    }
}