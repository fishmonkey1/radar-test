using System.Collections.Generic;
using Mirror;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json;

public class CustomRoomsManager : NetworkManager
{
    public static CustomRoomsManager Instance { get; private set; }

    // Player management
    public Dictionary<uint, PlayerProfile> ConnectedPlayers = new Dictionary<uint, PlayerProfile>();
    public List<uint> ReadyPlayers = new List<uint>();
    private uint nextPlayerId = 1;

    // Game state
    [SerializeField] private int minPlayersToStart = 2;
    [SerializeField] private int maxPlayers = 8;
    private bool gameInProgress = false;

    // Events
    public delegate void PlayerEventHandler(uint playerId, PlayerProfile profile);
    public event PlayerEventHandler OnPlayerConnected;
    public event PlayerEventHandler OnPlayerDisconnected;
    public event PlayerEventHandler OnPlayerReady;

    public delegate void GameStateChangedHandler();
    public event GameStateChangedHandler OnAllPlayersReady;
    public event GameStateChangedHandler OnGameStarted;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void Start()
    {
        base.Start();

        NetworkServer.RegisterHandler<PlayerConnectedMessage>(OnServerReceivePlayerProfile);
        NetworkServer.RegisterHandler<PlayerReadyMessage>(OnServerReceivePlayerReady);

        NetworkClient.RegisterHandler<PlayerConnectedMessage>(OnClientReceivePlayerId);
        NetworkClient.RegisterHandler<PlayerListMessage>(OnClientReceivePlayerList);
        NetworkClient.RegisterHandler<ChatMessage>(OnClientReceiveChatMessage);
    }

    #endregion

    #region Server Connection Management

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        base.OnServerConnect(conn);

        if (gameInProgress || ConnectedPlayers.Count >= maxPlayers)
        {
            conn.Disconnect();
            Debug.Log($"Rejected connection: {(gameInProgress ? "Game in progress" : "Server full"}");
        }
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        uint playerId = GetPlayerIdFromConnection(conn);

        if (playerId != 0 && ConnectedPlayers.TryGetValue(playerId, out PlayerProfile profile))
        {
            ConnectedPlayers.Remove(playerId);
            ReadyPlayers.Remove(playerId);

            OnPlayerDisconnected?.Invoke(playerId, profile);
            ChatManager.SendSystemMessage($"{profile.PlayerName} disconnected");
            SendPlayerListToAllClients();
            CheckAllPlayersReady();
        }

        base.OnServerDisconnect(conn);
    }

    private uint GetPlayerIdFromConnection(NetworkConnectionToClient conn)
    {
        return ConnectedPlayers.FirstOrDefault(p =>
            p.Value.Holder != null &&
            p.Value.Holder.netIdentity.connectionToClient == conn
        ).Key;
    }

    #endregion

    #region Server Message Handlers

    private void OnServerReceivePlayerProfile(NetworkConnectionToClient conn, PlayerConnectedMessage message)
    {
        uint playerId = nextPlayerId++;
        var profile = JsonConvert.DeserializeObject<PlayerProfile>(message.ProfileJson);

        // Attach ProfileHolder
        var holder = conn.identity.GetComponent<ProfileHolder>();
        holder.Profile = profile;
        profile.Holder = holder;

        ConnectedPlayers[playerId] = profile;

        // Send confirmation with assigned ID
        conn.Send(new PlayerConnectedMessage
        {
            PlayerId = playerId,
            ProfileJson = message.ProfileJson
        });

        OnPlayerConnected?.Invoke(playerId, profile);
        ChatManager.SendSystemMessage($"{profile.PlayerName} joined");
        SendPlayerListToAllClients();
    }

    private void OnServerReceivePlayerReady(NetworkConnectionToClient conn, PlayerReadyMessage message)
    {
        if (!ConnectedPlayers.TryGetValue(message.PlayerId, out PlayerProfile profile))
            return;

        var newRole = JsonConvert.DeserializeObject<Role>(message.RoleJson);
        profile.SelectRole(newRole);

        if (newRole.Name != CrewRoles.UnassignedRole.Name)
        {
            if (!ReadyPlayers.Contains(message.PlayerId))
            {
                ReadyPlayers.Add(message.PlayerId);
                OnPlayerReady?.Invoke(message.PlayerId, profile);
                ChatManager.SendSystemMessage($"{profile.PlayerName} ready as {newRole.Name}");
            }
        }
        else
        {
            ReadyPlayers.Remove(message.PlayerId);
        }

        CheckAllPlayersReady();
    }

    private void SendPlayerListToAllClients()
    {
        var msg = new PlayerListMessage
        {
            PlayerId = 0,
            PlayerProfilesJson = ConnectedPlayers.Values
                .Select(p => JsonConvert.SerializeObject(p))
                .ToList()
        };

        NetworkServer.SendToAll(msg);
    }

    public void CheckAllPlayersReady()
    {
        if (ConnectedPlayers.Count >= minPlayersToStart &&
            ReadyPlayers.Count == ConnectedPlayers.Count)
        {
            OnAllPlayersReady?.Invoke();
        }
    }

    public void StartGame()
    {
        if (ReadyPlayers.Count == ConnectedPlayers.Count && ConnectedPlayers.Count >= minPlayersToStart)
        {
            gameInProgress = true;
            OnGameStarted?.Invoke();
            ChatManager.SendSystemMessage("Game starting!");
            // ServerChangeScene("GameScene");
        }
    }

    #endregion

    #region Client Methods

    public void SendPlayerProfileToServer(PlayerProfile profile)
    {
        NetworkClient.Send(new PlayerConnectedMessage
        {
            ProfileJson = JsonConvert.SerializeObject(profile)
        });
    }

    public void SendPlayerReady(uint playerId, Role role)
    {
        NetworkClient.Send(new PlayerReadyMessage
        {
            PlayerId = playerId,
            RoleJson = JsonConvert.SerializeObject(role)
        });
    }

    private void OnClientReceivePlayerId(PlayerConnectedMessage message)
    {
        var profile = JsonConvert.DeserializeObject<PlayerProfile>(message.ProfileJson);
        profile.Holder = NetworkClient.localPlayer.GetComponent<ProfileHolder>();
        profile.Holder.Profile = profile;
    }

    private void OnClientReceivePlayerList(PlayerListMessage message)
    {
        var profiles = message.PlayerProfilesJson
            .Select(JsonConvert.DeserializeObject<PlayerProfile>)
            .ToList();

        // Update UI with profiles
        // LobbyUI.Instance.UpdatePlayerList(profiles);
    }

    private void OnClientReceiveChatMessage(ChatMessage message)
    {
        // ChatUI.Instance.AddMessage(
        //     message.IsSystemMessage ? "[SYSTEM]" : message.SenderName,
        //     message.Content,
        //     message.RoleName
        // );
    }

    #endregion
}