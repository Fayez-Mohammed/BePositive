// Base.Services/Interfaces/IFcmService.cs

namespace Base.Services.Interfaces
{
    public interface IFcmService
    {
        /// <summary>
        /// Send a push notification to a single FCM token.
        /// Returns true if sent successfully, false if token is invalid.
        /// </summary>
        Task<bool> SendNotificationAsync(
            string fcmToken,
            string title,
            string body,
            Dictionary<string, string>? data = null);

        /// <summary>
        /// Send push notifications to multiple FCM tokens in batches of 500.
        /// Invalid tokens are logged but do not throw.
        /// </summary>
        Task SendBatchNotificationsAsync(
            IEnumerable<string> fcmTokens,
            string title,
            string body,
            Dictionary<string, string>? data = null);
    }
}
