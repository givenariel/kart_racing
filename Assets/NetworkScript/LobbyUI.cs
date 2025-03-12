using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text roomCodeText;
    [SerializeField] private TMP_Dropdown sceneDropdown;

    private void Start()
    {
        if (roomCodeText != null)
        {
            roomCodeText.text = "Room Code: " + RoomData.RoomCode;
        }

        if (sceneDropdown != null)
        {
            sceneDropdown.onValueChanged.AddListener(OnSceneSelected);
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

    private void OnSceneSelected(int index)
    {
        // Retrieve the selected scene name from the dropdown's option text.
        string selectedScene = sceneDropdown.options[index].text;
        Debug.Log("Selected gameplay scene: " + selectedScene);

        // Update the gameplay scene name in the ServerManager.
        if (ServerManager.instance != null)
        {
            ServerManager.instance.SetGameplaySceneName(selectedScene);
        }
        else
        {
            Debug.LogError("ServerManager instance not found!");
        }
    }
}
