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
        if (!IsServer) { return; }
        foreach (var client in ServerManager.instance.ClientData)
        {
            var cahracter = characterDatabase.GetCharacterById(client.Value.characterId);
            if (cahracter != null)
            {
                var spawnPos = new Vector3(UnityEngine.Random.Range(-3f, 3f), 0f, UnityEngine.Random.Range(-3f, 3f));
                var characterInstance = Instantiate(cahracter.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.clientId);
            }
        }

    }
}
