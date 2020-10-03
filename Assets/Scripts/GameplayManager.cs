using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class GameplayManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        var roomOptions = PhotonNetwork.CurrentRoom.CustomProperties;
        Debug.Log("Getting:");
        Debug.Log(roomOptions[RoomOptionKeys.PlayersInRoom]);
        Debug.Log(roomOptions[RoomOptionKeys.GameSpeed]);
        Debug.Log(roomOptions[RoomOptionKeys.MapSize]);
        Debug.Log(roomOptions[RoomOptionKeys.GameTimeInSeconds]);
        Debug.Log(PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }
}
