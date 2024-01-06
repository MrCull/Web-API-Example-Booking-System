namespace CinemaBooking.Model;

public class MovieChainException : Exception
{
    public MovieChainException(string message) : base(message)
    {
    }
}

public class MovieException : MovieChainException
{
    public MovieException(string message) : base(message)
    {
    }
}

public class TheaterException : MovieChainException
{
    public TheaterException(string message) : base(message)
    {
    }
}

public class ShowtimeException : MovieChainException
{
    public ShowtimeException(string message) : base(message)
    {
    }
}