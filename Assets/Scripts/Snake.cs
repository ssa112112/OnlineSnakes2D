using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Object = System.Object;

/// <summary>
/// A snake
/// </summary>
public class Snake : MonoBehaviourPunCallbacks, IPunObservable, IOnEventCallback
{
    GameField gameField;

    Direction inputDirection = Direction.Undefined; //pressed
    Direction currentDirection = Direction.Undefined; //pressed and applied 

    [SerializeField] int actorID; //Creator of this object

    const int StartLenght = 3;

    LinkedList<Vector2Int> nodeCoordinates = new LinkedList<Vector2Int>(); //Coordinates of nodes. Head always first.

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

    void Start()
    {
        gameField = GameObject.FindWithTag("GameField").GetComponent<GameField>();

        //Getting actorID
        actorID = (int) photonView.InstantiationData[0];

        //Registration 
        GameplayManager.RegisterSnakeScript(this);

        //Spawn
        Spawn(gameField.GetStartPositionForSnakeByID(actorID, StartLenght));
    }


    /// <summary>
    /// Restore snake for default position
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
            if (startPosition.Item2 == Direction.Left)
                pointer = pointer.DecrementX();
            else
                pointer = pointer.IncrementX();
            gameField.ChangeSquareOfField(pointer, FieldSquareState.BodyOfSnake, actorID);
            nodeCoordinates.AddLast(new LinkedListNode<Vector2Int>(pointer));
        }
    }

    public void Respawn((Vector2Int, Direction) startPosition)
    {
        Clear();
        
        Spawn(startPosition);
    }

    void Update()
    {
        if (photonView.IsMine)
            HandlingInput();
    }

    void HandlingInput()
    {
        KeyboardInput();
    }

    void KeyboardInput()
    {
        if (Input.GetAxis("Horizontal") > 0 && currentDirection != Direction.Left)
            inputDirection = Direction.Right;
        else if (Input.GetAxis("Horizontal") < 0 && currentDirection != Direction.Right)
            inputDirection = Direction.Left;

        if (Input.GetAxis("Vertical") < 0 && currentDirection != Direction.Down)
            inputDirection = Direction.Up;
        else if (Input.GetAxis("Vertical") > 0 && currentDirection != Direction.Up)
            inputDirection = Direction.Down;
    }

    public void MakeStepLocal(Direction to)
    {
        if (nodeCoordinates.Count == 0) return;
        
        currentDirection = to;

        Vector2Int newGameFieldPosition = nodeCoordinates.First.Value;
        switch (currentDirection)
        {
            case Direction.Down:
                newGameFieldPosition = newGameFieldPosition.DecrementY();
                break;
            case Direction.Up:
                newGameFieldPosition = newGameFieldPosition.IncrementY();
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

        //Now it's second square, means it's body, not head
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
        //Clear tail
        gameField.ChangeSquareOfField(nodeCoordinates.Last.Value, FieldSquareState.Empty, actorID);
        nodeCoordinates.RemoveLast();
    }

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
            Debug.Log("CreateRespawnEvent1");
        }
        else
        {
            PhotonNetwork.RaiseEvent(RemoteEventNames.SnakeDead, actorID, raiseEventOptions, sendOptions);
            Debug.Log("CreateRespawnEvent2");
        }
    }
    
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
}
