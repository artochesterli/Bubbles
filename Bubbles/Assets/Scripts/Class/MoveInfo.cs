using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    Left,
    Right,
    Up,
    Down
}

public class MoveInfo
{
    public Direction direction;
    public Vector2Int CurrentPos;
    public Vector2Int TargetPos;

    public MoveInfo(Direction dir, Vector2Int Pos)
    {
        direction = dir;
        CurrentPos = Pos;
        switch (direction)
        {
            case Direction.Left:
                TargetPos = CurrentPos + Vector2Int.left;
                break;
            case Direction.Right:
                TargetPos = CurrentPos + Vector2Int.right;
                break;
            case Direction.Up:
                TargetPos = CurrentPos + Vector2Int.up;
                break;
            case Direction.Down:
                TargetPos = CurrentPos + Vector2Int.down;
                break;
        }
    }
}
