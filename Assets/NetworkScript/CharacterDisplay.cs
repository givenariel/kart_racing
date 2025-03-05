using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using System.Linq;
using UnityEngine.UI;
using NUnit.Framework;
using System.Collections.Generic;

public class CharacterDisplay : NetworkBehaviour
{
    private NetworkList<CharacterSelectState> players;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton characterSelectButton;
    [SerializeField] private PlayerCards[] PlayerCards;
    [SerializeField] private Transform introSpawnPoint;
    [SerializeField] private Button LockInButton;

    private GameObject introInstance;
    private List<CharacterSelectButton> characterButtons = new List<CharacterSelectButton>();
    

    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            Character[] allCharacters = characterDatabase.GetAllCharacter(); 

            foreach (var character in allCharacters)
            {
                var selecButtonInstance = Instantiate(characterSelectButton, charactersHolder);
                selecButtonInstance.SetCharacter(this, character);
                characterButtons.Add(selecButtonInstance);
            }

            players.OnListChanged += HandlePlayerStateChange;

        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnected;

            foreach (NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayerStateChange;
        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        Debug.Log($"Client Connected: {clientId}");
        bool alreadyExists = false;
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                alreadyExists = true;
                break; 
            }
        }
        if (!alreadyExists)
        {
            players.Add(new CharacterSelectState(clientId));
            Debug.Log($"Added Player: {clientId}. Player Count: {players.Count}");
        }
        else
        {
            Debug.Log($"Duplicate Player Connection Attempt: {clientId}");
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    public void Select(Character character)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }
            if (players[i].IsLockedIn) { return; }
            if (players[i].CharacterId == character.Id) { return; }
            if (IsCharacterTaken(character.Id, false)) { return; }
        }

        characterNameText.text = character.DisplayName;
        characterInfoPanel.SetActive(true);
        if (introInstance != null)
        {
            Destroy(introInstance);
        }
        introInstance = Instantiate(character.IntroPrefabs, introSpawnPoint);

        SelectServerRpc(character.Id);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams severRpcParam = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != severRpcParam.Receive.SenderClientId) { continue;}
            if (!characterDatabase.IsvalidCharacterId(characterId)) { return; }
            if (IsCharacterTaken(characterId, true)) { return; }
            players[i] = new CharacterSelectState(
                    players[i].ClientId, characterId,
                    players[i].IsLockedIn);
            
        }
    }

    public void LockIn()
    {
        LockInServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams severRpcParam = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != severRpcParam.Receive.SenderClientId) { continue; }
            if (!characterDatabase.IsvalidCharacterId(players[i].CharacterId)) { return; }
            if (IsCharacterTaken(players[i].CharacterId, true)) { return; }
                    players[i] = new CharacterSelectState(
                        players[i].ClientId,
                        players[i].CharacterId,
                        true);

        }

        foreach (var player in players)
        {
            if (!player.IsLockedIn) { return; }
        }

        foreach (var player in players)
        {
            ServerManager.instance.SetCharacter(player.ClientId, player.CharacterId);
        }

        ServerManager.instance.StartGame();
    }

    private void HandlePlayerStateChange(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        for (int i = 0; i < PlayerCards.Length; i++)
        {
            if (players.Count > i)
            {
                PlayerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                PlayerCards[i].DisableDisplay();
            }
        }

        foreach (var button in characterButtons)
        {
            if (button.IsDisable) { continue; }
            if (IsCharacterTaken(button.character.Id, false))
            {
                button.SetDisable();
            }
        }

        foreach (var player in players)
        {
            if (player.ClientId != NetworkManager.Singleton.LocalClientId) { continue; }
            if (player.IsLockedIn)
            {
                LockInButton.interactable = false;
                break;
            }
            if (IsCharacterTaken(player.CharacterId, false))
            {
                LockInButton.interactable = true;
                break;
            }
            LockInButton.interactable = true;
            break;
        }
    }


    private bool IsCharacterTaken(int characterId, bool checkAll)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (!checkAll)
            {
                if (players[i].ClientId == NetworkManager.Singleton.LocalClientId) { continue; }
            }
            if (players[i].IsLockedIn && players[i].CharacterId == characterId)
            {
                return true;
            }
        }
        return false;
    }

}
