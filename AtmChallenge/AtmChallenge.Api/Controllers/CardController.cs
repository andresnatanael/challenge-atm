using System.Security.Claims;
using AtmChallenge.Application.DTOs;
using AtmChallenge.Application.Exceptions;
using AtmChallenge.Application.Interfaces;
using AtmChallenge.Domain.Entities.Card;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
[ApiController]
[Route("api/v1/card")]
public class CardController : ControllerBase
{
    private readonly ICardService _cardService;
    private readonly ILogger<CardController> _logger;

    public CardController(ICardService cardService, ILogger<CardController> logger)
    {
        _cardService = cardService;
        _logger = logger;
    }
    
    [HttpGet("operations")]
    public async Task<IActionResult> GetOperations([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        string encryptedCard = GetEncryptedCardNumber();
        if (encryptedCard == null) return Unauthorized("❌ Invalid token.");

        var (transactions, totalRecords) = await _cardService.GetTransactions(encryptedCard, page, pageSize);

        if (transactions == null || transactions.Count == 0)
            return NotFound("❌ No transactions found.");
        
        

        var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);
        
        var transactionResponses = transactions.Select(transaction => new CardOperationResponse
        {
            Id = transaction.Id,
            Amount = transaction.Amount,
            Date = transaction.Date,
            AtmLocation = transaction.AtmLocation,
            Type = transaction.Type.ToString()
        }).ToList();
        
        return Ok(new
        {
            TotalRecords = totalRecords,
            TotalPages = totalPages,
            CurrentPage = page,
            PageSize = pageSize,
            Transactions = transactionResponses
        });
    }

    /// 🔹 **Get Card Balance (Extracts EncryptedCard from JWT)**
    [HttpGet("balance")]
    public async Task<IActionResult> GetBalance()
    {
        string encryptedCard = GetEncryptedCardNumber();
        if (encryptedCard == null)
        {
            _logger.LogError("The ClaimsPrincipal does not contain an EncryptedCard.");
            return Unauthorized("❌ Invalid token.");
        }

        try
        {
            double balance = await _cardService.GetBalance(encryptedCard);
            return Ok(new { Balance = balance });
        }
        catch (CardNotFoundException ex)
        {
            _logger.LogError(ex, "Card not found.");
            return NotFound("❌ Card not found.");
        }
    }

    [HttpPost("withdrawal")]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
    {
        string encryptedCard = GetEncryptedCardNumber();
        if (encryptedCard == null)
        {
            return Unauthorized("❌ Invalid token.");
        }
        
        try
        {
            var result = await _cardService.Withdraw(encryptedCard, request.Amount, idempotencyKey);
            CardTransaction transaction = result.Item1;
            double newBalance = result.Item2;
            return StatusCode(201, new
            {
                Message = "✅ Withdrawal processed successfully!",
                TransactionId = transaction.Id,
                NewBalance = newBalance
            });
        }
        catch (InsufficientBalanceException ex)
        {
            _logger.LogError(ex, "Insufficient funds.");
            return BadRequest("❌ Insufficient funds.");
        }
        catch (TxDuplicatedException ex)
        {
            _logger.LogWarning("The Transaction was already submitted");
            _logger.LogWarning("The transaction was already submitted for the idempotencyKey: {key}",
                idempotencyKey);
            return StatusCode(202, new
            {
                Message = "Withdrawal already processed. " + ex.Message,
            });
        }
    }
    
    
    /// 🔹 **Extracts EncryptedCard from JWT Claims**
    private string GetEncryptedCardNumber()
    {
        return User.Claims.FirstOrDefault(c => c.Type == "EncryptedCard")?.Value;
    }
}