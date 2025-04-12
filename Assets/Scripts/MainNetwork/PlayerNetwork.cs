using UnityEngine;
using Unity.Cinemachine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private CinemachineCamera vc;
    [SerializeField] private Camera playerCam;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            vc.Priority = 1;
        }
        else
        {
            vc.Priority = 0;
            playerCam.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
