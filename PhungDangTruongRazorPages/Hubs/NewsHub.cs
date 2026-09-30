using Microsoft.AspNetCore.SignalR;

namespace PhungDangTruongRazorPages.Hubs;

/// <summary>
/// Real-time channel for news article changes. The server pushes
/// "NewsChanged"(action, articleId, title, byName) to every connected page.
/// </summary>
public class NewsHub : Hub
{
    public const string NewsChanged = "NewsChanged";
}
