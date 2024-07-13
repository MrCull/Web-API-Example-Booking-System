using Domain.Aggregates.TheaterAggregate;
using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.ShowtimeAggregate;

internal class SeatReservation : ISeatReservation
{
    public SeatReservation(Guid id, DateTime reservationTimeUtc, DateTime reservationTimeoutUtc, Showtime showtime, List<Seat> seats)
    {
        Id = id;
        ReservationTimeUtc = reservationTimeUtc;
        ReservationTimeoutUtc = reservationTimeoutUtc;
        Status = ReservationStatus.Reserved;
        Seats = seats;
        SetShowtime(showtime);
    }

    public Guid Id { get; private set; }

    [Required]
    public int ShowtimeId { get; private set; }

    public DateTime ReservationTimeUtc { get; private set; }

    public DateTime? ReservationTimeoutUtc { get; private set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Total price must be greater than zero.")]
    public decimal Price { get; private set; }

    [Required]
    public ReservationStatus Status { get; private set; }

    // Navigation properties
    internal IShowtime Showtime { get; private set; }

    public List<Seat> Seats { get; private set; }

    internal void Confirm()
    {
        Status = ReservationStatus.Confirmed;
        ReservationTimeoutUtc = null;
    }

    internal bool IsReservationCnfirmedOrAcitivePending()
        => Status == ReservationStatus.Confirmed || !IsTimedOut();

    internal bool IsReserved()
        => Status == ReservationStatus.Reserved;

    internal bool IsTimedOut()
        => Status == ReservationStatus.Reserved && ReservationTimeoutUtc < DateTime.UtcNow;

    public void SetReservationTimeout(DateTime reservationTimeoutUtc)
    {
        ReservationTimeoutUtc = reservationTimeoutUtc;
    }

    public void SetShowtime(IShowtime showtime)
    {
        if (showtime is not null)
        {
            Showtime = showtime;
            ShowtimeId = showtime.Id;
            Price = showtime.Price * Seats.Count;
        }
    }
}

public enum ReservationStatus
{
    Reserved,
    Confirmed
}