using System.ComponentModel.DataAnnotations;

namespace Domain.Aggregates.TheaterAggregate;

public record Seat : ISeat
{
    public Seat(string seatNumber, Guid? id = null)
    {
        Id = id ?? Guid.NewGuid();
        SeatNumber = seatNumber;
    }

    [Required]
    public Guid Id { get; private set; }

    [Required]
    [StringLength(10, ErrorMessage = "Seat number length can't be more than 10 characters.")]
    public string SeatNumber { get; private set; }
}