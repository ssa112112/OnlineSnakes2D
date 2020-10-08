using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using Unity.Mathematics;
using Object = System.Object;

/// <summary>
/// Responsible for game loop.
/// Keeps snakes (links) and makes snakes moved; handling and sends (if IsMasterClient) events and more.
/// </summary>
public class GameplayManager : MonoBehaviourPunCallbacks, IOnEventCallback
{
    [SerializeField] Rating rating;
    [SerializeField] Text timeLeftText;
    [SerializeField] GameObject gameOverMessageBox;

    const int PointsForFruit = 5;
    const string TimePrefix = "TIME LEFT: ";
    const string GameWonText = "YOU ARE WON";
    const string GameLoseText = "YOU ARE LOSE";
    const string AllPlayersDisconnectSuffix = "(all players are disconnect)";
    const string GameEndTextFor1PlayerGames = "GAME IS END";

    /// <summary>
    /// Sorted (by snake.actorID) actuality list of snake scripts
    /// </summary>
    public static List<Snake> Snakes { get; private set; }

    public static Fruit Fruit{ get; private set;}
    
    double timeBetweenStep;
    double timeLastStep;
    float timeLeft;

    bool gameOver;

    void Start()
    {
        //Save Time
        timeLastStep = PhotonNetwork.Time;
        
        //Apply game speed
        byte gameSpeed = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.GameSpeed];
        timeBetweenStep = 0.9d - gameSpeed/10d; //0.9d just magic number (works normal for ping 100ms and less in max speed) 
        
        //Clear data from old game
        Snakes?.Clear();
        Fruit = null;
        
        //Set TimeLeft to start value 
        timeLeft = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.GameTimeInSeconds];
        timeLeftText.text = TimePrefix + math.round(timeLeft);
    }

    void Update()
    {
        //Don't update until the game has started or the game already end
        if (PhotonNetwork.CurrentRoom.IsOpen || gameOver)
            return;
        
        //Make global step, if this is the master client. 
        //Handling steps in any case (because we can become master) 
        if (PhotonNetwork.Time > timeLastStep + timeBetweenStep)
        {
            if (PhotonNetwork.IsMasterClient)
                MakeStepGlobal();
            timeLastStep = PhotonNetwork.Time;
        }
        
        //Reduce timeLeft
        timeLeft -= Time.deltaTime;
        timeLeftText.text = TimePrefix + math.round(timeLeft);
        
        //Check conditions of the game end
        if (timeLeft <= 0) 
            CompleteGame(timeIsEnd: true);
    }

    #region ObjectRegisterMethods

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

    /// <summary>
    /// Register the fruit
    /// </summary>
    /// <param name="fruit"></param>
    public static void RegisterFruit(Fruit fruit)
    {
        Fruit = fruit;
    }

    #endregion
    
    /// <summary>
    /// Reaction on left. Deactivate the rating write and the snake.
    /// </summary>
    /// <param name="otherPlayer"></param>
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (gameOver)
            return;

        var actorID = (int) otherPlayer.CustomProperties[PlayerOptionKeys.ActorID];
        for (var i = 0; i < Snakes.Count; i++)
        {
            if (Snakes[i].GetActorIDOfCreator() == actorID)
            {
                Snakes[i].Clear();
                Snakes[i].Disconnect = true;
                rating.PlayerLeft(actorID);
                if (Snakes.Count(s => !s.Disconnect) < 2)
                    CompleteGame(timeIsEnd: false);
                return;
            }
        }
    }

    #region HandlingInComingRemoteEvents

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
            Snakes[(int) photonEvent.CustomData - 1].Clear();
        }

        if (photonEvent.Code == RemoteEventNames.FruitCollected)
        {
            Object[] readData = (Object[]) photonEvent.CustomData;
            int actorID = (int) readData[0];
            Vector2Int newFruitPosition = (Vector2Int) readData[1];
            Fruit.ChangeGameFieldPosition(newFruitPosition);
            Snakes[actorID-1].AddNode();
            rating.AddScore(PointsForFruit,actorID);
        }
    }

    #endregion

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
        foreach (var snake in Snakes.Where(s => !s.Disconnect))
        {
            if (snake.GetCurrentLenght() == 0)
                snake.CreateRespawnEvent(); 
        }
    }

    /// <summary>
    /// Create a sync event with info about the fruit and its collector
    /// </summary>
    /// <param name="actorIDWhoCollected"></param>
    /// <param name="newFruitPosition"></param>
    public static void FruitCollected(int actorIDWhoCollected, Vector2Int newFruitPosition)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        var sendData = new Object[] {actorIDWhoCollected, newFruitPosition};
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        SendOptions sendOptions = new SendOptions {Reliability = true};
        PhotonNetwork.RaiseEvent(RemoteEventNames.FruitCollected, sendData, raiseEventOptions, sendOptions);
    }

    public override void OnLeftRoom()
    {
        gameObject.SetActive(false);
        SceneManager.LoadScene(0);
    }

    void CompleteGame(bool timeIsEnd)
    {
        //Stop the game
        gameOver = true;
        
        //Create message box
        var gameOverMessageBoxLink = Instantiate(gameOverMessageBox);
        var gameOverText = gameOverMessageBoxLink.GetComponentInChildren<Text>();
        
        //If player was 1
        if ((byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.PlayersInRoom] == 1)
        {
            gameOverText.text = GameEndTextFor1PlayerGames;
            return;
        }
        
        //If players were many 
        gameOverText.text = rating.AmIFist() ? GameWonText : GameLoseText;
        if (!timeIsEnd)
        {
            gameOverText.text += Environment.NewLine + AllPlayersDisconnectSuffix;
        }
    }

    #region OnEnable/OnDisable
    
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

    #endregion

}
