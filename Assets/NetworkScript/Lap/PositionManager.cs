using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PositionManager : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI positionText; // Assign in Inspector
    [SerializeField] private GameObject uiCanvas; // Assign the UI Canvas in Inspector

    private void Start()
    {
        // Ensure the UI is only displayed for the local player
        if (IsLocalPlayer) // Ensures the UI is active only for the local player
        {
            if (uiCanvas != null)
            {
                uiCanvas.SetActive(true);
            }
        }


        // Initialize the UI for the local player (client or host)
        if (uiCanvas != null)
        {
            uiCanvas.SetActive(true); // Ensure the UI is active for the local player
        }

        if (positionText != null)
        {
            positionText.text = "Position: -"; // Default text before the first update
        }

        // Start updating the position every second
        InvokeRepeating(nameof(UpdatePosition), 1f, 1f);
    }

    private void UpdatePosition()
    {
        if (RaceManager.Instance == null)
        {
            Debug.LogWarning("RaceManager instance is not available.");
            return;
        }

        Debug.Log($"Requesting position update for Player {NetworkObject.OwnerClientId}");
        RequestPositionUpdateServerRpc(NetworkObject.OwnerClientId);
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestPositionUpdateServerRpc(ulong requestingPlayerID)
    {
        if (RaceManager.Instance == null) return;

        int position = RaceManager.Instance.GetPlayerPosition(requestingPlayerID);

        Debug.Log($"[Server] Player {requestingPlayerID} is in Position {position}");

        UpdatePositionClientRpc(requestingPlayerID, position);
    }



    [ClientRpc]
    private void UpdatePositionClientRpc(ulong playerID, int newPosition)
    {
        Debug.Log($"[ClientRpc] Updating UI for Player {playerID}, Position: {newPosition}, Local Player: {NetworkManager.Singleton.LocalClientId}");

        if (playerID != NetworkManager.Singleton.LocalClientId) return;

        if (positionText != null)
        {
            positionText.text = ""; // Force UI refresh
            positionText.text = $"Position: {newPosition}/{RaceManager.Instance.GetTotalPlayers()}";
        }
    }



}