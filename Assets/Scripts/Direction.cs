
public enum Direction
{
    Up,
    Down,
    Left,
    Right,
    Undefined
}

public static class DirectionSerialize
{
    public static object DeserializeDirection(byte[] data)
    {
        return (Direction) data[0];
    }

    public static byte[] SerializeDirection(object obj)
    {
        Direction direction = (Direction) obj;
        byte[] result = new byte[1];
        result[0] = (byte) direction;
        return result;
    }
}

