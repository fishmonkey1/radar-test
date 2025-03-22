using System.Collections.Generic;
using Mirror;
using Newtonsoft.Json;

public interface IBaseNetworkMessage
{
    uint PlayerId { get; set; }
}

public struct PlayerConnectedMessage : NetworkMessage, IBaseNetworkMessage
{
    public uint PlayerId { get; set; }
    public string ProfileJson;
}

public struct PlayerReadyMessage : NetworkMessage, IBaseNetworkMessage
{
    public uint PlayerId { get; set; }
    public string RoleJson;
}

public struct PlayerListMessage : NetworkMessage, IBaseNetworkMessage
{
    public uint PlayerId { get; set; }
    public List<string> PlayerProfilesJson;
}

public struct ChatMessage : NetworkMessage, IBaseNetworkMessage
{
    public uint PlayerId { get; set; }
    public string SenderName;
    public string Content;
    public bool IsSystemMessage;
    public string RoleName;
}

public struct ServerOnlyMessage : NetworkMessage, IBaseNetworkMessage
{
    public uint PlayerId { get; set; }
    public string Command;
    public Dictionary<string, object> Parameters;
}

public static class NetworkMessagesExtensions
{
    public static void WriteRole(this NetworkWriter writer, Role role)
    {
        writer.WriteString(JsonConvert.SerializeObject(role));
    }

    public static Role ReadRole(this NetworkReader reader)
    {
        return JsonConvert.DeserializeObject<Role>(reader.ReadString());
    }
}