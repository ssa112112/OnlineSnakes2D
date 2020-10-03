/// <summary>
/// Use like string key for custom room properties
/// Returns a short cut for efficient network transmission 
/// </summary>
public static class RoomOptionKeys
{
    public static string PlayersInRoom => "P";
    public static string MapSize => "M";
    public static string GameSpeed => "S";
    public static string GameTimeInSeconds => "T";
}