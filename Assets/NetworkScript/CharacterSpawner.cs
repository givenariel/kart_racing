using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;

    public override void OnNetworkSpawn()
    {
        Debug.Log("[CharacterSpawner] OnNetworkSpawn Triggered");
        if (!IsServer) { return; }

        foreach (var client in ServerManager.instance.ClientData)
        {
            Debug.Log($"[CharacterSpawner] Spawning character for ClientID: {client.Key}, CharacterID: {client.Value.characterId}");

            var character = characterDatabase.GetCharacterById(client.Value.characterId);
            if (character != null)
            {
                var spawnPos = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f));
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);

                Debug.Log($"[CharacterSpawner] Spawned {character.GameplayPrefab.name} at {spawnPos}");
            }
            else
            {
                Debug.LogError($"[CharacterSpawner] Character data not found for CharacterID: {client.Value.characterId}");
            }
            }
        }
    }
