using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Object = System.Object;

//TODO: add bot mode

/// <summary>
/// A snake
/// </summary>
public class Snake : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    GameField gameField; //Link

    Direction inputDirection = Direction.Undefined; //pressed
    Direction currentDirection = Direction.Undefined; //pressed and applied 
    Vector2 touchStarted; //Support handling touch and mouse input 
    Camera mainCamera; //Support handling touch and mouse input 

    [SerializeField] int actorID; //Creator of this object

    const int StartLenght = 3; //Supported value from 2 to 7 (not tested)

    LinkedList<Vector2Int> nodeCoordinates = new LinkedList<Vector2Int>(); //Coordinates of nodes. Head always first.

    bool grow; //If this snake eat fruit, this bool become true ( addNode() ) and its grow;

    public bool Disconnect { get; set; }

    void Start()
    {
        gameField = GameObject.FindWithTag("GameField").GetComponent<GameField>();
        mainCamera = Camera.main;

        //Getting actorID
        actorID = (int) photonView.InstantiationData[0];

        //Registration 
        GameplayManager.RegisterSnakeScript(this);

        //Spawn
        Spawn(gameField.GetStartPositionForSnakeByID(actorID, StartLenght));
    }

    #region Geters

    public int GetActorIDOfCreator()
    {
        return actorID;
    }

    public Direction GetCurrentInputDirection()
    {
        return inputDirection;
    }

    public int GetCurrentLenght()
    {
        return nodeCoordinates.Count;
    }

    #endregion

    #region MethodsForChangeSnake

    /// <summary>
    /// Restore snake for uninitialized position
    /// </summary>
    public void Clear()
    {
        inputDirection = Direction.Undefined;
        currentDirection = Direction.Undefined;
        gameField.ClearByID(actorID);
        nodeCoordinates.Clear();
    }

    void Spawn((Vector2Int, Direction) startPosition)
    {
        //Spawn head
        Vector2Int pointer = startPosition.Item1; //coordinates of head spawn
        gameField.ChangeSquareOfField(pointer, FieldSquareState.HeadOfSnake, actorID);
        nodeCoordinates.AddFirst(new LinkedListNode<Vector2Int>(pointer));

        //Spawn nodes
        for (int i = 0; i < StartLenght - 1; i++) //-1 because head is already spawn 
        {
            pointer = startPosition.Item2 == Direction.Left ? pointer.DecrementX() : pointer.IncrementX();
            gameField.ChangeSquareOfField(pointer, FieldSquareState.BodyOfSnake, actorID);
            nodeCoordinates.AddLast(new LinkedListNode<Vector2Int>(pointer));
        }

        //Set start direction 
        currentDirection = startPosition.Item2 == Direction.Left ? Direction.Right : Direction.Left;
    }

    public void Respawn((Vector2Int, Direction) position)
    {
        Clear();
        Spawn(position);
    }

    public void MakeStepLocal(Direction to)
    {
        if (nodeCoordinates.Count == 0) return;
        if (to == Direction.Undefined) return;

        currentDirection = to;

        Vector2Int newGameFieldPosition = nodeCoordinates.First.Value;
        switch (currentDirection)
        {
            case Direction.Down:
                newGameFieldPosition = newGameFieldPosition.IncrementY();
                break;
            case Direction.Up:
                newGameFieldPosition = newGameFieldPosition.DecrementY();
                break;
            case Direction.Left:
                newGameFieldPosition = newGameFieldPosition.DecrementX();
                break;
            case Direction.Right:
                newGameFieldPosition = newGameFieldPosition.IncrementX();
                break;
            case Direction.Undefined:
                return;
        }

        //Now it's the second square, means it's body, not head
        gameField.ChangeSquareOfField(nodeCoordinates.First.Value, FieldSquareState.BodyOfSnake, actorID);
        //Write new head
        nodeCoordinates.AddFirst(new LinkedListNode<Vector2Int>(newGameFieldPosition));
        //Display it
        var successfulStep =
            gameField.ChangeSquareOfField(nodeCoordinates.First.Value, FieldSquareState.HeadOfSnake, actorID);
        if (!successfulStep)
        {
            Clear();
            CreateRespawnEvent();
            return;
        }

        //Clear tail, if grow is false, else growing
        if (grow)
        {
            grow = false;
            return;
        }

        gameField.ChangeSquareOfField(nodeCoordinates.Last.Value, FieldSquareState.Empty, actorID);
        nodeCoordinates.RemoveLast();
    }

    public void AddNode()
    {
        grow = true;
    }

    #endregion

    #region HanglingInput

    void Update()
    {
        if (photonView.IsMine)
            HandlingInput();
    }

    void HandlingInput()
    {
        Direction newInputDirection;

        newInputDirection = TouchInput();
        if (newInputDirection == Direction.Undefined)
            newInputDirection = KeyboardInput();


        switch (newInputDirection)
        {
            case Direction.Right when currentDirection != Direction.Left:
                inputDirection = Direction.Right;
                break;
            case Direction.Left when currentDirection != Direction.Right:
                inputDirection = Direction.Left;
                break;
            case Direction.Up when currentDirection != Direction.Down:
                inputDirection = Direction.Up;
                break;
            case Direction.Down when currentDirection != Direction.Up:
                inputDirection = Direction.Down;
                break;
        }
    }

    Direction KeyboardInput()
    {
        if (Input.GetAxis("Horizontal") > 0)
            return Direction.Right;
        if (Input.GetAxis("Horizontal") < 0)
            return Direction.Left;
        if (Input.GetAxis("Vertical") > 0)
            return Direction.Up;
        if (Input.GetAxis("Vertical") < 0)
            return Direction.Down;

        return Direction.Undefined;
    }

    Direction TouchInput()
    {
        if (Input.GetMouseButtonDown(0))
            touchStarted = mainCamera.ScreenToViewportPoint(Input.mousePosition);
        else if (Input.GetMouseButtonUp(0))
        {
            Vector2 touchEnded = mainCamera.ScreenToViewportPoint(Input.mousePosition);
            Vector2 swipe = touchEnded - touchStarted;

            if (swipe.magnitude > 0.1) //0.1 - minimal swipe size at % from screen
            {
                //Horizontal swipe? 
                if (Mathf.Abs(swipe.x) > Mathf.Abs(swipe.y))
                    return swipe.x > 0 ? Direction.Right : Direction.Left;
                //else
                    return swipe.y > 0 ? Direction.Up : Direction.Down;
            }
        }

        return Direction.Undefined;
    }

#endregion

    #region NetwotkingMethods
    
    public void CreateRespawnEvent()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        
        var respawnPosition = gameField.GetRespawnPositionForSnakeByID(actorID, StartLenght);
        RaiseEventOptions raiseEventOptions = new RaiseEventOptions {Receivers = ReceiverGroup.All};
        SendOptions sendOptions = new SendOptions {Reliability = true};

        if (respawnPosition.HasValue)
        {
            var sendData = new Object[] {actorID, respawnPosition.Value.Item1, respawnPosition.Value.Item2};
            PhotonNetwork.RaiseEvent(RemoteEventNames.SnakeDeadAndRespawn, sendData, raiseEventOptions, sendOptions);
        }
        else
        {
            PhotonNetwork.RaiseEvent(RemoteEventNames.SnakeDead, actorID, raiseEventOptions, sendOptions);
        }
    }
    
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(inputDirection);
        }
        else
        {
            inputDirection = (Direction) stream.ReceiveNext();
        }
    }

    public void OnEvent(EventData photonEvent) { }  //Only created Event
    
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
