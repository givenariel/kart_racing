using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ServerManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string lobbySceneName = "Lobby";
    [SerializeField] private string characterSelectSceneName = "CharacterSelect";
    [SerializeField] private string gameplaySceneName = "Gameplay";

    public static ServerManager instance;
    private bool gameHasStarted;
    public Dictionary<ulong, ClientData> ClientData { get; private set; }
    private Lobby currentLobby;
    public string PlayerName { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Debug.LogError("Another instance of ServerManager already exists! Destroying this one.");
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        if (NetworkManager.Singleton != null)
        {
            DontDestroyOnLoad(NetworkManager.Singleton.gameObject);
        }
    }
    private async void Start()
    {
        Debug.Log("Initializing Unity Services...");
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Signing in anonymously...");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Signed in with Player ID: {AuthenticationService.Instance.PlayerId}");
        }
    }


    public void SetPlayerName(string name)
    {
        PlayerName = name;
        Debug.Log("Player name set to: " + PlayerName);
    }

    public async void StartHost()
    {
        try
        {
            Debug.Log("Attempting to start Host...");
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(8);
            string roomCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            Debug.Log($"Room Code Generated: {roomCode}");
            RoomData.RoomCode = roomCode;

            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
            );

            NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
            NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
            ClientData = new Dictionary<ulong, ClientData>();

            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("✅ Host started successfully!");
                await CreateLobby();
            }
            else
            {
                Debug.LogError("❌ Failed to start Host.");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Exception when starting Host: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private async Task CreateLobby()
    {
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = new Dictionary<string, DataObject>
                {
                    { "RoomCode", new DataObject(DataObject.VisibilityOptions.Public, RoomData.RoomCode) },
                    { "PlayerName", new DataObject(DataObject.VisibilityOptions.Public, PlayerName) }
                }
            };
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("GameLobby", 8, options);
            Debug.Log($"Lobby Created: {currentLobby.Id}, Room Code: {RoomData.RoomCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Lobby creation failed: {e.Message}");
        }
    }
    public async void StartClient(string inputRoomCode)
    {
        try
        {
            Debug.Log($"Attempting to join game with code: {inputRoomCode}");

            if (string.IsNullOrEmpty(inputRoomCode))
            {
                Debug.LogError("Room code is missing! Cannot join a relay.");
                return;
            }

            Debug.Log("Fetching UnityTransport...");
            UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport == null)
            {
                Debug.LogError("UnityTransport component is missing! Attach it to NetworkManager.");
                return;
            }

            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost)
            {
                Debug.LogError("Client/Host already running! Cannot start again.");
                return;
            }


            Debug.Log("Joining Relay...");
            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(inputRoomCode);
            Debug.Log($"Relay Joined! Server: {allocation.RelayServer.IpV4}:{allocation.RelayServer.Port}");

            transport.SetClientRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.HostConnectionData
            );

            Debug.Log("Relay Data Set Successfully! Waiting before StartClient...");
            await Task.Delay(500);  // Ensure relay data is set before starting client

            Debug.Log("Starting Client...");
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started successfully!");
            }
            else
            {
                Debug.LogError("Failed to start client. Check NetworkManager settings.");
            }
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError($"Relay Service Error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error when joining room: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        if (ClientData.Count >= 8 || gameHasStarted)
        {
            response.Approved = false;
            return;
        }
        response.Approved = true;
        response.CreatePlayerObject = true;
        response.Pending = false;

        ClientData[request.ClientNetworkId] = new ClientData(request.ClientNetworkId, PlayerName);
        Debug.Log($"Adding Player {request.ClientNetworkId} with name {PlayerName}");
    }

    private void OnNetworkReady()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientData.ContainsKey(clientId))
        {
            ClientData.Remove(clientId);
            Debug.Log($"Removed Player {clientId}");
        }
    }

    public void SetCharacter(ulong clientId, int characterId)
    {
        if (ClientData.TryGetValue(clientId, out ClientData data))
        {
            data.characterId = characterId;
            Debug.Log($"Player {clientId} selected character ID: {characterId}");
        }
    }

    public void GoToCharacterSelect()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("Moving to Character Selection...");
            NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
        }
    }

    public void StartGame()
    {
        gameHasStarted = true;

        // Debugging: Check if ClientData exists and has the right character IDs
        foreach (var client in ClientData)
        {
            Debug.Log($"[StartGame] ClientID: {client.Key}, CharacterID: {client.Value.characterId}");
        }

        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);

        //Destroy(gameObject);
    }

    public void CheckNetworkStatus()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.Log("NetworkManager is NULL! It might have been destroyed.");
            return;
        }

        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log("The game is running as a HOST.");
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            Debug.Log("The game is running as a CLIENT.");
        }
        else
        {
            Debug.Log("The game is NOT connected to any server.");
        }
    }

    public void SetGameplaySceneName(string sceneName)
    {
        gameplaySceneName = sceneName;
        Debug.Log("Gameplay scene name updated to: " + gameplaySceneName);
    }


}
