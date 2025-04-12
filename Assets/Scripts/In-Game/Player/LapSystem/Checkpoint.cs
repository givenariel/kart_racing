using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class Checkpoint : NetworkBehaviour
{
    public int checkpointID; // Unique ID for each checkpoint
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out NetworkObject networkObject))
        {
            ulong playerId = networkObject.OwnerClientId;

                LapManager.Instance.PlayerCrossedCheckpointServerRpc(playerId, checkpointID);
                Debug.Log($"Client: Player {playerId} crossed checkpoint {checkpointID}");
            
        }
    }
}