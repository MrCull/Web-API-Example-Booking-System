using System.ComponentModel.DataAnnotations;

namespace CinemaBooking.Model;

public class Seat(string seatNumber)
{
    [Required]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [Required]
    [StringLength(10, ErrorMessage = "Seat number length can't be more than 10 characters.")]
    public string SeatNumber { get; private set; } = seatNumber;
}