/// <summary>
/// Names for remote events
/// Return eventCode
/// </summary>
public static class RemoteEventNames
{
    /// <summary>
    /// Send sorted Direction[] of all snakes
    /// </summary>
    public static byte RegularStep => 69;
    
    /// <summary>
    /// Send actorID whose shake dead.
    /// Calls only if respawn snake is impossible.  
    /// </summary>
    public static byte SnakeDead => 70;
    
    /// <summary>
    /// Send this: new Object[] {int actorIDWhoCollected,Vector2Int newFruitPosition}
    /// </summary>
    public static byte FruitCollected => 71;
    
    /// <summary>
    /// Send this: new Object[] {int actorID,Vector2Int newPositionHeadOfSnake, Direction directionTailOfSnake}
    /// </summary>
    public static byte SnakeDeadAndRespawn => 72;
    
}
