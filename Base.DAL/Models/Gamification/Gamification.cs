using System;
using System.Collections.Generic;

namespace BloodManagement.Gamification.Domain
{
    // Enums
    public enum AchievementStatus
    {
        Locked,
        Unlocked
    }

    public enum TierLevel
    {
        Bronze = 0,
        Silver = 1000,  
        Gold = 2000,
        Platinum = 3000
    }

     
    public record Points(int Value);

     
    public class UserGamificationProfile
    {
        public string UserId { get; set; }
        public Points TotalPoints { get; set; }
        public TierLevel CurrentTier { get; set; }
        public List<UserAchievement> Achievements { get; set; } = new List<UserAchievement>();
        public List<UserReward> RedeemedRewards { get; set; } = new List<UserReward>();

        public UserGamificationProfile(string userId)
        {
            UserId = userId;
            TotalPoints = new Points(0);
            CurrentTier = TierLevel.Bronze;
        }

        public void AddPoints(int points)
        {
            TotalPoints = new Points(TotalPoints.Value + points);
            UpdateTier();
        }

        public bool TryRedeemReward(Reward reward)
        {
            if (TotalPoints.Value >= reward.Cost.Value)
            {
                TotalPoints = new Points(TotalPoints.Value - reward.Cost.Value);
                RedeemedRewards.Add(new UserReward(reward.Id, DateTime.UtcNow));
                return true;
            }
            return false;
        }

        private void UpdateTier()
        {
            
            if (TotalPoints.Value >= (int)TierLevel.Platinum)
            {
                CurrentTier = TierLevel.Platinum;
            }
            else if (TotalPoints.Value >= (int)TierLevel.Gold)
            {
                CurrentTier = TierLevel.Gold;
            }
            else if (TotalPoints.Value >= (int)TierLevel.Silver)
            {
                CurrentTier = TierLevel.Silver;
            }
            else
            {
                CurrentTier = TierLevel.Bronze;
            }
        }
    }

    public class Achievement
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PointsAwarded { get; set; }
        public string Icon { get; set; } // e.g., FontAwesome icon class or URL
        public AchievementType Type { get; set; } // e.g., FirstDonation, MultipleDonations, TierBased
        public int TargetValue { get; set; } // e.g., 3 for 'Save 3 lives', 5 for 'Donate within 5 days'

        public Achievement(string name, string description, int pointsAwarded, string icon, AchievementType type, int targetValue = 0)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            PointsAwarded = pointsAwarded;
            Icon = icon;
            Type = type;
            TargetValue = targetValue;
        }
    }

    public class UserAchievement
    {
        public Guid AchievementId { get; set; }
        public AchievementStatus Status { get; set; }
        public DateTime? UnlockedDate { get; set; }
        public int CurrentProgress { get; set; } // For achievements with progress (e.g., Consistency Champion)

        public UserAchievement(Guid achievementId, DateTime? unlockedDate = null, int currentProgress = 0)
        {
            AchievementId = achievementId;
            Status = unlockedDate.HasValue ? AchievementStatus.Unlocked : AchievementStatus.Locked;
            UnlockedDate = unlockedDate;
            CurrentProgress = currentProgress;
        }
    }

    public enum AchievementType
    {
        SingleEvent,
        Cumulative,
        TierBased,
        TimeBased
    }

    public class Reward
    {
        
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Points Cost { get; set; }
        public string Image { get; set; } // URL or path to reward image
        public RewardType Type { get; set; } // e.g., PhysicalItem, Service, Voucher

        public Reward()
        {
            
        }
        public Reward(string name, string description, int cost, string image, RewardType type)
        {
            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            Cost = new Points(cost);
            Image = image;
            Type = type;
        }
    }

    public class UserReward
    {
        public Guid RewardId { get; set; }
        public DateTime RedeemedDate { get; set; }

        public UserReward(Guid rewardId, DateTime redeemedDate)
        {
            RewardId = rewardId;
            RedeemedDate = redeemedDate;
        }
    }

    public enum RewardType
    {
        PhysicalItem,
        Service,
        Voucher
    }
}
