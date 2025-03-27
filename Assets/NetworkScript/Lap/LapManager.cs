using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LapManager : NetworkBehaviour
{
    public static LapManager Instance;
    public int totalLaps = 3;
    public List<Transform> checkpoints = new List<Transform>();
    public Dictionary<ulong, PlayerLapData> playerLapData = new Dictionary<ulong, PlayerLapData>();

    private NetworkList<ulong> playerPositions = new NetworkList<ulong>();

    public UIManager uiManager;

    public NetworkVariable<NetworkObjectReference> uiManagerRef = new NetworkVariable<NetworkObjectReference>();

    public NetworkVariable<int> pData = new NetworkVariable<int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        if (playerPositions == null)
        {
            playerPositions = new NetworkList<ulong>();
        }
    }


    private void Start()
    {
        if (playerPositions == null)
        {
            playerPositions = new NetworkList<ulong>();
        }

        checkpoints.Clear();
        foreach (Checkpoint checkpoint in FindObjectsByType<Checkpoint>(FindObjectsSortMode.None))
        {
            checkpoints.Add(checkpoint.transform);
        }
        checkpoints.Sort((a, b) => a.GetComponent<Checkpoint>().checkpointID.CompareTo(b.GetComponent<Checkpoint>().checkpointID));
        
    }

    public override void OnNetworkSpawn()
    {

        if (IsServer)
        {
            uiManager.SetLapManagerRefServerRpc(new NetworkObjectReference(GetComponent<NetworkObject>()));
            if (playerPositions == null)
            {
                playerPositions = new NetworkList<ulong>();
            }

            // Register all connected clients
            foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
            {
                RegisterPlayerServerRpc(clientId);
            }
            
        }
        NetworkObject netUi = uiManager.GetComponent<NetworkObject>();
        uiManagerRef.Value = new NetworkObjectReference(netUi);
    }

    [ServerRpc]
    public void RegisterPlayerServerRpc(ulong playerId)
    {
        if (!playerLapData.ContainsKey(playerId))
        {
            playerLapData[playerId] = new PlayerLapData();
            if (IsServer)
            {
                if (!playerPositions.Contains(playerId))
                {
                    playerPositions.Add(playerId);
                }
            }
            //pData.Value = playerLapData[playerId].position.Value;
        }
        
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerCrossedCheckpointServerRpc(ulong playerId, int checkpointIndex)
    {
        if (!playerLapData.ContainsKey(playerId)) return;

        var playerData = playerLapData[playerId];

        if (checkpointIndex == 0 && playerData.lastCheckpoint.Value == checkpoints.Count - 1)
        {
            playerData.lap.Value++;
            pData.Value = playerData.lap.Value;
            if (playerData.lap.Value > totalLaps)
            {
                Debug.Log($"Player {playerId} finished the race!");
            }
        }

        playerData.lastCheckpoint.Value = checkpointIndex;

        // 🔹 Update posisi pemain
        UpdatePlayerPositionsServerRpc();

        // 🔹 Kirim update ke semua client melalui UIManager
        
    }

    [ClientRpc]
    private void UpdatePlayerPositionsClientRpc(ulong playerId, int checkpointIndex, int lap, int position)
    {
        Debug.Log($"Client: Player {playerId} reached checkpoint {checkpointIndex}");

        if (uiManagerRef.Value.TryGet(out NetworkObject netObj) && netObj != null)
        {
            uiManager = netObj.GetComponent<UIManager>();
            uiManager.UpdateUIClientRpc(playerId, lap, position);
        }
        
    }
    [ServerRpc]
    private void UpdatePlayerPositionsServerRpc()
    {
        Debug.Log("cobacekPos");
        if (!IsServer) return;

        var sortedPlayers = new List<ulong>(playerLapData.Keys);
        sortedPlayers.Sort((a, b) =>
        {
            int lapCompare = playerLapData[b].lap.Value.CompareTo(playerLapData[a].lap.Value);
            if (lapCompare != 0) return lapCompare;
            return playerLapData[b].lastCheckpoint.Value.CompareTo(playerLapData[a].lastCheckpoint.Value);
        });

        playerPositions.Clear();
        for (int i = 0; i < sortedPlayers.Count; i++)
        {
            ulong playerId = sortedPlayers[i];

            int previousPosition = playerLapData[playerId].position.Value; // Get old position
            int newPosition = i + 1; // New position based on sorted order

            if (previousPosition != newPosition)
            {
                Debug.Log($"Player {playerId} is updating position from {previousPosition} to {newPosition}");
            }

            playerPositions.Add(playerId);
            playerLapData[playerId].position.Value = newPosition;
            UpdatePlayerPositionsClientRpc(playerId, playerLapData[playerId].lastCheckpoint.Value, playerLapData[playerId].lap.Value, playerLapData[playerId].position.Value);
            Debug.Log("playeridd" + playerId);
        }
        Debug.Log("countsortPlayer" + sortedPlayers.Count);
    }


    public IReadOnlyDictionary<ulong, PlayerLapData> PlayerLapData => playerLapData;
}

// ✅ PlayerLapData class (inside LapManager.cs)
public class PlayerLapData
{
    public NetworkVariable<int> lap = new NetworkVariable<int>(1);
    public NetworkVariable<int> lastCheckpoint = new NetworkVariable<int>(0);
    public NetworkVariable<int> position = new NetworkVariable<int>(1);
}
