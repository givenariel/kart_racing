using UnityEngine;
using Unity.Cinemachine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{

    [SerializeField] private CinemachineCamera vc;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            vc.Priority = 1;
        }
        else
        {
            vc.Priority = 0;
        }
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
