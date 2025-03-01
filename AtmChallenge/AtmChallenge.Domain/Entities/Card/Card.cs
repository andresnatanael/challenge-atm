using System.Transactions;
using System.ComponentModel.DataAnnotations;

namespace AtmChallenge.Domain.Entities.Card;

public enum CardType
{
    Debit,
    Credit,
    Prepaid
}

public enum Network
{
    Banelco,
    RedLink,
    Visa,
    AmericanExpress
}

public class Card
{
    [Key] public string NumberHash { get; set; }
    public string PinHash { get; set; }
    public Network Network { get; set; }
    public CardType Type { get; set; }
    public User User { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockoutEnd { get; set; }
    public Double Balance { get; set; }
    public List<Transaction> Transactions { get; set; }
}