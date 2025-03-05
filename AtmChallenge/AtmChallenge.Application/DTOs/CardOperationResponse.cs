namespace AtmChallenge.Application.DTOs;

public class CardOperationResponse
{
    public int Id { get; set; }
    public double Amount { get; set; }
    public DateTime Date { get; set; }
    public string AtmLocation { get; set; }
    public string Type { get; set; }
}