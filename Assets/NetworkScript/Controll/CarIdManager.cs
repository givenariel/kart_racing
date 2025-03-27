using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class CarIdManager : NetworkBehaviour
{
    public NetworkList<NetworkObjectReference> networkObjects = new NetworkList<NetworkObjectReference>();
    public List<Transform> players = new List<Transform>();

    public List<Transform> GetTransformList()
    {
        List<Transform> transformList = new List<Transform>();

        foreach (var netObjRef in networkObjects)
        {
            if (netObjRef.TryGet(out NetworkObject netObj))
            {
                transformList.Add(netObj.transform);
            }
        }

        return transformList;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
            Debug.Log("[Server] CarIdManager Spawned!");

        if (IsClient)
            Debug.Log("[Client] CarIdManager Terdeteksi!");
    }

    private void Start()
    {
        players = GetTransformList();

        foreach (Transform p in players)
        {
            p.GetComponent<PlayerInventory>().carIdManager = this;
        }
    }
}
