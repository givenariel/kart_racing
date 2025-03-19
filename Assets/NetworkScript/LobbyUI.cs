using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;
    

    private void Start()
    {
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + RoomData.RoomCode;
        }

        else
        {
            Debug.LogError("SceneDropdown is not assigned in the inspector.");
        }
    }
    public void OnStartButtonClicked()
    {
        if (ServerManager.instance != null)
        {
            ServerManager.instance.GoToCharacterSelect();
        }
        else
        {
            Debug.LogError("ServerManager instance not found!");
        }
    }

   /* public void OnGoToCharacterSelection()
    {
        ServerManager.instance.GoToCharacterSelect();
    }*/
}
