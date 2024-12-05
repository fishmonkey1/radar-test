using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;
using System;

/// <summary>
/// Networked script the handles writing messages from over the network into its UI window.
/// </summary>
public class Chat : NetworkBehaviour
{
    [SerializeField] GameObject chatPanel;
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] TMP_Text chatText;

    public delegate void OnMessageReceived(ChatMessage message);
    /// <summary>
    /// For any scripts that want to know when the local player gets a message. Consider using this to play a sound when a message comes in.
    /// </summary>
    public event OnMessageReceived OnMessageArrival;

    /// <summary>
    /// A small struct to send over the network with all the player's information.
    /// </summary>
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

    /// <summary>
    /// Extra information about the type of message, mostly for determining how something gets displayed.
    /// </summary>
    [Serializable]
    public struct MessageContext
    {
        public MessageTypes messageType;
        public bool connection, disconnection; //Flag the message as part of a connection or disconnection message
        public MessageContext(MessageTypes type, bool connectionMessage = false, bool disconnectionMessage = false)
        {
            messageType = type;
            connection = connectionMessage;
            disconnection = disconnectionMessage;
        }
    }

    /// <summary>
    /// Arbitrary list of messages to append after a connection message.
    /// TODO: Yank these out into a JSON file we can load
    /// </summary>
    string[] connectionMessages = { "Everybody say hello!", "SERVER hopes you don't blow up. c:", "Watch this one, they're cool.", "Prepare yourselves for silliness.", "B)", "Good luck!", "Prepare for battle!" };
    /// <summary>
    /// Same idea as the connectionMessages, but for when they exit the server
    /// </summary>
    string[] disconnectMessages = { "Byeeeee~~!", "Oh no, we lost one!", "RIP.", "See you next time.", "D:" };

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
        CmdSendChatMessage(text, NetworkClient.connection.connectionId, PlayerProfile.LoadedProfileName, messageType);
    }

    /// <summary>
    /// Send the message across the network if the host didn't send it.
    /// </summary>
    /// <param name="text"></param>
    /// <param name="connectionID"></param>
    /// <param name="playerName"></param>
    /// <param name="messageType"></param>
    [Command(requiresAuthority = false)]
    public void CmdSendChatMessage(string text, int connectionID, string playerName, MessageTypes messageType)
    { //Server finds the player's name, appends the text to make the whole message, then sends the message to all clients
        ChatMessage message = new ChatMessage(text, playerName, connectionID, MessageTypes.ALL); //Just default to all for now
        chatText.text += BuildMessage(message); //Stick this on the end of the server's chatText area
        //Now we need to send this message to all the clients for display on their end
        RpcReceiveMessage(message);
    }

    /// <summary>
    /// This can only be called from the server and sends a message to all clients that is formatted differently from user chat messages.
    /// </summary>
    /// <param name="text">The text to display with server formatting.</param>
    /// <param name="messageType">For each type supplied the message will appear in those chats.</param>
    [Server]
    public void SendServerMessage(string text, MessageContext context)
    {
        MessageTypes messageType = context.messageType;
        string finishedMessage = text; //Kick things off here
        //Then start with a basic message to build
        ChatMessage message = new ChatMessage(finishedMessage, "Server", 0, MessageTypes.SERVER);
        //
        finishedMessage = BuildMessage(message, context.connection, context.disconnection); //Pass the context stuff along
        finishedMessage = FormatMessage(messageType, finishedMessage); //Now format it
        message.messageText = finishedMessage; //Update the struct's text with the built and formatted string
        chatText.text += finishedMessage; //Spit out the message in the host's chat
        RpcReceiveMessage(message); //Then yeet the struct across the network
    }

    /// <summary>
    /// Clients format their received messages and then display them in the chat UI.
    /// </summary>
    /// <param name="message"></param>
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

    /// <summary>
    /// Format the message based on the context it came with.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="addConnectionMessage"></param>
    /// <param name="addDisconnectionMessage"></param>
    /// <returns></returns>
    string BuildMessage(ChatMessage message, bool addConnectionMessage = false, bool addDisconnectionMessage = false)
    {
        if (message.messageType == MessageTypes.ALL) //This was the default case before
            return message.PlayerName + ": " + message.messageText + "\n";
        if (message.messageType == MessageTypes.SERVER)
        {
            string extraMessage = "";
            if (addConnectionMessage)
            {
                extraMessage = connectionMessages[UnityEngine.Random.Range(0, connectionMessages.Length)];
            }
            if (addDisconnectionMessage)
            {
                extraMessage = disconnectMessages[UnityEngine.Random.Range(0, disconnectMessages.Length)];
            }
            if (addConnectionMessage && addDisconnectionMessage) //Be mean to the programmer if they mess up
                Debug.LogWarning("You asked to build a message with both add and disconnect messages. Check your code, stupid.");
            return message.messageText + " " + extraMessage + "\n"; //The message is already good to go on server messages, so just write it into the textfield, unless they want extra messages which gets appended

        }
        else //Be mean to the programmer >:)
            return "MessageType is not supported, dumbass. Check your parameters and try again.";
    }

    /// <summary>
    /// Format the message from the network based on the context it was sent with.
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="text"></param>
    /// <returns></returns>
    string FormatMessage(MessageTypes messageType, string text)
    {
        string formattedMessage = "";
        if (messageType == MessageTypes.SERVER) //Render text with Server as the name for these kinds of messages
            formattedMessage = $"Server: <i>{text}</i>"; //Try out using rich text tags to render server messages in italics
        if (messageType == MessageTypes.ERROR) //Error messages should be bolded and italicized when sent
            formattedMessage = $"Error: <b><i>{text}</i></b>";
        return formattedMessage;
    }
}
