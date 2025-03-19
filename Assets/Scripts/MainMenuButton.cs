using UnityEngine;

public class MainMenuButton : MonoBehaviour
{
    public GameObject MainMenuBtn;
    public GameObject HostUI;
    public GameObject ClientUI;
    public GameObject StartUI;
    public GameObject PlayerInputField;


    public void SetActiveStartUI()
    {
        StartUI.SetActive(true);
        MainMenuBtn.SetActive(false);
    }

    public void SetActiveFalseStartUI()
    {
        StartUI.SetActive(false);
        MainMenuBtn.SetActive(true);
    }

    public void SetActiveHostUI()
    {
        HostUI.SetActive(true);
        StartUI.SetActive(false);
        PlayerInputField.SetActive(true);
    }

    public void SetActiveFalseHostUI()
    {
        HostUI.SetActive(false);
        StartUI.SetActive(true);
        PlayerInputField.SetActive(false);
    }

    public void SetActiveClientUI()
    {
        ClientUI.SetActive(true);
        StartUI.SetActive(false);
        PlayerInputField.SetActive(true);
    }
    public void SetActiveFalseClientUI()
    {
        ClientUI.SetActive(false);
        StartUI.SetActive(true);
        PlayerInputField.SetActive(false);
    }
}
