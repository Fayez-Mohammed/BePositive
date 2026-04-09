 
using BloodManagement.Gamification.Domain;
using BloodManagement.Gamification.Domain.Interfaces;
 

namespace BloodManagement.Gamification.Application.Services
{
    public class GamificationProfileService : IGamificationProfileService
    {
        // In-memory store for demonstration. In a real application, this would be a database context.
        public static readonly Dictionary<string, UserGamificationProfile> _userProfiles = new Dictionary<string, UserGamificationProfile>();
        private readonly IAchievementService _achievementService;
        private readonly IEventPublisher _eventPublisher;

        public GamificationProfileService(IAchievementService achievementService, IEventPublisher eventPublisher)
        {
            _achievementService = achievementService;
            _eventPublisher = eventPublisher;
        }

        public async Task<UserGamificationProfile> GetOrCreateUserProfileAsync(string userId)
        {
            if (!_userProfiles.ContainsKey(userId))
            {
                var newProfile = new UserGamificationProfile(userId);
                // Initialize with all achievements as locked
                var allAchievements = await _achievementService.GetAllAchievementsAsync();
                foreach (var achievement in allAchievements)
                {
                    newProfile.Achievements.Add(new UserAchievement(achievement.Id));
                }
                _userProfiles[userId] = newProfile;
            }
            return _userProfiles[userId];
        }

        public async Task AddPointsAsync(string userId, int points)
        {
            var profile = await GetOrCreateUserProfileAsync(userId);
            profile.AddPoints(points);
            await _eventPublisher.PublishPointsAwardedEvent(userId, points);
            // Check for tier achievements after adding points
            await CheckAndUnlockTierAchievements(userId, profile);
        }

        public async Task<bool> TryRedeemRewardAsync(string userId, Guid rewardId)
        {
            var profile = await GetOrCreateUserProfileAsync(userId);
            var reward = await _achievementService.GetAchievementByIdAsync(rewardId); // Assuming reward service can fetch rewards
            if (reward == null) return false; // Reward not found

            // For demonstration, let's create a dummy reward object here
            // In a real app, you'd fetch the actual Reward object from a repository
            var dummyReward = new Reward("Dummy Reward", "", 0, "", RewardType.Voucher);
            // Replace with actual reward fetching logic
            // var actualReward = await _rewardService.GetRewardByIdAsync(rewardId);
            // if (actualReward == null) return false;

            if (profile.TryRedeemReward(dummyReward)) // Use dummyReward for now
            {
                await _eventPublisher.PublishRewardRedeemedEvent(userId, rewardId);
                return true;
            }
            return false;
        }

        public async Task UpdateAchievementProgressAsync(string userId, Guid achievementId, int progressIncrement = 1)
        {
            var profile = await GetOrCreateUserProfileAsync(userId);
            var userAchievement = profile.Achievements.FirstOrDefault(ua => ua.AchievementId == achievementId);

            if (userAchievement != null && userAchievement.Status == AchievementStatus.Locked)
            {
                userAchievement.CurrentProgress += progressIncrement;
                var achievementDefinition = await _achievementService.GetAchievementByIdAsync(achievementId);

                if (achievementDefinition != null && userAchievement.CurrentProgress >= achievementDefinition.TargetValue)
                {
                    await _achievementService.UnlockAchievementAsync(userId, achievementId);
                    profile.AddPoints(achievementDefinition.PointsAwarded);
                }
            }
        }

        private async Task CheckAndUnlockTierAchievements(string userId, UserGamificationProfile profile)
        {
            var allAchievements = await _achievementService.GetAllAchievementsAsync();
            foreach (var achievement in allAchievements.Where(a => a.Type == AchievementType.TierBased))
            {
                // Assuming achievement.TargetValue stores the point threshold for tier-based achievements
                if (profile.TotalPoints.Value >= achievement.TargetValue)
                {
                    var userAchievement = profile.Achievements.FirstOrDefault(ua => ua.AchievementId == achievement.Id);
                    if (userAchievement != null && userAchievement.Status == AchievementStatus.Locked)
                    {
                        await _achievementService.UnlockAchievementAsync(userId, achievement.Id);
                        // Points for tier achievements are already included in the tier update logic, so no need to add again here.
                    }
                }
            }
        }
    }

    public class AchievementService : IAchievementService
    {
        // In-memory store for demonstration
        private static readonly List<Achievement> _achievements = new List<Achievement>();
        private readonly IEventPublisher _eventPublisher;

        public AchievementService(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
            // Seed some achievements based on the UI image
            if (!_achievements.Any())
            {
                _achievements.Add(new Achievement("First Drop", "Complete your first blood donation", 100, "heart", AchievementType.SingleEvent, 1));
                _achievements.Add(new Achievement("Life Saver", "Save 3 lives through donations", 250, "star", AchievementType.Cumulative, 3));
                _achievements.Add(new Achievement("Gold Donor", "Reach Gold tier status", 500, "trophy", AchievementType.TierBased, (int)TierLevel.Gold));
                _achievements.Add(new Achievement("Speed Demon", "Donate within 5 days of request", 150, "lightning", AchievementType.TimeBased, 5));
                _achievements.Add(new Achievement("Consistency Champion", "Donate 3 times in a year", 300, "lock", AchievementType.Cumulative, 3));
                _achievements.Add(new Achievement("Platinum Hero", "Reach Platinum tier status", 1000, "lock", AchievementType.TierBased, (int)TierLevel.Platinum));
            }
        }

