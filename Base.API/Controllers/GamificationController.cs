 
using Microsoft.AspNetCore.Mvc;
using BloodManagement.Gamification.Domain.Interfaces;
using BloodManagement.Gamification.Application.DTOs;

namespace BloodManagement.Gamification.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GamificationController : ControllerBase
    {
        private readonly IGamificationProfileService _gamificationProfileService;
        private readonly IAchievementService _achievementService;
        private readonly IRewardService _rewardService;

        public GamificationController(
            IGamificationProfileService gamificationProfileService,
            IAchievementService achievementService,
            IRewardService rewardService)
        {
            _gamificationProfileService = gamificationProfileService;
            _achievementService = achievementService;
            _rewardService = rewardService;
        }

        /// <summary>
        /// Gets a user's gamification profile, including points, tier, achievements, and redeemed rewards.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The user's gamification profile.</returns>
        [HttpGet("profile/{userId}")]
        public async Task<ActionResult<UserGamificationProfileDto>> GetUserProfile(string userId)
        {
            var profile = await _gamificationProfileService.GetOrCreateUserProfileAsync(userId);
            if (profile == null)
            {
                return NotFound();
            }

            // Map domain entities to DTOs
            var achievementsDto = profile.Achievements.Select(ua => new UserAchievementDto
            {
                AchievementId = ua.AchievementId,
                Status = ua.Status.ToString(),
                UnlockedDate = ua.UnlockedDate,
                CurrentProgress = ua.CurrentProgress
            }).ToList();

            var redeemedRewardsDto = profile.RedeemedRewards.Select(ur => new UserRewardDto
            {
                RewardId = ur.RewardId,
                RedeemedDate = ur.RedeemedDate
            }).ToList();

            var profileDto = new UserGamificationProfileDto
            {
                UserId = profile.UserId,
                TotalPoints = profile.TotalPoints.Value,
                CurrentTier = profile.CurrentTier.ToString(),
                Achievements = achievementsDto,
                RedeemedRewards = redeemedRewardsDto
            };

            return Ok(profileDto);
        }

        /// <summary>
        /// Adds points to a user's gamification profile.
        /// </summary>
        /// <param name="request">The request containing user ID and points to add.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost("points/add")]
        public async Task<IActionResult> AddPoints([FromBody] AddPointsRequestDto request)
        {
            await _gamificationProfileService.AddPointsAsync(request.UserId, request.Points);
            return Ok();
        }

        /// <summary>
        /// Redeems a reward for a user.
        /// </summary>
        /// <param name="request">The request containing user ID and reward ID.</param>
        /// <returns>A response indicating if the redemption was successful.</returns>
        [HttpPost("rewards/redeem")]
        public async Task<ActionResult<RedeemRewardResponseDto>> RedeemReward([FromBody] RedeemRewardRequestDto request)
        {
            var success = await _gamificationProfileService.TryRedeemRewardAsync(request.UserId, request.RewardId);
            var profile = await _gamificationProfileService.GetOrCreateUserProfileAsync(request.UserId);

            if (success)
            {
                return Ok(new RedeemRewardResponseDto { Success = true, Message = "Reward redeemed successfully.", NewTotalPoints = profile.TotalPoints.Value });
            }
            return BadRequest(new RedeemRewardResponseDto { Success = false, Message = "Insufficient points or reward not found.", NewTotalPoints = profile.TotalPoints.Value });
        }

        /// <summary>
        /// Updates the progress of a specific achievement for a user.
        /// </summary>
        /// <param name="request">The request containing user ID, achievement ID, and progress increment.</param>
        /// <returns>An action result indicating success or failure.</returns>
        [HttpPost("achievements/progress")]
        public async Task<IActionResult> UpdateAchievementProgress([FromBody] UpdateAchievementProgressRequestDto request)
        {
            await _gamificationProfileService.UpdateAchievementProgressAsync(request.UserId, request.AchievementId, request.ProgressIncrement);
            return Ok();
        }

        /// <summary>
        /// Gets a list of all available achievements.
        /// </summary>
        /// <returns>A list of achievement DTOs.</returns>
        [HttpGet("achievements")]
        public async Task<ActionResult<IEnumerable<AchievementDto>>> GetAllAchievements()
        {
            var achievements = await _achievementService.GetAllAchievementsAsync();
            var achievementDtos = new List<AchievementDto>();

            foreach (var achievement in achievements)
            {
                achievementDtos.Add(new AchievementDto
                {
                    Id = achievement.Id,
                    Name = achievement.Name,
                    Description = achievement.Description,
                    PointsAwarded = achievement.PointsAwarded,
                    Icon = achievement.Icon,
                    TargetValue = achievement.TargetValue,
                    Status = "N/A" // Status is specific to a user, not a general achievement definition
                });
            }
            return Ok(achievementDtos);
        }

        /// <summary>
        /// Gets a list of all available rewards.
        /// </summary>
        /// <returns>A list of reward DTOs.</returns>
        [HttpGet("rewards")]
        public async Task<ActionResult<IEnumerable<RewardDto>>> GetAllRewards()
        {
            var rewards = await _rewardService.GetAllRewardsAsync();
            var rewardDtos = rewards.Select(r => new RewardDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                Cost = r.Cost.Value,
                Image = r.Image
            }).ToList();
            return Ok(rewardDtos);
        }
    }
}
