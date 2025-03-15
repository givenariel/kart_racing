using UnityEngine;
using TMPro;
using Unity.Netcode;
using System.Collections;

public class PlayerUI : MonoBehaviour
{
    public TextMeshProUGUI positionText;

    private bool isReady = false;

    private void Start()
    {
        StartCoroutine(WaitForRaceManager());
    }

    private IEnumerator WaitForRaceManager()
    {
        while (RaceManager.Instance == null || !RaceManager.Instance.IsSpawned)
        {
            yield return null;
        }
        isReady = true;
    }

    private void Update()
    {
        if (!isReady || positionText == null) return;

        ulong localPlayerID = NetworkManager.Singleton.LocalClientId;
        int position = RaceManager.Instance.GetPlayerPosition(localPlayerID);
        int totalPlayers = RaceManager.Instance.GetTotalPlayers();

        if (totalPlayers > 1)
        {
            positionText.text = position.ToString() + "/" + totalPlayers.ToString();
        }
    }
}
