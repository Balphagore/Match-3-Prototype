using UnityEngine;
using System;
[Serializable]
public class Point
{
    public int x;
    public int y;
    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
    public void Multiple(int multiple)
    {
        x *= multiple;
        y *= multiple;
    }
    public void Add(Point point)
    {
        x += point.x;
        y += point.y;
    }
    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }
    public bool Equals(Point point)
    {
        return x == point.x && y == point.y;
    }
    public static Point FromVector(Vector2 vector2)
    {
        return new Point((int)vector2.x, (int)vector2.y);
    }
    public static Point FromVector(Vector3 vector3)
    {
        return new Point((int)vector3.x, (int)vector3.y);
    }
    public static Point Multiple(Point point, int multiple)
    {
        return new Point(point.x * multiple, point.y * multiple);
    }
    public static Point Add(Point currentPoint, Point addedPoint)
    {
        return new Point(currentPoint.x + addedPoint.x, currentPoint.y + addedPoint.y);
    }
    public static Point Clone(Point point)
    {
        return new Point(point.x, point.y);
    }
    public static Point Zero
    {
        get
        {
            return new Point(0, 0);
        }
    }
    public static Point One
    {
        get
        {
            return new Point(1, 1);
        }
    }
    public static Point Up
    {
        get
        {
            return new Point(0, 1);
        }
    }
    public static Point Down
    {
        get
        {
            return new Point(0, -1);
        }
    }
    public static Point Right
    {
        get
        {
            return new Point(1, 0);
        }
    }
    public static Point Left
    {
        get
        {
            return new Point(-1, 0);
        }
    }
}