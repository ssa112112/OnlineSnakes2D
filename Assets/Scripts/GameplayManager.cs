using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using Photon.Realtime;
using Object = System.Object;

/// <summary>
/// Keeps snakes (links) and makes snakes moved
/// </summary>
public class GameplayManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    /// <summary>
    /// Sorted (by snake.actorID) actuality list of snake scripts
    /// </summary>
    public static List<Snake> Snakes { get; private set; }
    
    double timeBetweenStep;
    double timeLastStep;
    
    void Start()
    {
        //Save Time
        timeLastStep = PhotonNetwork.Time;
        //Apply game speed
        byte gameSpeed = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.GameSpeed];
        timeBetweenStep = 0.9d - gameSpeed/10d; //0.9d just magic number (works normal for ping 100ms and less in max speed) 
        //Clear list of snakes, if need
        Snakes?.Clear();
    }

    void Update()
    {
        if (PhotonNetwork.Time > timeLastStep + timeBetweenStep && PhotonNetwork.CurrentRoom.IsOpen == false)
        {
            if (PhotonNetwork.IsMasterClient)
                MakeStepGlobal();
            timeLastStep = PhotonNetwork.Time;
        }
    }

    public void OnEvent(EventData photonEvent)
    {
        if (photonEvent.Code == RemoteEventNames.RegularStep)
        { 
            Direction[] directions = (Direction[]) photonEvent.CustomData;
            for (var i = 0; i< Snakes.Count;i++)
                Snakes[i].MakeStepLocal(directions[i]);
        }

        if (photonEvent.Code == RemoteEventNames.SnakeDeadAndRespawn)
        { 
            Debug.Log("Apply respawn1");
            Object[] readData = (Object[]) photonEvent.CustomData;
            int actorID =(int) readData[0];
            Vector2Int spawnPoint = (Vector2Int) readData[1];
            Direction spawnDirection = (Direction) readData[2];
            (Vector2Int, Direction) spawnPosition;
            spawnPosition.Item1 = spawnPoint;
            spawnPosition.Item2 = spawnDirection;
            Snakes[actorID-1].Respawn(spawnPosition);
        }

        if (photonEvent.Code == RemoteEventNames.SnakeDead)
        {
            Debug.Log("Apply respawn2");
            Snakes[(int) photonEvent.CustomData - 1].Clear();
        }
    }
    
    /// <summary>
    /// Send everyone the command for make a step (authoritarian)
    /// </summary>
    void MakeStepGlobal()
    {
        var directions = Snakes.Select(s => s.GetCurrentInputDirection()).ToArray();
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        SendOptions sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(RemoteEventNames.RegularStep, directions, raiseEventOptions, sendOptions);

        //Try respawn for deactivate snakes
        foreach (var snake in Snakes)
        {
            if (snake.GetCurrentLenght() == 0)
                snake.CreateRespawnEvent(); 
        }
    }

    /// <summary>
    /// Register the snake in the Snakes list
    /// </summary>
    /// <param name="snake"></param>
    /// <returns></returns>
    public static bool RegisterSnakeScript(Snake snake)
    {
        if (Snakes == null)
            Snakes = new List<Snake>(4);
        
        if (snake == null || Snakes.Contains(snake))
            return false;
        
        Snakes.Add(snake);

        Snakes = Snakes.OrderBy(s => s.GetActorIDOfCreator()).ToList();
        
        return true;
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        //Unregister snake
        var actorID = otherPlayer.GetPlayerNumber();
        for (var i = 0; i < Snakes.Count; i++)
        {
            if (Snakes[i].GetActorIDOfCreator() == actorID)
            {
                Snakes.RemoveAt(i);
                return;
            }
        }
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene(0);
    }

    public override void OnEnable()
    {
        PhotonNetwork.AddCallbackTarget(this);
        base.OnEnable();
    }

    public override void OnDisable()
    {
        PhotonNetwork.RemoveCallbackTarget(this);
        base.OnDisable();
    }
}
