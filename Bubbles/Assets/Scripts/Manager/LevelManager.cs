using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int LevelIndex;
    public GameObject AllSlot;
    public GameObject AllBubble;
    public float MotionInterval;


    private List<List<SlotInfo>> Map;
    private Vector2 PivotOffset;
    private BubbleType HeldBubbleType;

    private SerialTasks BubbleMotionTasks = new SerialTasks();

    void Start()
    {
        Map = new List<List<SlotInfo>>();
        GetMapInfo();
        //ShowMapInfo();
        HeldBubbleType = BubbleType.Disappear;
        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
    }

    void Update()
    {
        BubbleMotionTasks.Update();
    }


    private void ShowMapInfo()
    {
        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                Debug.Log(Map[i][j].InsideBubbleType);
                Debug.Log(Map[i][j].InsideBubbleState);
            }
        }
    }

    private void GetMapInfo()
    {
        float MinX=int.MaxValue, MinY=int.MaxValue, MaxX=int.MinValue, MaxY=int.MinValue;
        
        foreach(Transform child in AllSlot.transform)
        {
            if (child.localPosition.x < MinX)
            {
                MinX = child.localPosition.x;
            }

            if (child.localPosition.y < MinY)
            {
                MinY = child.localPosition.y;
            }

            if (child.localPosition.x > MaxX)
            {
                MaxX = child.localPosition.x;
            }

            if (child.localPosition.y > MaxY)
            {
                MaxY = child.localPosition.y;
            }
        }

        PivotOffset = new Vector2(MinX, MinY);
        Vector2 Size = new Vector2(Mathf.RoundToInt(MaxX - MinX) + 1, Mathf.RoundToInt(MaxY - MinY) + 1);

        for(int i = 0; i < Size.x; i++)
        {
            Map.Add(new List<SlotInfo>());
            for(int j = 0; j < Size.y; j++)
            {
                Map[i].Add(null);
            }
        }

        foreach (Transform child in AllSlot.transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x - MinX);
            int y = Mathf.RoundToInt(child.localPosition.y - MinY);
            Map[x][y] = new SlotInfo(new Vector2Int(x,y), BubbleType.Null, BubbleState.Default, null);
            child.GetComponent<SlotObject>().ConnectedSlotInfo = Map[x][y];
        }

        foreach (Transform child in AllBubble.transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x - MinX);
            int y = Mathf.RoundToInt(child.localPosition.y - MinY);
            Map[x][y].InsideBubbleType = child.GetComponent<Bubble>().Type;
            Map[x][y].ConnectedBubble = child.gameObject;
        }
    }

    private void PlaceBubble(Vector2Int Pos)
    {
        List<Vector2Int> PosList = new List<Vector2Int>();
        PosList.Add(Pos);
        switch (HeldBubbleType)
        {
            case BubbleType.Disappear:
                Map[Pos.x][Pos.y].ConnectedBubble = (GameObject)Instantiate(Resources.Load("Prefabs/GameObjects/DisappearBubble"), Pos + PivotOffset, Quaternion.Euler(0, 0, 0));
                Map[Pos.x][Pos.y].InsideBubbleType = BubbleType.Disappear;
                Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Default;
                BubbleInflate(PosList);
                break;

            default:
                break;
        }
    }

    private void BubbleInflate(List<Vector2Int> PosList)
    {
        if (PosList.Count == 0)
        {
            BubbleMotionTasks.Add(new WaitTask(MotionInterval));
            ParallelTasks BubbleDeflateTasks = new ParallelTasks();
            for(int i = 0; i < Map.Count; i++)
            {
                for(int j = 0; j < Map[i].Count; j++)
                {
                    if(Map[i][j]!=null && Map[i][j].InsideBubbleType!=BubbleType.Null && Map[i][j].InsideBubbleState == BubbleState.Inflated)
                    {
                        GameObject Bubble = Map[i][j].ConnectedBubble;
                        var Data = Bubble.GetComponent<BubbleMotionData>();
                        BubbleDeflateTasks.Add(new DeflateTask(Bubble, Data.InflatedScale, Data.OriScale, Data.DeflateTime));
                    }
                }
            }
            BubbleMotionTasks.Add(BubbleDeflateTasks);
            BubbleMotionTasks.Add(new MotionFinishTask());
            return;
        }

        List<Vector2Int> NewPosList = new List<Vector2Int>();
        ParallelTasks BubbleInflateMoveBlocked = new ParallelTasks();

        for(int i = 0; i < PosList.Count; i++)
        {
            Map[PosList[i].x][PosList[i].y].InsideBubbleState = BubbleState.Inflated;
            GameObject Bubble = Map[PosList[i].x][PosList[i].y].ConnectedBubble;
            var Data = Bubble.GetComponent<BubbleMotionData>();
            BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale, Data.InflatedScale, Data.InflateTime));

            if (PosList[i].x < Map.Count - 1)
            {
                if(Map[PosList[i].x+1][PosList[i].y]!=null && Map[PosList[i].x + 1][PosList[i].y].InsideBubbleType!=BubbleType.Null && Map[PosList[i].x + 1][PosList[i].y].InsideBubbleState == BubbleState.Default)
                {
                    Bubble = Map[PosList[i].x + 1][PosList[i].y].ConnectedBubble;
                    Data = Bubble.GetComponent<BubbleMotionData>();
                    if (PosList[i].x < Map.Count - 2)
                    {
                        if (Map[PosList[i].x + 2][PosList[i].y] != null && Map[PosList[i].x + 2][PosList[i].y].InsideBubbleType != BubbleType.Null)
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x + 1, PosList[i].y));
                            BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.right,Data.BlockedDis, Data.BlockedTime));
                        }
                        else
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x + 2, PosList[i].y));
                            BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Bubble.transform.localPosition, Vector3.right, Data.MoveDis, Data.MoveTime));
                            Map[PosList[i].x + 2][PosList[i].y].ConnectedBubble = Bubble;
                            Map[PosList[i].x + 2][PosList[i].y].InsideBubbleType = Bubble.GetComponent<Bubble>().Type;
                            Map[PosList[i].x + 1][PosList[i].y].ConnectedBubble = null;
                            Map[PosList[i].x + 1][PosList[i].y].InsideBubbleType = BubbleType.Null;
                        }
                    }
                    else
                    {
                        NewPosList.Add(new Vector2Int(PosList[i].x + 1, PosList[i].y));
                        BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.right, Data.BlockedDis, Data.BlockedTime));
                    }
                }
            }

            if (PosList[i].x > 0)
            {
                if (Map[PosList[i].x - 1][PosList[i].y] != null && Map[PosList[i].x - 1][PosList[i].y].InsideBubbleType != BubbleType.Null && Map[PosList[i].x - 1][PosList[i].y].InsideBubbleState == BubbleState.Default)
                {
                    Bubble = Map[PosList[i].x - 1][PosList[i].y].ConnectedBubble;
                    Data = Bubble.GetComponent<BubbleMotionData>();
                    if (PosList[i].x > 1)
                    {
                        if (Map[PosList[i].x - 2][PosList[i].y] != null && Map[PosList[i].x - 2][PosList[i].y].InsideBubbleType != BubbleType.Null)
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x - 1, PosList[i].y));
                            BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.left, Data.BlockedDis, Data.BlockedTime));
                        }
                        else
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x - 2, PosList[i].y));
                            BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Bubble.transform.localPosition, Vector3.left, Data.MoveDis, Data.MoveTime));
                            Map[PosList[i].x - 2][PosList[i].y].ConnectedBubble = Bubble;
                            Map[PosList[i].x - 2][PosList[i].y].InsideBubbleType = Bubble.GetComponent<Bubble>().Type;
                            Map[PosList[i].x - 1][PosList[i].y].ConnectedBubble = null;
                            Map[PosList[i].x - 1][PosList[i].y].InsideBubbleType = BubbleType.Null;
                        }
                    }
                    else
                    {
                        NewPosList.Add(new Vector2Int(PosList[i].x - 1, PosList[i].y));
                        BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.left, Data.BlockedDis, Data.BlockedTime));
                    }
                }
            }

            if (PosList[i].y < Map[PosList[i].x].Count - 1)
            {
                if (Map[PosList[i].x][PosList[i].y + 1] != null && Map[PosList[i].x][PosList[i].y + 1].InsideBubbleType != BubbleType.Null && Map[PosList[i].x][PosList[i].y + 1].InsideBubbleState == BubbleState.Default)
                {
                    Bubble = Map[PosList[i].x][PosList[i].y + 1].ConnectedBubble;
                    Data = Bubble.GetComponent<BubbleMotionData>();
                    if (PosList[i].y < Map[PosList[i].x].Count - 2)
                    {
                        if (Map[PosList[i].x][PosList[i].y + 2] != null && Map[PosList[i].x][PosList[i].y + 2].InsideBubbleType != BubbleType.Null)
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x, PosList[i].y + 1));
                            BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.up, Data.BlockedDis, Data.BlockedTime));
                        }
                        else
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x, PosList[i].y + 2));
                            BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Bubble.transform.localPosition, Vector3.up, Data.MoveDis, Data.MoveTime));
                            Map[PosList[i].x][PosList[i].y + 2].ConnectedBubble = Bubble;
                            Map[PosList[i].x][PosList[i].y + 2].InsideBubbleType = Bubble.GetComponent<Bubble>().Type;
                            Map[PosList[i].x][PosList[i].y + 1].ConnectedBubble = null;
                            Map[PosList[i].x][PosList[i].y + 1].InsideBubbleType = BubbleType.Null;
                        }
                    }
                    else
                    {
                        NewPosList.Add(new Vector2Int(PosList[i].x, PosList[i].y + 1));
                        BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.up, Data.BlockedDis, Data.BlockedTime));
                    }
                }
            }

            if (PosList[i].y > 0)
            {
                if (Map[PosList[i].x][PosList[i].y - 1] != null && Map[PosList[i].x][PosList[i].y - 1].InsideBubbleType != BubbleType.Null && Map[PosList[i].x][PosList[i].y - 1].InsideBubbleState == BubbleState.Default)
                {
                    Bubble = Map[PosList[i].x][PosList[i].y - 1].ConnectedBubble;
                    Data = Bubble.GetComponent<BubbleMotionData>();
                    if (PosList[i].y > 1)
                    {
                        if (Map[PosList[i].x][PosList[i].y - 2] != null && Map[PosList[i].x][PosList[i].y - 2].InsideBubbleType != BubbleType.Null)
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x, PosList[i].y - 1));
                            BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.down, Data.BlockedDis, Data.BlockedTime));
                        }
                        else
                        {
                            NewPosList.Add(new Vector2Int(PosList[i].x, PosList[i].y - 2));
                            BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Bubble.transform.localPosition, Vector3.down, Data.MoveDis, Data.MoveTime));
                            Map[PosList[i].x][PosList[i].y - 2].ConnectedBubble = Bubble;
                            Map[PosList[i].x][PosList[i].y - 2].InsideBubbleType = Bubble.GetComponent<Bubble>().Type;
                            Map[PosList[i].x][PosList[i].y - 1].ConnectedBubble = null;
                            Map[PosList[i].x][PosList[i].y - 1].InsideBubbleType = BubbleType.Null;
                        }
                    }
                    else
                    {
                        NewPosList.Add(new Vector2Int(PosList[i].x, PosList[i].y - 1));
                        BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, Vector3.down, Data.BlockedDis, Data.BlockedTime));
                    }
                }
            }

            if (Map[PosList[i].x][PosList[i].y].ConnectedBubble.GetComponent<Bubble>().Type == BubbleType.Disappear)
            {
                Map[PosList[i].x][PosList[i].y].ConnectedBubble = null;
                Map[PosList[i].x][PosList[i].y].InsideBubbleType = BubbleType.Null;
                Map[PosList[i].x][PosList[i].y].InsideBubbleState = BubbleState.Default;
            }
        }

        BubbleMotionTasks.Add(BubbleInflateMoveBlocked);
        BubbleMotionTasks.Add(new WaitTask(MotionInterval));
        Debug.Log("cc");

        
        BubbleInflate(NewPosList);
    }

    private void Synchronize()
    {
        List<Vector2Int> BubblePosList = new List<Vector2Int>();
        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                Map[i][j].ConnectedBubble = null;
                Map[i][j].InsideBubbleState = BubbleState.Default;
                Map[i][j].InsideBubbleType = BubbleType.Null;
            }
        }

        foreach (Transform child in AllBubble.transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x - PivotOffset.x);
            int y = Mathf.RoundToInt(child.localPosition.y - PivotOffset.y);
            Map[x][y].InsideBubbleType = child.GetComponent<Bubble>().Type;
            Map[x][y].InsideBubbleState = child.GetComponent<Bubble>().State;
            Map[x][y].ConnectedBubble = child.gameObject;
        }
    }


    private void OnPlace(Place P)
    {
        Debug.Log("Place");
        PlaceBubble(P.Pos);
    }

    private void OnMotionFinish(MotionFinish M)
    {
        Debug.Log("Complete");
        Synchronize();
    }
}
