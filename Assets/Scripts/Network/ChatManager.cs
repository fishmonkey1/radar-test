using UnityEngine;
using Mirror;

/// <summary>
/// Static manager class that handles all chat message functionality.
/// </summary>
public static class ChatManager
{
    /// <summary>
    /// Sends a message from a player to all clients.
    /// </summary>
    /// <param name="senderId">The unique ID of the sending player</param>
    /// <param name="senderName">The display name of the sending player</param>
    /// <param name="content">The message content</param>
    public static void SendPlayerMessage(uint senderId, string senderName, string content)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Cannot send chat message: Not a server");
            return;
        }

        ChatMessage message = new ChatMessage
        {
            PlayerId = senderId,
            SenderName = senderName,
            Content = content,
            IsSystemMessage = false
        };

        // Send to all clients
        NetworkServer.SendToAll(message);
    }

    /// <summary>
    /// Sends a system message to all clients.
    /// </summary>
    /// <param name="content">The system message content</param>
    public static void SendSystemMessage(string content)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Cannot send system message: Not a server");
            return;
        }

        ChatMessage message = new ChatMessage
        {
            PlayerId = 0, // System messages use ID 0
            SenderName = "System",
            Content = content,
            IsSystemMessage = true
        };

        // Send to all clients
        NetworkServer.SendToAll(message);
    }

    /// <summary>
    /// Sends a direct message to a specific player.
    /// </summary>
    /// <param name="targetPlayerId">The ID of the player to receive the message</param>
    /// <param name="senderId">The ID of the sending player (0 for system)</param>
    /// <param name="senderName">The name of the sending player</param>
    /// <param name="content">The message content</param>
    public static void SendDirectMessage(uint targetPlayerId, uint senderId, string senderName, string content)
    {
        if (!NetworkServer.active)
        {
            Debug.LogError("Cannot send direct message: Not a server");
            return;
        }

        // Find the connection for the target player
        NetworkConnectionToClient targetConn = null;
        foreach (var conn in NetworkServer.connections.Values)
        {
            // In a real implementation, you would have a way to look up the connection by player ID
            // This is just a placeholder
            if (conn != null)
            {
                // Some logic to find the right connection
                // targetConn = conn;
                // break;
            }
        }

        if (targetConn == null)
        {
            Debug.LogWarning($"Could not find connection for player ID: {targetPlayerId}");
            return;
        }

        ChatMessage message = new ChatMessage
        {
            PlayerId = senderId,
            SenderName = senderName,
            Content = $"[DM] {content}",
            IsSystemMessage = false
        };

        // Send only to target player
        targetConn.Send(message);

        // Also send a copy to the sender if it's not the system
        if (senderId != 0)
        {
            // Find sender connection and send copy
            // Similar logic as above
        }
    }
}