using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using System.Linq;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;
    public int totalLapsToWin = 3;

    private Dictionary<ulong, int> playerLaps = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> playerCheckpoints = new Dictionary<ulong, int>();
    private Dictionary<ulong, float> checkpointTimestamps = new Dictionary<ulong, float>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            playerLaps.Clear();
            playerCheckpoints.Clear();
            checkpointTimestamps.Clear();
        }
    }

    public void RegisterPlayer(ulong playerID)
    {
        if (!playerLaps.ContainsKey(playerID))
        {
            playerLaps[playerID] = 0;
            playerCheckpoints[playerID] = -1;
            checkpointTimestamps[playerID] = Time.time;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateCheckpointServerRpc(ulong playerID, int checkpointID)
    {
        if (!playerCheckpoints.ContainsKey(playerID)) return;

        playerCheckpoints[playerID] = checkpointID;
        checkpointTimestamps[playerID] = Time.time;

        UpdatePositionsClientRpc();
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
        return playerLaps.Keys
            .OrderByDescending(player => playerLaps[player])  // Higher lap count first
            .ThenByDescending(player => playerCheckpoints[player]) // Higher checkpoint ID
            .ThenBy(player => checkpointTimestamps[player]) // Earlier timestamp
            .ToList();
    }

    [ClientRpc]
    private void UpdatePositionsClientRpc()
    {
        // Clients will now fetch the data from GetPlayerPosition() in PlayerUI
    }

    public int GetPlayerPosition(ulong playerID)
    {
        List<ulong> sortedPlayers = GetPlayerPositions();
        return sortedPlayers.IndexOf(playerID) + 1;
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
