namespace LostUAL.Contracts.Notifications;

public enum NotificationType
{
    NewClaim = 0,
    NewMessage = 1,
    ClaimAccepted = 2,
    ClaimStandby = 3,
    ClaimReactivated = 4,
    ResolutionMarked = 5,
    Resolved = 6,
    PostClosed = 7
}
