using Unity.Netcode;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointID; // Unique ID for each checkpoint
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            LapManager lapManager = other.GetComponent<LapManager>();
            if (lapManager != null)
            {
                ulong playerID = other.GetComponent<NetworkObject>().OwnerClientId; // Get player ID
                lapManager.OnCheckpointPassed(checkpointID, playerID);
            }
        }
    }
}
