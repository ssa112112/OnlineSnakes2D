using UnityEngine;

public class FieldSquare
{
    public FieldSquareState SquareState {get; private set;}
    readonly SpriteRenderer spriteRenderer;
    public int ActorID { get; private set; } //0 is "general actor"; 1,2,3,4 - players
    
    /// <summary>
    /// Create empty FieldSquare
    /// </summary>
    /// <param name="spriteRenderer"></param>
    public FieldSquare(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;
        SquareState = FieldSquareState.Empty;
        ActorID = 0;
    }

    /// <summary>
    /// Change state. Used force mode.
    /// </summary>
    /// <param name="newState"></param>
    /// <param name="actorID">0 is "general actor"; 1,2,3,4 - players</param>
    public void ChangeState(FieldSquareState newState,int actorID)
    {
        SquareState = newState;
        ActorID = actorID;
        
        switch (newState)
        {
            case FieldSquareState.Empty:
                spriteRenderer.color = Color.white;
                break;
            case FieldSquareState.Fruit:
                spriteRenderer.color = Color.red;
                break;
            case FieldSquareState.BodyOfSnake:
            case FieldSquareState.HeadOfSnake:
                spriteRenderer.color = ColorByActorID.Get(actorID);
                break;
            default:
                spriteRenderer.color = Color.black;
                break;
        }
    }
}
