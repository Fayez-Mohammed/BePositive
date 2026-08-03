// Base.Services/HangfireJobs/LogRequestToBlockchainJob.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Base.DAL.Contexts;
using Base.Shared.DTOs;
using Hangfire; // Added for AutomaticRetry attribute

namespace Base.Services.HangfireJobs
{
    public class LogRequestToBlockchainJob
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public LogRequestToBlockchainJob(AppDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        // FIX: Enforces exactly 4 retry attempts if an exception is thrown, with clean dashboard tracking.
        [AutomaticRetry(Attempts = 4, LogEvents = true, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
        public async Task ExecuteAsync(string requestId)
        {
            // 1. Fetch data required for payload using scalar selection (Strict Rule 5 & 6)
            var requestData = await _context.DonationRequests
                .AsNoTracking()
                .Where(r => r.Id == requestId && !r.IsDeleted)
                .Select(r => new
                {
                    r.Id,
                    r.HospitalId,
                    r.BloodTypeId,
                    r.QuantityRequired,
                    UrgencyLevelInt = (int)r.UrgencyLevel,
                    r.Deadline
                })
                .FirstOrDefaultAsync();

            if (requestData == null) return; // Request doesn't exist or was deleted

            // 2. Build out the specific camelCase payload
            var blockchainPayload = new BlockchainRequestDTO
            {
                RequestId = requestData.Id,
                HospitalId = requestData.HospitalId,
                BloodTypeId = requestData.BloodTypeId,
                QuantityRequired = requestData.QuantityRequired,
                UrgencyLevel = requestData.UrgencyLevelInt,
                Deadline = requestData.Deadline?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty
            };

            // 3. Dispatch the external HTTP POST request
            var httpClient = _clientFactory.CreateClient("BlockchainClient");
            var response = await httpClient.PostAsJsonAsync("blockchain/requests", blockchainPayload);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                // Throwing an exception triggers the [AutomaticRetry] handler above
                throw new HttpRequestException($"Blockchain API failed with status {response.StatusCode}: {errorContent}");
            }
        }
    }
}

//// Base.Services/HangfireJobs/LogRequestToBlockchainJob.cs
//using System;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using Base.DAL.Contexts;
//using Base.Shared.DTOs;

//namespace Base.Services.HangfireJobs
//{
//    public class LogRequestToBlockchainJob
//    {
//        private readonly AppDbContext _context;
//        private readonly IHttpClientFactory _clientFactory;

//        public LogRequestToBlockchainJob(AppDbContext context, IHttpClientFactory clientFactory)
//        {
//            _context = context;
//            _clientFactory = clientFactory;
//        }

//        // Hangfire will automatically retry this method if an exception is thrown
//        public async Task ExecuteAsync(string requestId)
//        {
//            // 1. Fetch data required for payload using scalar selection (Strict Rule 5 & 6)
//            var requestData = await _context.DonationRequests
//                .AsNoTracking()
//                .Where(r => r.Id == requestId && !r.IsDeleted)
//                .Select(r => new
//                {
//                    r.Id,
//                    r.HospitalId,
//                    r.BloodTypeId,
//                    r.QuantityRequired,
//                    UrgencyLevelInt = (int)r.UrgencyLevel,
//                    r.Deadline
//                })
//                .FirstOrDefaultAsync();

//            if (requestData == null) return; // Request doesn't exist or was deleted

//            // 2. Build out the specific camelCase payload
//            var blockchainPayload = new BlockchainRequestDTO
//            {
//                RequestId = requestData.Id,
//                HospitalId = requestData.HospitalId,
//                BloodTypeId = requestData.BloodTypeId,
//                QuantityRequired = requestData.QuantityRequired,
//                UrgencyLevel = requestData.UrgencyLevelInt,
//                Deadline = requestData.Deadline?.ToString("yyyy-MM-ddTHH:mm:ssZ") ?? string.Empty
//            };

//            // 3. Dispatch the external HTTP POST request
//            var httpClient = _clientFactory.CreateClient("BlockchainClient");
//            var response = await httpClient.PostAsJsonAsync("blockchain/requests", blockchainPayload);

//            if (!response.IsSuccessStatusCode)
//            {
//                var errorContent = await response.Content.ReadAsStringAsync();
//                throw new HttpRequestException($"Blockchain API failed with status {response.StatusCode}: {errorContent}");
//            }

//            // Optional: You could read the transaction hash here and save it to your DB if needed
//            // var result = await response.Content.ReadFromJsonAsync<BlockchainResponseDTO>();
//        }
//    }
//}