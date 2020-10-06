using System;
using UnityEngine;

public static class Vector2IntExtension
{
    /// <summary>
    /// Return new copy
    /// </summary>
    /// <param name="from"></param>
    /// <returns></returns>
    public static Vector2Int Copy(this Vector2Int from)
    {
        return new Vector2Int(from.x,from.y);
    }

    /// <summary>
    /// Increment X of this(!) vector by 1
    /// </summary>
    /// <param name="where"></param>
    public static Vector2Int IncrementX(this Vector2Int where)
    {
        where[0]++;
        return where;
    }
    
    /// <summary>
    /// Increment Y of this(!) vector by 1
    /// </summary>
    /// <param name="where"></param>
    public static Vector2Int IncrementY(this Vector2Int where)
    {
        where[1]++;
        return where;
    }
    
    
    /// <summary>
    /// Decrement X of this(!) vector by 1
    /// </summary>
    /// <param name="where"></param>
    public static Vector2Int DecrementX(this Vector2Int where)
    {
        where[0]--;
        return where;
    }
    
    /// <summary>
    /// Decrement Y of this(!) vector by 1
    /// </summary>
    /// <param name="where"></param>
    public static Vector2Int DecrementY(this Vector2Int where)
    {
        where[1]--;
        return where;
    }
    
   public static object DeserializeVector2Int(byte[] data)
   {
       Vector2Int result = new Vector2Int();

       result.x = BitConverter.ToInt32(data, 0);
       result.y = BitConverter.ToInt32(data, 4);

       return result;
   }

   public static byte[] SerializeVector2Int(object obj)
   {
       Vector2Int vector2Int = (Vector2Int) obj;
       byte[] result = new byte[8];
       
       BitConverter.GetBytes(vector2Int.x).CopyTo(result,0);
       BitConverter.GetBytes(vector2Int.y).CopyTo(result,4);

       return result;
   }
    
}
