using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchLevelStateTask : Task
{
    private readonly LevelState Start;
    private readonly LevelState End;

    public SwitchLevelStateTask(LevelState start,LevelState end)
    {
        Start = start;
        End = end;
    }

    protected override void Init()
    {
        base.Init();
        GameManager.levelState = End;
        SetState(TaskState.Success);
    }
}
