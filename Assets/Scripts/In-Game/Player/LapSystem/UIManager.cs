using UnityEngine;
using TMPro;
using Unity.Netcode;

public class UIManager : NetworkBehaviour
{
    //public static UIManager Instance;

    public TMP_Text lapText;
    public TMP_Text positionText;

    public NetworkVariable<NetworkObjectReference> LapManagerRef = new NetworkVariable<NetworkObjectReference>();

    public LapManager lapManager;

    
    
    [ServerRpc]
    public void SetLapManagerRefServerRpc(NetworkObjectReference managerRef)
    {
        LapManagerRef.Value = managerRef;
    }

    private void Awake()
    {
        
        
    }

    private void Update()
    {
       
        
    }

    // 🔹 Client hanya menerima data dari server
    [ClientRpc]
    public void UpdateUIClientRpc(ulong playerId, int lap, int position)
    {
        Debug.Log("iddd+ " + NetworkManager.Singleton.LocalClientId);
        //if (!IsOwner) { return; }
        if (playerId == NetworkManager.Singleton.LocalClientId)
        {
            lapText.text = $"Lap: {lap} / {LapManager.Instance.totalLaps}";
            positionText.text = $"Position: {position}";
        }
    }
}
