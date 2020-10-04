using UnityEngine;

public class FieldSquare
{
    public FieldSquareState SquareState {get; private set;}
    readonly SpriteRenderer spriteRenderer;
    
    public FieldSquare(SpriteRenderer spriteRenderer)
    {
        this.spriteRenderer = spriteRenderer;
        SquareState = FieldSquareState.Empty;
    }

    /// <summary>
    /// Change state. Used force mode.
    /// </summary>
    /// <param name="newState"></param>
    public void ChangeState(FieldSquareState newState)
    {
        SquareState = newState;
        switch (newState)
        {
            case FieldSquareState.Empty:
                spriteRenderer.color = Color.white;
                break;
            case FieldSquareState.Fruit:
                spriteRenderer.color = Color.red;
                break;
            case FieldSquareState.BodyOfSnake:
                spriteRenderer.color = Color.magenta;
                break;
            case FieldSquareState.HeadOfSnake:
                spriteRenderer.color = Color.magenta;
                break;
            default:
                spriteRenderer.color = Color.black;
                break;
        }
    }

    /// <summary>
    /// Restore default state. Return last state.
    /// </summary>
    /// <returns></returns>
    public FieldSquareState RestoreState()
    {
        var stateBeforeRestore = SquareState;
        ChangeState(FieldSquareState.Empty);
        return stateBeforeRestore;
    }
}
