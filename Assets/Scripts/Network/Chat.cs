using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

public class Chat : NetworkBehaviour
{
    [SerializeField] GameObject chatPanel;
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] TMP_Text chatText;

    public delegate void OnMessageReceived(ChatMessage message);
    public OnMessageReceived OnMessageArrival;

    [Serializable]
    public struct ChatMessage
    {
        public string messageText;
        public string PlayerName;
        public int connectionID;
        public MessageTypes messageType;

        public ChatMessage(string message, string playerName, int ID, MessageTypes type)
        {
            messageText = message;
            PlayerName = playerName;
            connectionID = ID;
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
        chatInput.text = ""; //Clear the text out of the inputField
        CmdSendChatMessage(text, NetworkClient.connection.connectionId, PlayerInfo.localPlayerName);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendChatMessage(string text, int connectionID, string playerName)
    { //Server finds the player's name, appends the text to make the whole message, then sends the message to all clients
        ChatMessage message = new ChatMessage(text, playerName, connectionID, MessageTypes.ALL); //Just default to all for now
        chatText.text += BuildMessage(message); //Stick this on the end of the server's chatText area
        //Now we need to send this message to all the clients for display on their end
        RpcReceiveMessage(message);
    }

    [ClientRpc]
    public void RpcReceiveMessage(ChatMessage message)
    {
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
        return message.PlayerName + ": " + message.messageText + "\n";
    }
}
