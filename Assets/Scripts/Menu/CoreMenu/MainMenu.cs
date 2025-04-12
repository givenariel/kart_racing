using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MainMenu : MonoBehaviour
{
    [SerializeField]
    Button m_StartHostButton;
    [SerializeField]
    Button m_StartClientButton;
    [SerializeField]
    Button m_StartServerButton;

    
    public void StartServer()
    {
        //ServerManager.instance.StartServer();
        //DeactivateButtons();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        //DeactivateButtons();
    }

    public void StartHost()
    {
        //NetworkManager.Singleton.StartHost();
        ServerManager.instance.StartHost();
        //DeactivateButtons();
    }

    /*void DeactivateButtons()
    {
        m_StartHostButton.interactable = false;
        m_StartClientButton.interactable = false;
        m_StartServerButton.interactable = false;
    }*/
}

