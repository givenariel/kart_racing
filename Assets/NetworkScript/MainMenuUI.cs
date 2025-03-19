using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField playerNameInput;
    [SerializeField] private TMP_InputField roomCodeInput;

    public void SetPlayerName()
    {
        if (playerNameInput != null && ServerManager.instance != null)
        {
            ServerManager.instance.SetPlayerName(playerNameInput.text);
        }
    }

    public void StartHost()
    {
        if (ServerManager.instance != null)
        {
            SetPlayerName();
            ServerManager.instance.StartHost();
        }
    }

    public void StartClient()
    {
        if (roomCodeInput != null && ServerManager.instance != null && playerNameInput != null)
        {
            SetPlayerName();
            ServerManager.instance.StartClient(roomCodeInput.text, playerNameInput.text);
        }
    }
}