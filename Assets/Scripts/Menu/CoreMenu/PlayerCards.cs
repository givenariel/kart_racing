using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerCards : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image characterIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text characterNameText;

    public void UpdateDisplay(CharacterSelectState state)
    {
        if (state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;
            characterNameText.text = character.DisplayName;
        }
        else
        {
            characterIconImage.enabled = false;
        }

        playerNameText.text = state.IsLockedIn? $"Player{state.ClientId}" : $"Player {state.ClientId} \n Picking...";

        visuals.SetActive(true);

    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }

}
