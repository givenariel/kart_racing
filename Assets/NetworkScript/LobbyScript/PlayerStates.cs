using System;
using Unity.Collections;
using Unity.Netcode;

public struct PlayerStates : INetworkSerializable, IEquatable<PlayerStates>
{
    public ulong ClientId;
    public FixedString64Bytes PlayerName; // Use FixedString64Bytes for strings in Netcode

    public PlayerStates(ulong clientId, string playerName)
    {
        ClientId = clientId;
        PlayerName = new FixedString64Bytes(playerName); // Convert string to FixedString64Bytes
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref PlayerName);
    }

    public bool Equals(PlayerStates other)
    {
        return ClientId == other.ClientId && PlayerName.Equals(other.PlayerName);
    }
}