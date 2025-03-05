namespace AtmChallenge.Application.DTOs;

public class CardOperationsResponse
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string AtmLocation { get; set; }
    public int Type { get; set; }
}