namespace SmartCarWash.Application.DTOs;

public class FeedbackDto
{
    public Guid Id { get; set; }
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Guid UpdateBy { get; set; }
}

public class CreateFeedbackDto
{
    public Guid BookingId { get; set; }
    public Guid CustomerId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}

public class UpdateFeedbackDto
{
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
