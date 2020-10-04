using UnityEngine;
using Photon.Pun;

public class GameField : MonoBehaviour
{
    [SerializeField] LineFactory lineFactory;
    
    float screenLeft;
    float screenRight;
    float screenTop;
    float screenBottom;

    [SerializeField] float drawingStep = 1f;
    [SerializeField] float drawingWidth = 0.04f;
    [SerializeField] Color drawingColor = Color.green;

    void Awake()
    {
        //Change camera size for draw more tiles
        var mapSize = (byte) PhotonNetwork.CurrentRoom.CustomProperties[RoomOptionKeys.MapSize];
        Camera.main.orthographicSize += mapSize;
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
            lineFactory.GetLine(startPoint, endPoint, drawingWidth, drawingColor);
            startPoint.y -= drawingStep;
            endPoint.y -= drawingStep;
        }
        
        //Drawing vertical lines
        startPoint.x = screenLeft;
        startPoint.y = screenTop;
        endPoint.x = screenLeft;
        endPoint.y = screenBottom;

        var countHorizontalLine = lineFactory.GetActive().Count;
        for (var i = 0;i < countHorizontalLine;i++)
        {
            lineFactory.GetLine(startPoint, endPoint, drawingWidth, drawingColor);
            startPoint.x += drawingStep;
            endPoint.x += drawingStep;
        }
    }
    
    void SaveScreenPositionsInWorldPoints()
    {
        // save screen edges in world coordinates
        float screenZ = -Camera.main.transform.position.z;
        Vector3 lowerLeftCornerScreen = new Vector3(0, 0, screenZ);
        Vector3 upperRightCornerScreen = new Vector3(
            Screen.width, Screen.height, screenZ);
        Vector3 lowerLeftCornerWorld =
            Camera.main.ScreenToWorldPoint(lowerLeftCornerScreen);
        Vector3 upperRightCornerWorld =
            Camera.main.ScreenToWorldPoint(upperRightCornerScreen);
        screenLeft = lowerLeftCornerWorld.x;
        screenRight = upperRightCornerWorld.x;
        screenTop = upperRightCornerWorld.y;
        screenBottom = lowerLeftCornerWorld.y;
    }
    
}
