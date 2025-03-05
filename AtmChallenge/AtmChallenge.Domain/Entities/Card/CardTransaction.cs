using System.ComponentModel.DataAnnotations;

namespace AtmChallenge.Domain.Entities.Card;

public enum TransactionType
{
    Deposit,
    Withdrawal,
}

public class CardTransaction
{
    [Key] 
    public int Id { get; set; }
    [Required]
    public Double Amount { get; set; }
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public Card Card { get; set; }
    public String AtmLocation { get; set; }
    [Required]
    public string IdempotencyKey { get; set; }
    [Required]
    public TransactionType Type { get; set; }
}