namespace LostUAL.Contracts.Chat;

public sealed class MessageDto
{
    public int Id { get; set; }
    public string SenderUserId { get; set; } = "";
    public string? SenderEmail { get; set; }
    public string Body { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; }
}

public sealed class SendDto
{
    public string Body { get; set; } = "";
}
