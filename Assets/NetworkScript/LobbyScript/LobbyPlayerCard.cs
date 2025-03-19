using UnityEngine;
using TMPro;
using Unity.Collections;

public class LobbyPlayerCard : MonoBehaviour
{
    [SerializeField] private GameObject visuals; // The card's visual elements
    [SerializeField] private TMP_Text playerNameText; // Text to display the player's name

    // Update the card with the player's name and whether it's the local player
    public void UpdateDisplay(FixedString64Bytes playerName, bool isLocalPlayer)
    {
        playerNameText.text = isLocalPlayer ? $"{playerName.ToString()} (YOU)" : playerName.ToString();
        visuals.SetActive(true); // Enable the card
    }

    // Disable the card when the player leaves
    public void DisableDisplay()
    {
        visuals.SetActive(false); // Disable the card
    }
}