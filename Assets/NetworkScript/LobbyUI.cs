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
    }
}
