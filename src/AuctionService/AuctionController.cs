using System;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService
{
	[ApiController]
	[Route("api/auctions")]
	public class Auctioncontroller: ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly AuctionDbContext _context;
		public Auctioncontroller(AuctionDbContext context, IMapper mapper)
		{
			_context = context;
			_mapper = mapper;
		}
		[HttpGet]
		public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
		{
			var auctions = await _context.Auctions
				.Include(x => x.Item)
				.OrderBy(x => x.Item.Make)
				.ToListAsync();
			return _mapper.Map<List<AuctionDto>>(auctions);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
		{
			var auction = await _context.Auctions
				.Include(x => x.Item)
				.FirstOrDefaultAsync(x => x.Id == id);
			if (auction == null) return NotFound();
			return _mapper.Map<AuctionDto>(auction);
		}
		[HttpPost]
		public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
		{
			var auction = _mapper.Map<Auction>(auctionDto);
			//add current user as seller
			auction.Seller = "test";
			_context.Auctions.Add(auction);
			var result = await _context.SaveChangesAsync() > 0;
			if (!result) return BadRequest("Could not save changes to DB");

			return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map <AuctionDto>(auction));
		}
		[HttpPut("{id}")]
		public async Task<ActionResult> UpdateAuction(Guid id, UpdateOptionDto updateDto)
		{
			var auction = await _context.Auctions.Include(x => x.Item)
				.FirstOrDefaultAsync(y => y.Id == id);
			if (auction == null) return NotFound();
			// check Seller == userName
			auction.Item.Make = updateDto.Make ?? auction.Item.Make;
            auction.Item.Color = updateDto.Color ?? auction.Item.Color;
            auction.Item.Model = updateDto.Model ?? auction.Item.Model;
            auction.Item.Mileage = updateDto.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = updateDto.Year ?? auction.Item.Year;
			var results = await _context.SaveChangesAsync() > 0;
			if (results) return Ok();
			return BadRequest("Problem saving changes");

        }

		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteAuction(Guid id)
		{
			var auction = await _context.Auctions.FindAsync(id);
			if (auction == null) return NotFound();
            _context.Auctions.Remove(auction);
			var result = await _context.SaveChangesAsync() > 0;
			if (result) return Ok();
			return BadRequest("Could not remove from Db");
		}
    }
}

