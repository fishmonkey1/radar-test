using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

public class Chat : NetworkBehaviour
{
    [SerializeField] GameObject chatPanel;
    [SerializeField] InputField chatInput;
    [SerializeField] TMP_Text chatText;

    public delegate void OnMessageReceived(ChatMessage message);
    public OnMessageReceived OnMessageArrival;

    [Serializable]
    public struct ChatMessage
    {
        public string messageText;
        public uint senderNetID;
        public MessageTypes messageType;

        public ChatMessage(string message, uint ID, MessageTypes type)
        {
            messageText = message;
            senderNetID = ID;
            messageType = type;
        }
    }

    [Flags, Serializable]
    public enum MessageTypes
    {
        ALL,
        TEAM,
        SQUAD,
        SERVER,
        ERROR
    }

    // Start is called before the first frame update
    void Start()
    {
        chatInput.onSubmit.AddListener((text) => SendChatMessage(text));
    }

    void SendChatMessage(string text)
    { //This happens on the local machine, giving us a chance to collect info before sending to the server
        CmdSendChatMessage(text, NetworkClient.localPlayer.netId);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendChatMessage(string text, uint netID)
    { //Server finds the player's name, appends the text to make the whole message, then sends the message to all clients
        TankRoomPlayer player = TankRoomManager.singleton.GetRoomPlayerByID(netID);
        ChatMessage message = new ChatMessage(text, netID, MessageTypes.ALL); //Just default to all for now
        chatText.text += BuildMessage(message); //Stick this on the end of the server's chatText area
        //Now we need to send this message to all the clients for display on their end
        RpcReceiveMessage(message);
    }

    [ClientRpc]
    public void RpcReceiveMessage(ChatMessage message)
    {
        TankRoomPlayer player = TankRoomManager.singleton.GetRoomPlayerByID(message.senderNetID);
        if (!isServer)
        {
            chatText.text += BuildMessage(message);
        }
        if (OnMessageArrival != null)
        {
            OnMessageArrival.Invoke(message);
        }
    }

    string BuildMessage(ChatMessage message)
    {
        TankRoomPlayer player = TankRoomManager.singleton.GetRoomPlayerByID(message.senderNetID);
        return player.PlayerName + ": " + message.messageText + "\n";
    }
}
