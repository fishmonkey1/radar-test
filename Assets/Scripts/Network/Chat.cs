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

    /// <summary>
    /// Enum for the different chat message channels that can be used. For the life of me, I can't remember why I marked this with Flags...
    /// </summary>
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
        //TODO: Add variables to monitor which channels the Chat script is listening to.
        chatInput.onSubmit.AddListener((text) => SendChatMessage(text, MessageTypes.ALL)); //We default all messages to the ALL channel until more chat support is written
    }

    /// <summary>
    /// This function is triggered by the Chat script's InputField and captures the text and the channel it is meant to go on.
    /// </summary>
    /// <param name="text">The message to be sent over the network</param>
    /// <param name="messageType">Which channel to send the message on</param>
    void SendChatMessage(string text, MessageTypes messageType)
    { //This happens on the local machine, giving us a chance to collect info before sending to the server
        chatInput.text = ""; //Clear the text out of the inputField
        CmdSendChatMessage(text, NetworkClient.connection.connectionId, PlayerInfo.localPlayerName, messageType);
    }

    [Command(requiresAuthority = false)]
    public void CmdSendChatMessage(string text, int connectionID, string playerName, MessageTypes messageType)
    { //Server finds the player's name, appends the text to make the whole message, then sends the message to all clients
        ChatMessage message = new ChatMessage(text, playerName, connectionID, MessageTypes.ALL); //Just default to all for now
        chatText.text += BuildMessage(message); //Stick this on the end of the server's chatText area
        //Now we need to send this message to all the clients for display on their end
        RpcReceiveMessage(message);
    }

    public void SendServerMessage(string text, MessageTypes messageType)
    {
        string fullMessage = "Unassigned";
        if (messageType == MessageTypes.SERVER) //Render text with Server as the name for these kinds of messages
            fullMessage = $"Server: <i>{text}</i>"; //Try out using rich text tags to render server messages in italics
        if (messageType == MessageTypes.ERROR) //Error messages should be bolded and italicized when sent
            fullMessage = $"Error: <b><i>{text}</i></b>";
        ChatMessage message = new ChatMessage(fullMessage, "Server", 0, MessageTypes.SERVER);
        chatText.text += BuildMessage(message);
        RpcReceiveMessage(message);
    }

    [ClientRpc]
    public void RpcReceiveMessage(ChatMessage message)
    { //The server send out a message to all clients so they render the message
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
        if (message.messageType == MessageTypes.ALL) //This was the default case before
            return message.PlayerName + ": " + message.messageText + "\n";
        if (message.messageType == MessageTypes.SERVER)
            return message.messageText + "\n"; //The message is already good to go on server messages, so just write it into the textfield
        else //Be mean to the programmer >:)
            return "MessageType is not supported, dumbass. Check your parameters and try again.";
    }
}
