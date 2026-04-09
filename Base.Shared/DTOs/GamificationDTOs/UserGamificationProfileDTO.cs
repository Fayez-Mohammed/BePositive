using System;
using System.Collections.Generic;

namespace BloodManagement.Gamification.Application.DTOs
{
    public class UserGamificationProfileDto
    {
        public string UserId { get; set; }
        public int TotalPoints { get; set; }
        public string CurrentTier { get; set; }
        public List<UserAchievementDto> Achievements { get; set; }
        public List<UserRewardDto> RedeemedRewards { get; set; }
    }

    public class AchievementDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int PointsAwarded { get; set; }
        public string Icon { get; set; }
        public string Status { get; set; } // Unlocked or Locked
        public DateTime? UnlockedDate { get; set; }
        public int CurrentProgress { get; set; }
        public int TargetValue { get; set; }
    }

    public class RewardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cost { get; set; }
        public string Image { get; set; }
    }

    public class UserAchievementDto
    {
        public Guid AchievementId { get; set; }
        public string Status { get; set; }
        public DateTime? UnlockedDate { get; set; }
        public int CurrentProgress { get; set; }
    }

    public class UserRewardDto
    {
        public Guid RewardId { get; set; }
        public DateTime RedeemedDate { get; set; }
    }

    public class RedeemRewardRequestDto
    {
        public string UserId { get; set; }
        public Guid RewardId { get; set; }
    }

    public class RedeemRewardResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int NewTotalPoints { get; set; }
    }

    public class AddPointsRequestDto
    {
        public string UserId { get; set; }
        public int Points { get; set; }
    }

    public class UpdateAchievementProgressRequestDto
    {
        public string UserId { get; set; }
        public Guid AchievementId { get; set; }
        public int ProgressIncrement { get; set; }
    }
}
