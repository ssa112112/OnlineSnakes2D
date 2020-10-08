using UnityEngine;

//TODO: different color for head and body and, may be, rating  
public static class ColorByActorID
{
    /// <summary>
    /// Return unique color by ID
    /// </summary>
    /// <param name="ID"></param>
    /// <returns></returns>
    public static Color Get(int ID)
    {        
        switch (ID)
        {
            case 1:
                return Color.magenta;
            case 2:
                return Color.cyan;
            case 3:
                return Color.yellow;
            case 4:
                return Color.blue;
            default:
                return Color.black;
        }
    }
}
