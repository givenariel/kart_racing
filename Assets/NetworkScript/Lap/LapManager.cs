using UnityEngine;
using Unity.Netcode;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class LapManager : NetworkBehaviour
{
    private Dictionary<ulong, int> lastCheckpointPerPlayer = new Dictionary<ulong, int>();
    private Dictionary<ulong, int> lapPerPlayer = new Dictionary<ulong, int>();
    [SerializeField] private int totalCheckpoints;
    private bool hasFinished = false;

    [SerializeField] private TextMeshProUGUI lapText; // Assign in Inspector
    [SerializeField] private GameObject winUI; // Assign in Inspector
    [SerializeField] private GameObject loseUI;

    void Start()
    {
        StartCoroutine(InitializeCheckpoints());
        totalCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).Length;
        Debug.Log($"🔍 Found {totalCheckpoints} checkpoints in the scene.");
        RaceManager.Instance.RegisterPlayer(OwnerClientId);
        UpdateLapUI();
    }

    private IEnumerator InitializeCheckpoints()
    {
        yield return new WaitForSeconds(0.5f); // Delay to ensure all objects are loaded
        totalCheckpoints = FindObjectsByType<Checkpoint>(FindObjectsSortMode.None).Length;

        if (totalCheckpoints == 0)
        {
            Debug.LogError("🚨 No checkpoints found! Ensure checkpoints exist in the scene.");
            yield break;
        }

        RaceManager.Instance.RegisterPlayer(OwnerClientId);
        UpdateLapUI();
    }

    private void Update()
    {
        UpdateLapUI();
    }

    public void OnCheckpointPassed(int checkpointID, ulong playerID)
    {
        if (hasFinished) return;

        if (!lastCheckpointPerPlayer.ContainsKey(playerID))
        {
            lastCheckpointPerPlayer[playerID] = -1;
            lapPerPlayer[playerID] = 0;
        }

        int lastCheckpoint = lastCheckpointPerPlayer[playerID];
        Debug.Log($"🚗 Player {playerID} passed checkpoint {checkpointID}. LastCheckpoint: {lastCheckpoint}");

     
        if (checkpointID == (lastCheckpoint + 1) % totalCheckpoints)
        {
            lastCheckpointPerPlayer[playerID] = checkpointID;
        }

       
        if (checkpointID == 0 && lastCheckpoint == totalCheckpoints - 1)
        {
            lapPerPlayer[playerID]++;
            lastCheckpointPerPlayer[playerID] = 0; 

            Debug.Log($"🏁 Player {playerID} completed a lap! New lap count: {lapPerPlayer[playerID]}");

            if (IsOwner)
            {
                PlayerCompletedLapServerRpc(playerID, lapPerPlayer[playerID]);
            }

            UpdateLapUI();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerCompletedLapServerRpc(ulong playerID, int lap)
    {
        if (hasFinished) return;

        if (lap >= RaceManager.Instance.totalLapsToWin)
        {
            Debug.Log($"🎉 Player {playerID} won the race!");
            hasFinished = true;

            // Send winner info to all players
            ShowWinLoseUIClientRpc(playerID);
        }

        RaceManager.Instance.PlayerCompletedLapServerRpc(playerID, lap);
    }

    [ClientRpc]
    private void ShowWinLoseUIClientRpc(ulong winnerID)
    {
        Debug.Log($"📢 ShowWinLoseUIClientRpc CALLED! Winner: {winnerID}");
        if (!IsOwner) return;

        ulong localPlayerID = NetworkManager.Singleton.LocalClientId;
        Debug.Log($"🖥️ Local Player {localPlayerID}: Checking Win/Lose UI...");

        if (localPlayerID == winnerID)
        {
            if (winUI) winUI.SetActive(true);
            Debug.Log("🏆 WIN UI ACTIVATED!");
        }
        else
        {
            Debug.Log("❌ Player LOST. Running ShowLoseUIWithDelay()");
            StartCoroutine(ShowLoseUIWithDelay());
        }
    }

    private IEnumerator ShowLoseUIWithDelay()
    {
        Debug.Log("⌛ Coroutine STARTED: Waiting 1s...");
        yield return new WaitForSeconds(1f);

        if (loseUI)
        {
            loseUI.SetActive(true);
            Debug.Log("💀 LOSE UI ACTIVATED!");
        }
        else
        {
            Debug.LogError("❌ loseUI is NULL!");
        }
    }
    private void UpdateLapUI()
    {
        if (lapText != null)
        {
            if (!lapPerPlayer.ContainsKey(OwnerClientId))
            {
                lapPerPlayer[OwnerClientId] = 0; // Ensure player exists in dictionary
            }
            lapText.text = $"Lap: {lapPerPlayer[OwnerClientId]}/{RaceManager.Instance.totalLapsToWin}";
        }
    }

}
