using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>(); // List of predefined spawn points
    [SerializeField] private  CarIdManager carIdManager;

    private List<Transform> availableSpawnPoints = new List<Transform>(); // Track unused spawn points

    public override void OnNetworkSpawn()
    {
        Debug.Log("[CharacterSpawner] OnNetworkSpawn Triggered");
        if (!IsServer) { return; }

        // Initialize available spawn points
        availableSpawnPoints = new List<Transform>(spawnPoints);

        foreach (var client in ServerManager.instance.ClientData)
        {
            Debug.Log($"[CharacterSpawner] Spawning character for ClientID: {client.Key}, CharacterID: {client.Value.characterId}");

            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                // Ensure there are available spawn points
                if (availableSpawnPoints.Count == 0)
                {
                    Debug.LogError("[CharacterSpawner] No available spawn points!");
                    return;
                }

                // Get the first available spawn point
                Transform spawnPoint = availableSpawnPoints[0];
                availableSpawnPoints.RemoveAt(0); // Remove the spawn point from the list

                // Instantiate character at spawn point position with its rotation
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPoint.position, spawnPoint.rotation);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
                carIdManager.networkObjects.Add(characterInstance);

                //PlayerInventory playerInventory = characterInstance.GetComponent<PlayerInventory>();
                 
                // playerInventory.carIdManagerRef.Value = new NetworkObjectReference(carIdManager.GetComponent<NetworkObject>());

                NetworkObject netObj = carIdManager.GetComponent<NetworkObject>();
                AssignCarIdManagerClientRpc(netObj);
                characterInstance.GetComponent<PlayerInventory>().SetCarIdManagerRefServerRpc(new NetworkObjectReference(netObj));
                characterInstance.GetComponent<CarHandler>().carID = client.Value.clientId;

                Debug.Log($"[CharacterSpawner] Spawned {character.GameplayPrefab.name} at {spawnPoint.position} with rotation {spawnPoint.rotation.eulerAngles}");
            }
            else
            {
                Debug.LogError($"[CharacterSpawner] Character data not found for CharacterID: {client.Value.characterId}");
            }
        }
    }

    [ClientRpc]
    public void AssignCarIdManagerClientRpc(NetworkObjectReference managerRef)
    {
        Debug.Log("[Client] Menerima referensi CarIdManager dari server...");

        if (managerRef.TryGet(out NetworkObject netObj))
        {
            carIdManager = netObj.GetComponent<CarIdManager>();
            Debug.Log($"[Client] carIdManager berhasil diatur lewat RPC: {carIdManager}");
        }
        else
        {
            Debug.LogError("[Client] Referensi CarIdManager valid tapi NetworkObject tidak ditemukan!");
        }
    }
}
