using UnityEngine;

[System.Serializable]
public class ArrowData
{
    public Vector2Int gridPosition;

    public ArrowDirection direction;
}

public enum ArrowDirection
{
    Up,
    Down,
    Left,
    Right
}