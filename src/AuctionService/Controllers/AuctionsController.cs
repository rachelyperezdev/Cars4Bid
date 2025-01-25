using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionsController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        public AuctionsController(AuctionDbContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
            
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions() {
            var auctions = await _context.Auctions
                                         .Include(a => a.Item)
                                         .OrderBy(a => a.Item.Make)
                                         .ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        } 

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById([FromRoute] Guid id){
            var auction = await _context.Auctions
                                       .Include(a => a.Item)
                                       .FirstOrDefaultAsync(a => a.Id == id);

            if(auction is null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAuction([FromBody] CreateAuctionDto auctionDto){
            var auction = _mapper.Map<Auction>(auctionDto);

            // TODO: add current user as seller
            auction.Seller = "test";

            _context.Auctions.Add(auction);
            var result = await _context.SaveChangesAsync() > 0;

            if(!result) return BadRequest("Could not save changes to the DB.");

            return CreatedAtAction(nameof(GetAuctionById), new {Id = auction.Id}, _mapper.Map<AuctionDto>(auction));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAuction([FromRoute] Guid id, [FromBody] UpdateAuctionDto updateAuctionDto){
            var auction = await _context.Auctions.Include(a => a.Item)
                                                 .FirstOrDefaultAsync(a => a.Id == id);

            if(auction == null) return NotFound();

            // TODO: check if seller == username

            auction.Item.Make = updateAuctionDto.Make ?? auction.Item.Make;
            auction.Item.Model = updateAuctionDto.Model ?? auction.Item.Model;
            auction.Item.Color = updateAuctionDto.Color ?? auction.Item.Color;
            auction.Item.Mileage = updateAuctionDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateAuctionDto.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if(!result) return BadRequest("Problem saving changes");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAuction([FromRoute] Guid id){
            var auction = await _context.Auctions.FirstOrDefaultAsync(a => a.Id == id);

            if(auction is null) return NotFound();

            // TODO: check seller == username

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if(!result) return BadRequest("Could not update the db");

            return NoContent();
        }
    }
}