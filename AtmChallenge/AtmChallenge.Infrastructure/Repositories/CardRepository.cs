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
    
    public async Task<Card?> GetCardByHash(string numberHash)
    {
        return await _context.Cards
            .Include(c => c.User)
            .SingleOrDefaultAsync(c => c.NumberHash == numberHash);
    }
    
    public async Task<(List<CardTransaction>, int)> GetPaginatedTransactionsAsync(string encryptedCardNumber, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // 🔹 Prevent excessive data fetching

        var query = _context.CardTransactions
            .Include(t => t.Card) // 🔹 Ensure Card is loaded to prevent null issues
            .Where(t => t.Card != null && t.Card.NumberHash == encryptedCardNumber)
            .OrderByDescending(t => t.Date);

        int totalRecords = await query.CountAsync(); // 🔹 Optimize total count calculation

        var transactions = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(); // 🔹 Only execute the query once

        return (transactions, totalRecords);
    }
    public async Task AddTransactionAsync(CardTransaction transaction)
    {
        _context.CardTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }
    public async Task UpdateAsync(Card card)
    {
        _context.Cards.Update(card);
        await _context.SaveChangesAsync();
    }

    public async Task<CardTransaction?> GetCardTransactionByIdempotencyKey(string idempotencyKey)
    {
        return await _context.CardTransactions.SingleOrDefaultAsync(t => t.IdempotencyKey == idempotencyKey);
    }
}