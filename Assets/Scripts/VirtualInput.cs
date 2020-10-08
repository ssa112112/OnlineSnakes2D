using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Return Direction (type) from virtual button.
/// Can make visualization for input.
/// </summary>
public class VirtualInput : MonoBehaviour
{
    [SerializeField] Button left, right, up, down, deselect;
    static Direction savedDirection;
    static VirtualInput instance;

    void Awake()
    {
        savedDirection = Direction.Undefined;
        instance = this;
    }

    public static void LeftButtonPressed() => savedDirection = Direction.Left;
    public static void RightButtonPressed() => savedDirection = Direction.Right;
    public static void UpButtonPressed() => savedDirection = Direction.Up;
    public static void DownButtonPressed() => savedDirection = Direction.Down;

    /// <summary>
    /// Return the last pressed direction and cleans it at self
    /// </summary>
    /// <returns></returns>
    public static Direction GetDirection()
    {
        var result = savedDirection;
        savedDirection = Direction.Undefined;
        return result;
    }

    /// <summary>
    /// Visualization select on direction button.
    /// Unsafe if you don't have any instance of this class on scene. 
    /// </summary>
    /// <param name="direction"></param>
    public static void VisualizationInput(Direction direction)
    {
        switch (direction)
        {
            case Direction.Right:
                instance.right.Select();
                break;
            case Direction.Left:
                instance.left.Select();
                break;
            case Direction.Up:
                instance.up.Select();
                break;
            case Direction.Down:
                instance.down.Select();
                break;
            case Direction.Undefined:
                instance.deselect.Select();
                break;
        }
    }
}
