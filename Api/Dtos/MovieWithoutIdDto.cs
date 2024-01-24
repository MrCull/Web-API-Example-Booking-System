﻿namespace Api.Dtos;

public record MovieWithoutIdDto
{
    public MovieWithoutIdDto(string title, string description, int durationMins, string genre, DateTime releaseDateUtc, MovieStatus movieStatus)
    {
        Title = title;
        Description = description;
        DurationMins = durationMins;
        Genre = genre;
        ReleaseDateUtc = releaseDateUtc;
        MovieStatus = movieStatus;
    }

    public string Title { get; set; }
    public string Description { get; set; }
    public int DurationMins { get; set; }
    public string Genre { get; set; }
    public DateTime ReleaseDateUtc { get; set; }
    public MovieStatus MovieStatus { get; set; }
}

public enum MovieStatus
{
    Available,
    NoLongerAvailable
}