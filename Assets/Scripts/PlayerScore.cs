

// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class PlayerScore : MonoBehaviour
// {
//     public ulong playerId;    // Thêm playerId vào lớp PlayerScore
//     public string playerName;
//     public int score;

//     public PlayerScore(ulong playerId, string playerName, int score)
//     {
//         this.playerId = playerId;
//         this.playerName = playerName;
//         this.score = score;
//     }

// }
using Unity.Netcode;
using Unity.Collections;

public struct PlayerScore : INetworkSerializable
{
    public ulong playerId;
    public FixedString64Bytes playerName;
    public int score;

    public PlayerScore(ulong playerId, string playerName, int score)
    {
        this.playerId = playerId;

        this.playerName = new FixedString64Bytes(playerName);
        this.score = score;
    }

    // Implement Network Serialization
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerId);
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref score);
    }
}

