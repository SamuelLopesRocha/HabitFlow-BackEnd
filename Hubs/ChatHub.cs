using Microsoft.AspNetCore.SignalR;

public class ChatHub : Hub
{
    public async Task EntrarNoChat(string chatId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, chatId);
    }

    public async Task SairDoChat(string chatId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, chatId);
    }
}