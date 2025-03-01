using System.ComponentModel.DataAnnotations;

namespace AtmChallenge.Domain.Entities.Card;

public enum TransactionType
{
    Deposit,
    Withdrawal,
}

public class CardTransaction
{
    [Key] public int Id { get; set; }
    public Double Amount { get; set; }
    public DateTime Date { get; set; }
    public Card Card { get; set; }
    public String AtmLocation { get; set; }
    public TransactionType Type { get; set; }
}