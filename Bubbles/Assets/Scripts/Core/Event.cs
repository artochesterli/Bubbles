using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Event { }

public class Place : Event
{
    public GameObject UseableBubble;
    public Vector2Int Pos;
    public BubbleType Type;

    public Place(GameObject bubble, Vector2Int v, BubbleType type)
    {
        UseableBubble = bubble;
        Pos = v;
        Type = type;
    }
}

public class MotionFinish : Event { }


public class LevelLoaded : Event
{
    public int Index;
    public LevelLoaded(int num)
    {
        Index = num;
    }
}

public class BubbleSelected : Event
{
    public BubbleType Type;
    public BubbleSelected (BubbleType type)
    {
        Type = type;
    }
}

public class Back : Event
{
    public BubbleType Type;
    public Back(BubbleType type)
    {
        Type = type;
    }
}


public class CallLoadLevel: Event
{
    public LoadLevelType Type;
    public int index;
    public GameObject SelectedLevelButton;

    public CallLoadLevel(LoadLevelType type, int num, GameObject levelbutton = null)
    {
        Type = type;
        index = num;
        SelectedLevelButton = levelbutton;
    }
}

public class FinishLoadLevel : Event
{
    public int index;
    public FinishLoadLevel(int i)
    {
        index = i;
    }
}

public class CallGoToHelp : Event { }
public class CallGoToSelectLevel : Event { }

public class CallBackToSelectLevel : Event { }

public class CallBackToMainMenu : Event { }

public class RollBack : Event { }


