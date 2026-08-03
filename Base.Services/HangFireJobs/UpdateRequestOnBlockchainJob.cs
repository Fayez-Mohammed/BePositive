// Base.Services/HangfireJobs/UpdateRequestOnBlockchainJob.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Base.DAL.Contexts;
using Base.Shared.DTOs;
using Hangfire;

namespace Base.Services.HangfireJobs
{
    public class UpdateRequestOnBlockchainJob
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public UpdateRequestOnBlockchainJob(AppDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        [AutomaticRetry(Attempts = 4, LogEvents = true)]
        public async Task ExecuteAsync(string requestId)
        {
            // Fetch the ABSOLUTE LATEST status directly from the database at execution time
            var currentStatus = await _context.DonationRequests
                .AsNoTracking()
                .Where(r => r.Id == requestId)
                .Select(r => new { StatusInt = (int)r.Status }) // Strict Rule 6 compliance
                .FirstOrDefaultAsync();

            if (currentStatus == null) return; // Guard clause if request was deleted

            var httpClient = _clientFactory.CreateClient("BlockchainClient");

            var statusPayload = new BlockchainStatusUpdateDTO { Status = currentStatus.StatusInt };

            var statusResponse = await httpClient.PutAsJsonAsync($"blockchain/requests/{requestId}/status", statusPayload);

            if (!statusResponse.IsSuccessStatusCode)
            {
                var error = await statusResponse.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Blockchain status sync failed for {requestId}: {error}");
            }
        }
    }
}