using Unity.Netcode;
using UnityEngine;
using TMPro;

public class PositionManager : NetworkBehaviour
{
    /*private ulong playerID;
    private int currentPosition = 1;
    [SerializeField]private RaceManager raceManager;*/

    [SerializeField] private GameObject uiCanvas; // Assign UI Canvas holding the position text
    //[SerializeField] private TextMeshProUGUI positionText;

    /*void Awake()
    {
        playerID = NetworkObject.OwnerClientId;
        raceManager = RaceManager.Instance;
    }*/

    void Start()
    {
        if (!IsOwner)
        {
            if (uiCanvas != null)
            {
                Destroy(uiCanvas); // Destroy UI for non-owners in Start()
            }
            return;
        }

        //InvokeRepeating(nameof(UpdatePosition), 1f, 1f); // Update position every second
    }

    /*private void UpdatePosition()
    {
        if (raceManager == null) return;

        RequestPositionUpdateServerRpc(playerID);
    }

    [ServerRpc]
    private void RequestPositionUpdateServerRpc(ulong requestingPlayerID)
    {
        int position = raceManager.GetPlayerPosition(requestingPlayerID);
        UpdatePositionClientRpc(position);
    }

    [ClientRpc]
    private void UpdatePositionClientRpc(int newPosition)
    {
        if (!IsOwner) return; // Ensure only the owner updates their UI

        currentPosition = newPosition;
        if (positionText != null)
        {
            positionText.text = $"Position: {currentPosition}/{raceManager.GetTotalPlayers()}";
        }
    }*/
}
