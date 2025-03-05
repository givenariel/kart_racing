using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject disableOverlay;
    [SerializeField] private Button button;


    private CharacterDisplay characterSelect;
    public Character character { get; private set; }

    public bool IsDisable { get; private set; }

    public void SetCharacter(CharacterDisplay characterSelect, Character character)
    {
        iconImage.sprite = character.Icon;

        this.characterSelect = characterSelect;

        this.character = character;
    }

    public void SelectCharacter ()
    {
        characterSelect.Select(character);
    }

    public void SetDisable()
    {
        IsDisable = true;
        disableOverlay.SetActive(true);
        button.interactable = false;
    }
}
