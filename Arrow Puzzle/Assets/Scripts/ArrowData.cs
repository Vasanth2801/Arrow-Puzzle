using UnityEngine;

public class ArrowData : MonoBehaviour
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