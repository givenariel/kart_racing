using UnityEngine;
using Unity.Netcode;

public class Trap : NetworkBehaviour
{
    [SerializeField] private float disableDuration = 0.5f;
    public GameObject stunVFX;
    public Transform ownerTransform;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return; // Only the server handles trap activation

        if (other.CompareTag("Player") && other.transform != ownerTransform)
        {
            CarHandler playerKart = other.GetComponent<CarHandler>();
            Shield kartShield = other.GetComponent<Shield>();
            ulong targetClientId = other.GetComponent<NetworkObject>().OwnerClientId;

            if (kartShield != null && kartShield.IsShieldActive)
                return;

            if (playerKart != null)
            {
                // Call the RPC to apply stun on the client
                ApplyStunClientRpc(targetClientId, disableDuration);

                // Optionally apply stun on the server side for prediction
                playerKart.Stun(disableDuration, "Trap");
            }

            Destroy(gameObject); // Destroy the trap on all clients
        }
    }

    [ClientRpc]
    private void ApplyStunClientRpc(ulong targetClientId, float duration)
    {
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        CarHandler playerKart = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<CarHandler>();
        if (playerKart != null)
        {
            playerKart.Stun(duration, "Trap");

            if (stunVFX != null)
            {
                Vector3 spawnPosition = playerKart.transform.position + Vector3.up * 1.5f;
                GameObject effect = Instantiate(stunVFX, spawnPosition, Quaternion.identity);
                effect.transform.SetParent(playerKart.transform);
                Destroy(effect, duration);
            }
        }
    }
}