        public Task<IEnumerable<Achievement>> GetAllAchievementsAsync()
        {
            return Task.FromResult<IEnumerable<Achievement>>(_achievements);
        }

        public Task<Achievement> GetAchievementByIdAsync(Guid achievementId)
        {
            return Task.FromResult(_achievements.FirstOrDefault(a => a.Id == achievementId));
        }

        public async Task UnlockAchievementAsync(string userId, Guid achievementId)
        {
            var profile = await GetOrCreateUserProfile(userId); // Assuming access to user profile
            var userAchievement = profile.Achievements.FirstOrDefault(ua => ua.AchievementId == achievementId);

            if (userAchievement != null && userAchievement.Status == AchievementStatus.Locked)
            {
                userAchievement.Status = AchievementStatus.Unlocked;
                userAchievement.UnlockedDate = DateTime.UtcNow;
                await _eventPublisher.PublishAchievementUnlockedEvent(userId, achievementId);
            }
        }

        public async Task<IEnumerable<UserAchievement>> GetUserAchievementsAsync(string userId)
        {
            var profile = await GetOrCreateUserProfile(userId);
            return profile.Achievements;
        }

        // Helper to get user profile (should be injected or retrieved from a repository)
        private async Task<UserGamificationProfile> GetOrCreateUserProfile(string userId)
        {
            // This is a simplified approach. In a real application, you'd inject IGamificationProfileService
            // or a repository to get the user profile.
            if (!GamificationProfileService._userProfiles.ContainsKey(userId))
            {
                var newProfile = new UserGamificationProfile(userId);
                var allAchievements = await GetAllAchievementsAsync();
                foreach (var achievement in allAchievements)
                {
                    newProfile.Achievements.Add(new UserAchievement(achievement.Id));
                }
                GamificationProfileService._userProfiles[userId] = newProfile;
            }
            return GamificationProfileService._userProfiles[userId];
        }
    }

    public class RewardService : IRewardService
    {
        // In-memory store for demonstration
        private static readonly List<Reward> _rewards = new List<Reward>();

        public RewardService()
        {
            // Seed some rewards based on the UI image
            if (!_rewards.Any())
            {
                _rewards.Add(new Reward("Free Health Checkup", "Comprehensive health screening", 500, "health_checkup_icon", RewardType.Service));
                _rewards.Add(new Reward("Be Positive T-Shirt", "Limited edition donor merch", 750, "tshirt_icon", RewardType.PhysicalItem));
                _rewards.Add(new Reward("Priority Scheduling", "Skip the wait for 6 months", 1000, "alarm_icon", RewardType.Service));
                _rewards.Add(new Reward("Coffee Voucher", "$25 Starbucks gift card", 1500, "coffee_icon", RewardType.Voucher));
            }
        }

        public Task<IEnumerable<Reward>> GetAllRewardsAsync()
        {
            return Task.FromResult<IEnumerable<Reward>>(_rewards);
        }

        public Task<Reward> GetRewardByIdAsync(Guid rewardId)
        {
            return Task.FromResult(_rewards.FirstOrDefault(r => r.Id == rewardId));
        }

        public async Task<IEnumerable<UserReward>> GetUserRedeemedRewardsAsync(string userId)
        {
            var profile = await GetOrCreateUserProfile(userId);
            return profile.RedeemedRewards;
        }

        // Helper to get user profile (should be injected or retrieved from a repository)
        private async Task<UserGamificationProfile> GetOrCreateUserProfile(string userId)
        {
            // This is a simplified approach. In a real application, you'd inject IGamificationProfileService
            // or a repository to get the user profile.
            if (!GamificationProfileService._userProfiles.ContainsKey(userId))
            {
                var newProfile = new UserGamificationProfile(userId);
                // Initialize with all achievements as locked
                // This part would typically be handled by the GamificationProfileService upon user creation
                // For simplicity, we're assuming it's done elsewhere or will be refactored.
                GamificationProfileService._userProfiles[userId] = newProfile;
            }
            return GamificationProfileService._userProfiles[userId];
        }
    }

    public class EventPublisher : IEventPublisher
    {
        public Task PublishAchievementUnlockedEvent(string userId, Guid achievementId)
        {
            Console.WriteLine($"Event: Achievement {achievementId} unlocked for user {userId}");
            return Task.CompletedTask;
        }

        public Task PublishPointsAwardedEvent(string userId, int points)
        {
            Console.WriteLine($"Event: {points} points awarded to user {userId}");
            return Task.CompletedTask;
        }

        public Task PublishRewardRedeemedEvent(string userId, Guid rewardId)
        {
            Console.WriteLine($"Event: Reward {rewardId} redeemed by user {userId}");
            return Task.CompletedTask;
        }
    }
}
