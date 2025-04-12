using UnityEngine;
using Unity.Netcode;

public class ThrowableSlowTrap : NetworkBehaviour
{
    public float slowDuration = 3f; // Durasi efek slow
    public float slowMultiplier = 0.5f; // Seberapa lambat pemain jadi
    public GameObject slowTrapPrefab; // Prefab jebakan setelah terkena tanah
    public bool hasSpawnedTrap = false; // Cegah jebakan spawn lebih dari sekali
    public Transform ownerTransform;
    [SerializeField] LayerMask groundLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || hasSpawnedTrap) return;

        Debug.Log("Trigger dengan: " + other.name);

        if (other.CompareTag("Player") && other.transform != ownerTransform)
        {
            var targetNetObj = other.GetComponent<NetworkObject>();
            if (targetNetObj != null)
            {
                hasSpawnedTrap = true;
                Debug.Log("Mengenai player, memberikan efek slow!");
                ApplySlowEffectServerRpc(targetNetObj.NetworkObjectId);
            }
        }
        else if (other.CompareTag("Ground"))
        {
            hasSpawnedTrap = true;
            Debug.Log("Mengenai ground, mengubah menjadi jebakan!");

            RaycastHit hit;
            Vector3 contactPoint = transform.position;
            Quaternion targetRotation = Quaternion.identity;

            if (Physics.Raycast(transform.position, -transform.up, out hit, 10f, groundLayer) ||
                Physics.Raycast(transform.position, transform.up, out hit, 10f, groundLayer))
            {
                contactPoint = hit.point;
                targetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }

            SpawnGroundTrapServerRpc(contactPoint, targetRotation);
        }
    }

    [ServerRpc]
    private void ApplySlowEffectServerRpc(ulong targetId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetId, out var targetNetObj))
        {
            CarHandler playerKart = targetNetObj.GetComponent<CarHandler>();
            if (playerKart != null)
            {
                playerKart.ApplySlowEffect(slowMultiplier, slowDuration);
                Debug.Log("Efek slow diterapkan ke pemain!");
            }
        }

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    [ServerRpc]
    private void SpawnGroundTrapServerRpc(Vector3 position, Quaternion rotation)
    {
        if (slowTrapPrefab == null)
        {
            Debug.LogError("slowTrapPrefab belum diassign di Inspector!");
            return;
        }

        GameObject trap = Instantiate(slowTrapPrefab, position, rotation);
        NetworkObject trapNetObj = trap.GetComponent<NetworkObject>();
        if (trapNetObj != null)
        {
            trapNetObj.Spawn();
            ThrowableSlowTrap trapScript = trap.GetComponent<ThrowableSlowTrap>();
            if (trapScript != null)
            {
                trapScript.ownerTransform = ownerTransform;
                trapScript.ActivateTrap();
                Debug.Log("Jebakan berhasil dibuat dan dishare ke semua client.");
            }
        }
        else
        {
            Debug.LogError("Prefab jebakan tidak memiliki NetworkObject!");
        }

        if (NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    public void ActivateTrap()
    {
        Debug.Log("Jebakan aktif!");
        gameObject.tag = "Trap";
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!IsServer) return;

        if (CompareTag("Trap") && other.CompareTag("Player") && other.transform != ownerTransform)
        {
            CarHandler playerKart = other.GetComponent<CarHandler>();
            if (playerKart != null)
            {
                Debug.Log("Pemain menginjak jebakan, memberikan efek slow!");
                playerKart.ApplySlowEffect(slowMultiplier, slowDuration);
            }
        }
    }
}
