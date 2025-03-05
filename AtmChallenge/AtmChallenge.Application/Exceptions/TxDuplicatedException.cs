namespace AtmChallenge.Application.Exceptions;

public class TxDuplicatedException : Exception
{
    public TxDuplicatedException(string message) 
        : base(message)
    {
    }
}