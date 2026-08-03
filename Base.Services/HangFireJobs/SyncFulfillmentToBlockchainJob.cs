// Base.Services/HangfireJobs/SyncFulfillmentToBlockchainJob.cs
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Base.DAL.Contexts;
using Base.Shared.DTOs;

namespace Base.Services.HangfireJobs
{
    public class SyncFulfillmentToBlockchainJob
    {
        private readonly AppDbContext _context;
        private readonly IHttpClientFactory _clientFactory;

        public SyncFulfillmentToBlockchainJob(AppDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _clientFactory = clientFactory;
        }

        public async Task ExecuteAsync(string requestId, int Units)
        {
           

            var httpClient = _clientFactory.CreateClient("BlockchainClient");

            // Build payload using the verified "amount" schema property
            var payload = new BlockchainFulfillmentOnlyDTO
            {
                Amount = Units
            };

            // Only call the fulfillment endpoint using PUT
            var response = await httpClient.PutAsJsonAsync($"blockchain/requests/{requestId}/fulfilled", payload);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Blockchain quantity sync failed for request {requestId}: {error}");
            }
        }
    }
}