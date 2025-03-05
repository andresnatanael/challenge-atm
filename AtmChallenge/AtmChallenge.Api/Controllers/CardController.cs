using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AtmChallenge.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AtmChallenge.Application.Services;
using AtmChallenge.Infrastructure.Repositories;

[Authorize] // Require authentication for all card operations
[ApiController]
[Route("api/v1/card")]
public class CardController : ControllerBase
{
    private readonly CardRepository _cardRepository;

    public CardController(CardRepository cardRepository)
    {
        _cardRepository = cardRepository;
    }

    /// 🔹 **Get Card Balance (Extracts EncryptedCard from JWT)**
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        string encryptedCard = GetEncryptedCardNumber();
        if (encryptedCard == null) return Unauthorized("❌ Invalid token.");

        var card = await _cardRepository.GetCardByHash(encryptedCard);
        if (card == null) return NotFound("❌ Card not found.");

        return Ok(new { Balance = card.Balance });
    }

    /// 🔹 **Withdraw Money (Extracts EncryptedCard from JWT)**
    [HttpPost("withdraw")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request)
    {
        string encryptedCard = GetEncryptedCardNumber();
        if (encryptedCard == null) return Unauthorized("❌ Invalid token.");

        var card = await _cardRepository.GetCardByHash(encryptedCard);
        if (card == null) return NotFound("❌ Card not found.");
        if (card.Balance < request.Amount) return BadRequest("❌ Insufficient funds.");

        card.Balance -= request.Amount;
        await _cardRepository.UpdateAsync(card);

        return Ok(new { Message = "✅ Withdrawal successful.", NewBalance = card.Balance });
    }

    /* 🔹 **Get Card Operations (Extracts EncryptedCard from JWT)**
    [HttpGet("operations")]
    public async Task<IActionResult> GetOperations()
    {
        string encryptedCard = GetEncryptedCardNumber();
        if (encryptedCard == null) return Unauthorized("❌ Invalid token.");

        var transactions = await _cardRepository.GetCardTransactionsAsync(encryptedCard);
        if (transactions == null || transactions.Count == 0) return NotFound("❌ No transactions found.");

        return Ok(transactions);
    }*/

    /// 🔹 **Extracts EncryptedCard from JWT Claims**
    private string GetEncryptedCardNumber()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "EncryptedCard")?.Value;
    }

    private string GetUserId()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "UserId")?.Value;
    }

    private string GetUsername()
    {
        return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
    }
}