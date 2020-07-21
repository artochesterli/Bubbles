using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Left,
    Right,
    Up,
    Down,
    Null
}

public class MoveInfo
{
    public Direction direction;
    public Vector2Int CurrentPos;
    public Vector2Int TargetPos;
    public Vector3 CurrentLocation;
    public Vector3 TargetLocation;

    public MoveInfo(Direction dir, Vector2Int Pos, Vector3 Location)
    {
        direction = dir;
        CurrentPos = Pos;
        CurrentLocation = Location;
        switch (direction)
        {
            case Direction.Left:
                TargetPos = CurrentPos + Vector2Int.left;
                TargetLocation = CurrentLocation + Vector3.left;
                break;
            case Direction.Right:
                TargetPos = CurrentPos + Vector2Int.right;
                TargetLocation = CurrentLocation + Vector3.right;
                break;
            case Direction.Up:
                TargetPos = CurrentPos + Vector2Int.up;
                TargetLocation = CurrentLocation + Vector3.up;
                break;
            case Direction.Down:
                TargetPos = CurrentPos + Vector2Int.down;
                TargetLocation = CurrentLocation + Vector3.down;
                break;
        }
    }
}
