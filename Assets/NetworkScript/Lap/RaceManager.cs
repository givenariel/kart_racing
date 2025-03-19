using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;
    public int totalLapsToWin = 3;
    [SerializeField] private List<string> debugPlayerPositions = new List<string>();
    [SerializeField] private List<string> debugRegisteredPlayers = new List<string>();
    private Dictionary<ulong, int> playerLaps = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> playerCheckpoints = new Dictionary<ulong, int>();
    private Dictionary<ulong, float> checkpointTimestamps = new Dictionary<ulong, float>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ulong localPlayerID = NetworkManager.Singleton.LocalClientId;
        RegisterPlayer(localPlayerID);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerLaps.Clear();
            playerCheckpoints.Clear();
            checkpointTimestamps.Clear();

            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                RegisterPlayer(client.ClientId);
            }
        }
    }

    public void RegisterPlayer(ulong playerID)
    {
        Debug.Log($"[RaceManager] Attempting to register Player {playerID}");

        if (!playerLaps.ContainsKey(playerID))
        {
            playerLaps[playerID] = 0;
            playerCheckpoints[playerID] = -1;
            checkpointTimestamps[playerID] = Time.time;

            // Add to Inspector List
            debugRegisteredPlayers.Add($"Player {playerID}");

            Debug.Log($"[RaceManager] Successfully registered Player {playerID}");
        }
        else
        {
            Debug.LogWarning($"[RaceManager] Player {playerID} is already registered!");
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCheckpointServerRpc(ulong playerID, int checkpointID)
    {
        if (!playerCheckpoints.ContainsKey(playerID)) return;

        playerCheckpoints[playerID] = checkpointID;
        checkpointTimestamps[playerID] = Time.time;

        UpdatePositionsClientRpc(); // Ensure this is called
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlayerCompletedLapServerRpc(ulong playerID, int lap)
    {
        if (!playerLaps.ContainsKey(playerID)) return;

        playerLaps[playerID] = lap;

        if (lap >= totalLapsToWin)
        {
            DeclareWinner(playerID);
        }

        UpdatePositionsClientRpc();
    }

    private List<ulong> GetPlayerPositions()
    {
        Debug.Log("[RaceManager] Fetching player positions...");

        List<ulong> sortedPlayers = new List<ulong>(playerLaps.Keys);

        foreach (var id in sortedPlayers)
        {
            Debug.Log($"[RaceManager] Player {id} is in the race.");
        }

        sortedPlayers.Sort((a, b) =>
        {
            int checkpointComparison = playerCheckpoints[b].CompareTo(playerCheckpoints[a]);
            if (checkpointComparison != 0) return checkpointComparison;

            return checkpointTimestamps[a].CompareTo(checkpointTimestamps[b]);
        });

        return sortedPlayers;
    }

    [ClientRpc]
    private void UpdatePositionsClientRpc()
    {
        // Clients will now fetch the data from GetPlayerPosition() in PlayerUI
    }

    public int GetPlayerPosition(ulong playerID)
    {
        List<ulong> sortedPlayers = GetPlayerPositions();

        // Update the Debug List for Inspector
        debugPlayerPositions.Clear();
        foreach (var id in sortedPlayers)
        {
            debugPlayerPositions.Add($"Player {id}: Position {sortedPlayers.IndexOf(id) + 1}");
        }

        if (!sortedPlayers.Contains(playerID))
        {
            Debug.LogWarning($"[RaceManager] Player {playerID} is not in the position list!");
            return -1; // Return an invalid position if not found
        }

        return sortedPlayers.IndexOf(playerID) + 1;
    }

    public void UpdatePlayerCheckpoint(ulong playerID, int checkpointIndex)
    {
        if (!playerCheckpoints.ContainsKey(playerID)) return;

        playerCheckpoints[playerID] = checkpointIndex;
        checkpointTimestamps[playerID] = Time.time;

        Debug.Log($"[RaceManager] Updated Player {playerID} -> Checkpoint {checkpointIndex}");
    }

    public int GetTotalPlayers()
    {
        return playerLaps.Count;
    }

    private void DeclareWinner(ulong winnerID)
    {
        Debug.Log($"🎉 Player {winnerID} Wins the Race!");
    }
}
