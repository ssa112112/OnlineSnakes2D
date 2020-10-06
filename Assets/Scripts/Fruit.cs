using UnityEngine;

public class Fruit : MonoBehaviour
{
    GameField gameField;

    const int ActorID = 0;

    public Vector2Int GameFieldPosition { get; private set; }

    void Start()
    {
        gameField = GameObject.FindWithTag("GameField").GetComponent<GameField>();

        //Set default position 
        gameField.ChangeSquareOfField(gameField.Center, FieldSquareState.Fruit, ActorID);
        
        //Registration
        GameplayManager.RegisterFruit(this);
    }

    public void ChangeGameFieldPosition(Vector2Int newPosition)
    {
        //Erase old position
        if (gameField.CheckSquareState(GameFieldPosition, FieldSquareState.Fruit))
            gameField.ChangeSquareOfField(GameFieldPosition, FieldSquareState.Empty, ActorID);
        
        //Draw new position
        gameField.ChangeSquareOfField(newPosition, FieldSquareState.Fruit, ActorID);
        
        //Save new position
        GameFieldPosition = newPosition;
    }
}
