using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

public class LobbyManager : NetworkBehaviour
{
    private NetworkList<PlayerStates> players;
    [SerializeField] private LobbyPlayerCard[] playerCards; // Array of PlayerCard objects

    private void Awake()
    {
        players = new NetworkList<PlayerStates>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            players.OnListChanged += HandlePlayerStateChange;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            // Add existing connected clients to the player list
            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayerStateChange;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected: {clientId}");
        players.Add(new PlayerStates(clientId, $"Player {clientId}"));
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    private void HandlePlayerStateChange(NetworkListEvent<PlayerStates> changeEvent)
    {
        // Update the player cards based on the current player list
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (i < players.Count)
            {
                // Check if the current player is the local player
                bool isLocalPlayer = players[i].ClientId == NetworkManager.Singleton.LocalClientId;
                playerCards[i].UpdateDisplay(players[i].PlayerName, isLocalPlayer); // Update the card with the player's name and whether it's the local player
            }
            else
            {
                playerCards[i].DisableDisplay(); // Disable the card if there's no player
            }
        }
    }
}