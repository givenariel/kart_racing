using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSelector : MonoBehaviour
{
    public void SelectMap(string mapSceneName)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log($"Host is selecting map: {mapSceneName}");
            ServerManager.instance.SetGameplaySceneName(mapSceneName);
            ServerManager.instance.StartGame();
        }
        else
        {
            Debug.LogWarning("Only the host can select the map.");
        }
    }
}
