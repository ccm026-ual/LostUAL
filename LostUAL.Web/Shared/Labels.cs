using LostUAL.Contracts.Claims;
using LostUAL.Contracts.Posts;
using LostUAL.Contracts.Reports;

public static class Labels
{
    public static string PostTypeLabel(PostType t) => t switch
    {
        PostType.Lost => "Perdido",
        PostType.Found => "Encontrado",
        _ => t.ToString()
    };
    public static string ReportStatusLabel(ReportStatus s) => s switch
    {
        ReportStatus.Open => "Abierto",
        ReportStatus.Dismissed => "Desestimado",
        ReportStatus.ActionTaken => "Acción tomada",
        _ => s.ToString()
    };

    public static string ClaimStatusLabel(ClaimStatus s) => s switch
    {
        ClaimStatus.Pending => "Pendiente",
        ClaimStatus.Accepted => "Aceptada",
        ClaimStatus.Rejected => "Rechazada",
        ClaimStatus.Withdrawn => "Retirada",
        _ => s.ToString()
    };

    public static string PostStatusLabel(PostStatus s) => s switch
    {
        PostStatus.Open => "Abierto",
        PostStatus.InClaim => "En reclamación",
        PostStatus.Closed => "Cerrado",
        PostStatus.Resolved => "Resuelto",
        _ => s.ToString()
    };
}
