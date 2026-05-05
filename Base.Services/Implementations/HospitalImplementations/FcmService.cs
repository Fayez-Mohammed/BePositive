// Base.Services/Implementations/FcmService.cs

using Base.Services.Interfaces;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Base.Services.Implementations
{
    public class FcmService : IFcmService
    {
        private readonly ILogger<FcmService> _logger;
        private readonly bool _initialized;

        public FcmService(
            IConfiguration configuration,
            ILogger<FcmService> logger)
        {
            _logger = logger;

            // Initialize Firebase only once per application lifetime
            if (FirebaseApp.DefaultInstance != null)
            {
                _initialized = true;
                return;
            }

            try
            {
                var credentialJson = configuration["Firebase:CredentialJson"];
                var credentialPath = configuration["Firebase:CredentialPath"];

                GoogleCredential credential;

                if (!string.IsNullOrWhiteSpace(credentialJson))
                {
                    credential = GoogleCredential.FromJson(credentialJson);
                }
                else if (!string.IsNullOrWhiteSpace(credentialPath)
                         && File.Exists(credentialPath))
                {
                    credential = GoogleCredential.FromFile(credentialPath);
                }
                else
                {
                    _logger.LogWarning(
                        "Firebase credentials not configured. " +
                        "Add Firebase:CredentialPath or Firebase:CredentialJson to appsettings.json.");
                    return;
                }

                FirebaseApp.Create(new AppOptions { Credential = credential });
                _initialized = true;

                _logger.LogInformation("Firebase initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Firebase.");
            }
        }

        // ── Send to a single token ────────────────────────────
        public async Task<bool> SendNotificationAsync(
            string fcmToken,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            if (!_initialized)
            {
                _logger.LogWarning("Firebase not initialized. Skipping FCM send.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(fcmToken))
                return false;

            try
            {
                var message = new Message
                {
                    Token        = fcmToken,
                    Notification = new Notification { Title = title, Body  = body },
                    Data         = data ?? new Dictionary<string, string>(),
                    Android      = new AndroidConfig { Priority = Priority.High },
                    Apns         = new ApnsConfig
                    {
                        Aps = new Aps { Sound = "default" }
                    }
                };

                await FirebaseMessaging.DefaultInstance.SendAsync(message);
                return true;
            }
            catch (FirebaseMessagingException ex)
                when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered ||
                      ex.MessagingErrorCode == MessagingErrorCode.InvalidArgument)
            {
                // Token is stale or invalid — caller should remove it from DB
                _logger.LogWarning(
                    "Invalid or unregistered FCM token. Error: {Error}", ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send FCM notification.");
                return false;
            }
        }

        // ── Send to multiple tokens (FCM batch limit = 500) ───
        public async Task SendBatchNotificationsAsync(
            IEnumerable<string> fcmTokens,
            string title,
            string body,
            Dictionary<string, string>? data = null)
        {
            if (!_initialized)
            {
                _logger.LogWarning("Firebase not initialized. Skipping batch FCM send.");
                return;
            }

            var tokenList = fcmTokens
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Distinct()
                .ToList();

            if (!tokenList.Any()) return;

            const int batchSize = 500;

            for (int i = 0; i < tokenList.Count; i += batchSize)
            {
                var batch = tokenList.Skip(i).Take(batchSize).ToList();

                var messages = batch.Select(token => new Message
                {
                    Token        = token,
                    Notification = new Notification { Title = title, Body  = body },
                    Data         = data ?? new Dictionary<string, string>(),
                    Android      = new AndroidConfig { Priority = Priority.High },
                    Apns         = new ApnsConfig
                    {
                        Aps = new Aps { Sound = "default" }
                    }
                }).ToList();

                try
                {
                    var response = await FirebaseMessaging.DefaultInstance
                        .SendEachAsync(messages);

                    _logger.LogInformation(
                        "FCM batch sent: {Success} success, {Failure} failed out of {Total}.",
                        response.SuccessCount, response.FailureCount, batch.Count);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "FCM batch send failed at batch index {Index}.", i);
                }
            }
        }
    }
}
