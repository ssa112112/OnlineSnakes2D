using UnityEngine;
using Photon.Pun;

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
    /// Force rewrite added position
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="fieldPosition"></param>
    public void ChangeSquareOfField(FieldSquareState newState, Vector2Int fieldPosition)
    {
        field[fieldPosition.x, fieldPosition.y].ChangeState(newState);
    }

    #endregion
}
