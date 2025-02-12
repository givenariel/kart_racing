using UnityEngine;
using System;


[Serializable]
public class ClientData
{
    public ulong clientId;
    public int characterId = -1;
    public string playerName;
    public ClientData(ulong clientId, string playerName = "Unknown")
    {
        this.clientId = clientId;
        this.playerName = playerName;
    }
}