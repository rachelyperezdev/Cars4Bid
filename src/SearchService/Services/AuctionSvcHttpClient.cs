using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Services
{
    public class AuctionSvcHttpClient
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _configuration;

        public AuctionSvcHttpClient(HttpClient client, IConfiguration configuration)
        {
            _configuration = configuration;
            _client = client;
            
        }

        public async Task<List<Item>> GetItemsForDb() 
        {
            var lastUpdated = await DB.Find<Item, string>()
                                      .Sort(x => x.Descending(x => x.UpdatedAt))
                                      .Project(x => x.UpdatedAt.ToString())
                                      .ExecuteFirstAsync();

            return await _client.GetFromJsonAsync<List<Item>>(
                    _configuration["AuctionServiceUrl"] + 
                    "/api/auctions?date=" + 
                    lastUpdated);
        }
    }
}