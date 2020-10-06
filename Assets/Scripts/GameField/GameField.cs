using UnityEngine;
using Photon.Pun;
using Random = UnityEngine.Random;

/// <summary>
/// Game field. These are pretty squares that can change color.
/// (0,0) - left top corner.
/// </summary>
public class GameField : MonoBehaviour
{
    FieldSquare[,] field; //State of field
    [SerializeField] GameObject squareOfField; //Visual display

    [SerializeField] LineFactory lineFactory;
    [SerializeField] Camera cameraMain;
    
    //Screen points in world coordinates
    float screenLeft;
    //float screenRight;
    float screenTop;
    float screenBottom;

    //Setting of draw line
    float drawingLineStep = 1f;
    float drawingLineWidth = 0.03f;
    [SerializeField] Color drawingLineColor = Color.green;

    #region Inisitialize
    void Awake()
    {
        //Change camera size for draw more tiles
        byte mapSize = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.MapSize];
        cameraMain.orthographicSize += mapSize;
    }

    void Start()
    {
        SaveScreenPositionsInWorldPoints();
        
        DrawField();
    }

    /// <summary>
    /// Draw field. Need saved screen position.
    /// </summary>
    void DrawField()
    {
        var heightInWorldPoint = Mathf.Abs(screenBottom)+Mathf.Abs(screenTop);
        
        //Drawing horizontal lines
        var startPoint = new Vector2(screenLeft, screenTop);
        var endPoint = new Vector2(screenLeft + heightInWorldPoint, screenTop);

        while (startPoint.y >= screenBottom)
        {
            lineFactory.GetLine(startPoint, endPoint, drawingLineWidth, drawingLineColor);
            startPoint.y -= drawingLineStep;
            endPoint.y -= drawingLineStep;
        }
        
        //Drawing vertical lines and create field[,]
        startPoint.x = screenLeft;
        startPoint.y = screenTop;
        endPoint.x = screenLeft;
        endPoint.y = screenBottom;

        var countHorizontalLine = lineFactory.GetActive().Count;
        field = new FieldSquare[countHorizontalLine-1,countHorizontalLine-1];
        
        for (var i = 0;i < countHorizontalLine;i++)
        {
            lineFactory.GetLine(startPoint, endPoint, drawingLineWidth, drawingLineColor);
            startPoint.x += drawingLineStep;
            endPoint.x += drawingLineStep;
            
            if (i == countHorizontalLine - 1) 
                break; //Last line. Doesn't need spawn squares
            
            //Create square of field
            //1)Down half of drawingStep
            var spawnPositionForSquare = new Vector2(startPoint.x-0.5f*drawingLineStep,startPoint.y-0.5f*drawingLineStep);
            //2) And spawn
            for (var j = 0; j < countHorizontalLine - 1; j++)
            {
                var square = Instantiate(squareOfField, spawnPositionForSquare, Quaternion.identity,this.transform);
                field[i,j] = new FieldSquare(square.GetComponent<SpriteRenderer>());
                
                spawnPositionForSquare.y -= drawingLineStep;
            }
        }
    }
    
    void SaveScreenPositionsInWorldPoints()
    {
        // save screen edges in world coordinates
        float screenZ = -cameraMain.transform.position.z;
        Vector3 lowerLeftCornerScreen = new Vector3(0, 0, screenZ);
        Vector3 upperRightCornerScreen = new Vector3(
            Screen.width, Screen.height, screenZ);
        Vector3 lowerLeftCornerWorld =
            cameraMain.ScreenToWorldPoint(lowerLeftCornerScreen);
        Vector3 upperRightCornerWorld =
            cameraMain.ScreenToWorldPoint(upperRightCornerScreen);
        screenLeft = lowerLeftCornerWorld.x;
        //screenRight = upperRightCornerWorld.x;
        screenTop = upperRightCornerWorld.y;
        screenBottom = lowerLeftCornerWorld.y;
    }
    #endregion

    #region WorkWithField

    /// <summary>
    /// Rewrite added position. Resolve collision.
    /// </summary>
    /// <param name="fieldPosition"></param>
    /// <param name="newState"></param>
    /// <param name="actorID"></param>
    public bool ChangeSquareOfField(Vector2Int fieldPosition, FieldSquareState newState, int actorID)
    {
        //Check borders 
        if (fieldPosition.x < 0 || fieldPosition.x > field.GetUpperBound(0) ||
            fieldPosition.y < 0 || fieldPosition.y > field.GetUpperBound(0))
            return false;
        
        if (newState == FieldSquareState.HeadOfSnake)
            //Resolve collision
            //It's a very simple method. It isn't perfect, but enough good.
            //Possible non-critical inaccuracies (неточности) are extremely rare.
            switch (field[fieldPosition.x,fieldPosition.y].SquareState)
            {
                case FieldSquareState.Fruit:
                    //TODO
                    break;
                case FieldSquareState.BodyOfSnake:
                    return false;
                case FieldSquareState.HeadOfSnake:
                    var otherActorID = field[fieldPosition.x, fieldPosition.y].ActorID;
                    GameplayManager.Snakes[otherActorID-1].CreateRespawnEvent();
                    return false;
            }

        field[fieldPosition.x, fieldPosition.y].ChangeState(newState, actorID);

        return true;
    }

    /// <summary>
    /// Return these position:
    /// 1 - right, bottom
    /// 2 - left, top
    /// 3 - right, top
    /// 4 - left, bottom
    /// |2| | |3|
    /// | | | | |
    /// | | | | |
    /// |4| | |1|
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="startLenght"></param>
    /// <returns>Vector2Int for the Snake and Direction for draw tail</returns>
    public (Vector2Int , Direction) GetStartPositionForSnakeByID(int actorID, int startLenght)
    {
        var resultDirection = Direction.Undefined;
        var resultPoint = new Vector2Int();
        
        if (actorID == 1 || actorID == 3)
            resultDirection = Direction.Right;
        else
            resultDirection = Direction.Left;
        
        switch (actorID)
        {
            case 1:
                resultPoint.x = field.GetUpperBound(0) - (startLenght-1);
                resultPoint.y = field.GetUpperBound(0);
                break;
            case 2:
                resultPoint.x = 0 + (startLenght-1);
                resultPoint.y = 0;
                break;
            case 3:
                resultPoint.x = field.GetUpperBound(0)  - (startLenght-1);
                resultPoint.y = 0;
                break;
            case 4:
                resultPoint.x = 0 + (startLenght-1);
                resultPoint.y = field.GetUpperBound(0) ;
                break;
        }
        
        return (resultPoint, resultDirection);
    }

    /// <summary>
    /// Get new respawn position for snake by id. Can return null.
    /// </summary>
    /// <param name="actorID"></param>
    /// <param name="startLenght"></param>
    /// <returns></returns>
    public (Vector2Int, Direction)? GetRespawnPositionForSnakeByID(int actorID, int startLenght)
    {
        //GetStartPoint
        (Vector2Int, Direction) respawnPosition = GetStartPositionForSnakeByID(actorID, startLenght);
        
        //Add some Randomizing
        var randomNumber = Random.Range(0, field.GetUpperBound(0));
        switch (actorID)
        {
            case 1:
            case 4:
                respawnPosition.Item1.y -= randomNumber;
                break;
            case 2:
            case 3:
                respawnPosition.Item1.y += randomNumber;
                break;
        }
        
        if (CheckPositionForRespawn(respawnPosition, startLenght))
            return respawnPosition;
        
        //Random is unsuccessful. Start search!
        respawnPosition = GetStartPositionForSnakeByID(actorID, startLenght);
        int dirMod = respawnPosition.Item1.y == 0 ? 1 : -1;//DirectionModificator
        for (var i = 0;i < field.GetUpperBound(0);i++)
        {
            respawnPosition.Item1.y += dirMod;
            if (CheckPositionForRespawn(respawnPosition, startLenght))
                return respawnPosition;
        }

        //Position isn't exist
        return null;
    }

    /// <summary>
    /// Check future snake position and its entourage (окружение) 
    /// </summary>
    /// <param name="positionAndDirection"></param>
    /// <param name="startLenght"></param>
    /// <returns></returns>
    bool CheckPositionForRespawn((Vector2Int, Direction) positionAndDirection, int startLenght)
    {
        Vector2Int pointer = positionAndDirection.Item1;
        int dirMod = positionAndDirection.Item2 == Direction.Left ? -1 : 1; //DirectionModificator

        for (var i = -1; i < startLenght; i++) //Go by column
        {
            //Check row above
            if (pointer.y != 0) //if row - topmost don't check
                if (!CheckSquareState(new Vector2Int(pointer.x + i*dirMod, pointer.y - 1), 
                    FieldSquareState.Empty, FieldSquareState.Fruit)) return false;
            //Check row with future shake 
            if (!CheckSquareState(new Vector2Int(pointer.x + i*dirMod, pointer.y), 
                FieldSquareState.Empty)) return false;
            //Check row bellow
            if (pointer.y != field.GetUpperBound(0)) //if row - lowest (нижняя) don't check
                if (!CheckSquareState(new Vector2Int(pointer.x + i*dirMod, pointer.y + 1), 
                    FieldSquareState.Empty, FieldSquareState.Fruit)) return false;
        }
        //Position is very good
        return true;
    }

    /// <summary>
    /// Return true if in fieldPosition located stateForCompare, else false
    /// </summary>
    /// <param name="fieldPosition"></param>
    /// <param name="stateForCompare"></param>
    /// <returns></returns>
    public bool CheckSquareState(Vector2Int fieldPosition, FieldSquareState stateForCompare)
    {
        return field[fieldPosition.x, fieldPosition.y].SquareState == stateForCompare;
    }
    
    /// <summary>
    /// Return true if in fieldPosition located stateForCompare or stateForCompare2, else false
    /// </summary>
    /// <param name="fieldPosition"></param>
    /// <param name="stateForCompare"></param>
    /// <param name="stateForCompare2"></param>
    /// <returns></returns>
    public bool CheckSquareState(Vector2Int fieldPosition, FieldSquareState stateForCompare, FieldSquareState stateForCompare2)
    {
        return field[fieldPosition.x, fieldPosition.y].SquareState == stateForCompare ||
               field[fieldPosition.x, fieldPosition.y].SquareState == stateForCompare2;
    }

    /// <summary>
    /// Clear all square with given ID
    /// </summary>
    /// <param name="actorID"></param>
    public void ClearByID(int actorID)
    {
        foreach (var square in field)
        {
           if (square.ActorID == actorID)
               square.ChangeState(FieldSquareState.Empty,0);
        }
    }

    #endregion
    
}
