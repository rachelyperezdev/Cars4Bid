using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/search")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery] SearchParams searchParams){
            var query = DB.PagedSearch<Item, Item>();

            if(!string.IsNullOrEmpty(searchParams.SearchTerm)){
                query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
            }

            query = searchParams.OrderBy switch {
                "make" => query.Sort(a => a.Ascending(i => i.Make)),
                "new" => query.Sort(a => a.Descending(i => i.CreatedAt)),
                _ => query.Sort(a => a.Ascending(i => i.AuctionEnd))
            };

            query = searchParams.FilterBy switch {
                "finished" => query.Match(a => a.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(a => a.AuctionEnd < DateTime.UtcNow.AddHours(6) &&
                                                 a.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(a => a.AuctionEnd > DateTime.UtcNow)
            };

            if(!string.IsNullOrEmpty(searchParams.Seller)){
                query.Match(a => a.Seller == searchParams.Seller);
            }

            if(!string.IsNullOrEmpty(searchParams.Winner)){
                query.Match(a => a.Winner == searchParams.Winner);
            }

            query.PageNumber(searchParams.PageNumber);
            query.PageSize(searchParams.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new {
                results = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
}