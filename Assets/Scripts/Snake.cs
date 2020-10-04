using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;


/// <summary>
/// A snake
/// </summary>
public class Snake : MonoBehaviourPunCallbacks
{
    GameField gameField;
    
    Direction inputDirection = Direction.Undefined; //pressed
    Direction syncDirection = Direction.Undefined; //pressed and sync
    Direction currentDirection = Direction.Undefined; //pressed, sync and applied 

    float timeBetweenStep;
    float timeFromLastStep;

    int startLenght = 3;

    LinkedList<Vector2Int> nodeCoordinates = new LinkedList<Vector2Int>(); //Coordinates of nodes. Head always first.
    void Start()
    {
        gameField = GameObject.FindWithTag("GameField").GetComponent<GameField>();

        //Apply game speed
        byte gameSpeed = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.GameSpeed];
        timeBetweenStep = 0.945f - gameSpeed/10f; //0.945f just magic number
        
        //Spawn head
        Vector2Int pointer = new Vector2Int(2,3); //coordinates of head spawn
        gameField.ChangeSquareOfField(FieldSquareState.HeadOfSnake,pointer);
        nodeCoordinates.AddFirst(new LinkedListNode<Vector2Int>(pointer));
        
        //Spawn nodes
        for (int  i = 0;  i < startLenght-1;  i++) //-1 because head is already spawn 
        {
            pointer = pointer.DecrementX();
            gameField.ChangeSquareOfField(FieldSquareState.BodyOfSnake, pointer);
            nodeCoordinates.AddLast(new LinkedListNode<Vector2Int>(pointer));
        }
    }

    void Update()
    {
        HandlingInput();
        
        if (timeFromLastStep >= timeBetweenStep)
        {
            MakeStep();
            timeFromLastStep = 0f;
        }
        else
            timeFromLastStep += Time.deltaTime;
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

    void MakeStep()
    {
        currentDirection = inputDirection;

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
        
        //now it's second square, means it's body, not head
        gameField.ChangeSquareOfField(FieldSquareState.BodyOfSnake, nodeCoordinates.First.Value);
        //Write new head
        nodeCoordinates.AddFirst(new LinkedListNode<Vector2Int>(newGameFieldPosition));
        //Display it
        gameField.ChangeSquareOfField(FieldSquareState.HeadOfSnake,nodeCoordinates.First.Value);
        //Clear tail (хвост)
        gameField.ChangeSquareOfField(FieldSquareState.Empty,nodeCoordinates.Last.Value);
        nodeCoordinates.RemoveLast();
    }
        
}
