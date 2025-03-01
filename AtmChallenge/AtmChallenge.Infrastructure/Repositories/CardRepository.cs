using AtmChallenge.Domain.Entities.Card;
using AtmChallenge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtmChallenge.Infrastructure.Repositories;

public class CardRepository
{
    private readonly AppDbContext _context;

    public CardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Card?> AuthenticateCardAsync(string numberHash, string pinHash)
    {
        return await _context.Cards.SingleOrDefaultAsync(u => u.NumberHash == numberHash && u.PinHash == pinHash);
    }

    public async Task UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
    }
}