using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public int LevelIndex;
    public int DisappearBubbleInitNum;
    public int NormalBubbleInitNum;

    public GameObject AllSlot;
    public GameObject AllBubble;
    public float MotionInterval;

    private List<List<SlotInfo>> Map;
    private Vector2 PivotOffset;

    public static int RemainedDisappearBubble;
    public static int RemainedNormalBubble;

    private SerialTasks BubbleMotionTasks = new SerialTasks();

    private const float DropMoveTime = 0.1f;
    private Vector3 SlotScale = Vector3.one;

    private void OnEnable()
    {
        Map = new List<List<SlotInfo>>();
        GetMapInfo();

        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.AddHandler<LevelLoaded>(OnLevelLoaded);
    }

    void Start()
    {
        
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<LevelLoaded>(OnLevelLoaded);
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
            Map[x][y] = new SlotInfo(new Vector2Int(x,y), child.GetComponent<SlotObject>().Type, BubbleType.Null, BubbleState.Default, null);
            child.GetComponent<SlotObject>().ConnectedSlotInfo = Map[x][y];
            child.GetComponent<SlotObject>().ConnectedMap = Map;
            child.GetComponent<SlotObject>().MapPivotOffset = PivotOffset;
        }

        foreach (Transform child in AllBubble.transform)
        {
            int x = Mathf.RoundToInt(child.localPosition.x - MinX);
            int y = Mathf.RoundToInt(child.localPosition.y - MinY);
            Map[x][y].InsideBubbleType = child.GetComponent<Bubble>().Type;
            Map[x][y].ConnectedBubble = child.gameObject;
        }
    }

    private void PlaceBubble(Vector2Int Pos, BubbleType Type)
    {
        List<Vector2Int> PosList = new List<Vector2Int>();
        PosList.Add(Pos);

        switch (Type)
        {
            case BubbleType.Disappear:

                RemainedDisappearBubble--;
                EventManager.instance.Fire(new BubbleNumSet(BubbleType.Disappear,RemainedDisappearBubble));
                if (RemainedDisappearBubble == 0)
                {
                    CheckAvailableBubble();
                }

                Map[Pos.x][Pos.y].ConnectedBubble = (GameObject)Instantiate(Resources.Load("Prefabs/GameObjects/DisappearBubble"), Vector3.back * Camera.main.transform.position.z + Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.Euler(0, 0, 0));
                break;
            case BubbleType.Normal:

                RemainedNormalBubble--;
                EventManager.instance.Fire(new BubbleNumSet(BubbleType.Normal,RemainedNormalBubble));
                if (RemainedNormalBubble == 0)
                {
                    CheckAvailableBubble();
                }

                Map[Pos.x][Pos.y].ConnectedBubble = (GameObject)Instantiate(Resources.Load("Prefabs/GameObjects/NormalBubble"), Vector3.back * Camera.main.transform.position.z + Camera.main.ScreenToWorldPoint(Input.mousePosition), Quaternion.Euler(0, 0, 0));
                break;
        }

        Map[Pos.x][Pos.y].InsideBubbleType = Type;
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Default;
        Map[Pos.x][Pos.y].ConnectedBubble.transform.parent = AllBubble.transform;
        Map[Pos.x][Pos.y].ConnectedBubble.transform.localScale = Vector3.one * GetComponent<BubbleMotionData>().DropScale;
        AddDropMoveTask(Map[Pos.x][Pos.y].ConnectedBubble, Pos);
        BubbleInflate(PosList, true);
    }

    private void CheckAvailableBubble()
    {
        if(RemainedDisappearBubble > 0)
        {
            GameManager.HeldBubbleType = BubbleType.Disappear;
        }
        else if(RemainedNormalBubble > 0)
        {
            GameManager.HeldBubbleType = BubbleType.Normal;
        }
        else
        {
            GameManager.HeldBubbleType = BubbleType.Null;
        }
    }

    private void AddDropMoveTask(GameObject Bubble, Vector2Int Pos)
    {
        var Data = GetComponent<BubbleMotionData>();
        Vector3 Start = Bubble.transform.localPosition;
        Vector3 End = Pos + PivotOffset;
        BubbleMotionTasks.Add(new MoveTask(Bubble, Start, (End - Start).normalized, (End - Start).magnitude, DropMoveTime));
    }

    

    private void BubbleInflate(List<Vector2Int> PosList, bool Drop)
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
                        var Data = GetComponent<BubbleMotionData>();
                        

                        if(Map[i][j].InsideBubbleType == BubbleType.Disappear)
                        {
                            BubbleDeflateTasks.Add(new DeflateTask(Bubble, Data.InflatedScale * Vector3.one, Vector3.zero, Data.DeflateTime));
                            Map[i][j].ConnectedBubble = null;
                            Map[i][j].InsideBubbleType = BubbleType.Null;
                            Map[i][j].InsideBubbleState = BubbleState.Default;
                        }
                        else
                        {
                            BubbleDeflateTasks.Add(new DeflateTask(Bubble, Data.InflatedScale * Vector3.one, Data.OriScale * Vector3.one, Data.DeflateTime));
                        }
                    }
                }
            }
            BubbleMotionTasks.Add(BubbleDeflateTasks);
            BubbleMotionTasks.Add(new MotionFinishTask());
            return;
        }

        List<Vector2Int> NewPosList = new List<Vector2Int>();
        ParallelTasks BubbleInflateMoveBlocked = new ParallelTasks();

        List<List<int>> PosTarget = new List<List<int>>();

        for(int i = 0; i < Map.Count; i++)
        {
            PosTarget.Add(new List<int>());
            for(int j = 0; j < Map[i].Count; j++)
            {
                PosTarget[i].Add(0);
            }
        }

        List<MoveInfo> Moves = new List<MoveInfo>();

        for(int i = 0; i < PosList.Count; i++)
        {
            Map[PosList[i].x][PosList[i].y].InsideBubbleState = BubbleState.Inflated;
            GameObject Bubble = Map[PosList[i].x][PosList[i].y].ConnectedBubble;
            var Data = GetComponent<BubbleMotionData>();
            if (Drop)
            {
                BubbleMotionTasks.Add(new InflateTask(Bubble, Data.DropScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime * (Data.OriScale - Data.DropScale) / Data.InflatedScale));
                BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale * Vector3.one, Data.InflatedScale * Vector3.one, Data.InflateTime * (Data.InflatedScale-Data.OriScale)/Data.InflatedScale));
            }
            else
            {
                BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale * Vector3.one, Data.InflatedScale * Vector3.one, Data.InflateTime));
            }
            
            if(PosList[i].x < Map.Count - 1)
            {
                if(Map[PosList[i].x + 1][PosList[i].y]!=null && Map[PosList[i].x + 1][PosList[i].y].InsideBubbleType!=BubbleType.Null && Map[PosList[i].x + 1][PosList[i].y].InsideBubbleState == BubbleState.Default)
                {
                    Moves.Add(new MoveInfo(Direction.Right, new Vector2Int(PosList[i].x + 1, PosList[i].y)));
                }
                
            }

            if(PosList[i].x > 0)
            {
                if (Map[PosList[i].x - 1][PosList[i].y] != null && Map[PosList[i].x - 1][PosList[i].y].InsideBubbleType != BubbleType.Null && Map[PosList[i].x - 1][PosList[i].y].InsideBubbleState == BubbleState.Default)
                {
                    Moves.Add(new MoveInfo(Direction.Left, new Vector2Int(PosList[i].x - 1, PosList[i].y)));
                }
            }

            if (PosList[i].y < Map[PosList[i].x].Count - 1)
            {
                if (Map[PosList[i].x][PosList[i].y + 1] != null && Map[PosList[i].x][PosList[i].y + 1].InsideBubbleType != BubbleType.Null && Map[PosList[i].x][PosList[i].y + 1].InsideBubbleState == BubbleState.Default)
                {
                    Moves.Add(new MoveInfo(Direction.Up, new Vector2Int(PosList[i].x, PosList[i].y + 1)));
                }
            }

            if (PosList[i].y > 0)
            {
                if (Map[PosList[i].x][PosList[i].y - 1] != null && Map[PosList[i].x][PosList[i].y - 1].InsideBubbleType != BubbleType.Null && Map[PosList[i].x][PosList[i].y - 1].InsideBubbleState == BubbleState.Default)
                {
                    Moves.Add(new MoveInfo(Direction.Down, new Vector2Int(PosList[i].x, PosList[i].y - 1)));
                }
            }
        }

        for (int k = 0; k < Moves.Count; k++)
        {
            int x = Moves[k].TargetPos.x;
            int y = Moves[k].TargetPos.y;

            if (!(x < 0 || y < 0 || x >= Map.Count || y >= Map[x].Count || Map[x][y] == null))
            {
                PosTarget[x][y]++;
            }
        }

        for (int k = 0; k < Moves.Count; k++)
        {
            Vector2 dir;
            switch (Moves[k].direction)
            {
                case Direction.Left:
                    dir = Vector2.left;
                    break;
                case Direction.Right:
                    dir = Vector2.right;
                    break;
                case Direction.Up:
                    dir = Vector2.up;
                    break;
                case Direction.Down:
                    dir = Vector2.down;
                    break;
                default:
                    dir = Vector2.zero;
                    break;
            }

            int x = Moves[k].TargetPos.x;
            int y = Moves[k].TargetPos.y;

            GameObject Bubble = Map[Moves[k].CurrentPos.x][Moves[k].CurrentPos.y].ConnectedBubble;
            var Data = GetComponent<BubbleMotionData>();

            if (x < 0 || y < 0 || x >= Map.Count || y >= Map[x].Count || Map[x][y] == null || Map[x][y].InsideBubbleType != BubbleType.Null)
            {
                NewPosList.Add(new Vector2Int(Moves[k].CurrentPos.x, Moves[k].CurrentPos.y));
                BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, dir, Data.BlockedDis, Data.BlockedTime));
            }
            else
            {
                if (PosTarget[x][y] > 1)
                {
                    NewPosList.Add(new Vector2Int(Moves[k].CurrentPos.x, Moves[k].CurrentPos.y));
                    BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, dir, Data.ConflictBlockedDis, Data.BlockedTime));
                }
                else
                {
                    NewPosList.Add(new Vector2Int(x, y));
                    BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Bubble.transform.localPosition, dir, Data.MoveDis, Data.MoveTime));
                    Map[x][y].ConnectedBubble = Bubble;
                    Map[x][y].InsideBubbleType = Bubble.GetComponent<Bubble>().Type;
                    Map[Moves[k].CurrentPos.x][Moves[k].CurrentPos.y].ConnectedBubble = null;
                    Map[Moves[k].CurrentPos.x][Moves[k].CurrentPos.y].InsideBubbleType = BubbleType.Null;
                }
            }

        }

        BubbleMotionTasks.Add(BubbleInflateMoveBlocked);
        BubbleMotionTasks.Add(new WaitTask(MotionInterval));
        
        BubbleInflate(NewPosList, false);
    }

    private void Synchronize()
    {
        List<Vector2Int> BubblePosList = new List<Vector2Int>();
        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                if (Map[i][j] != null)
                {
                    Map[i][j].ConnectedBubble = null;
                    Map[i][j].InsideBubbleState = BubbleState.Default;
                    Map[i][j].InsideBubbleType = BubbleType.Null;
                }
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

    private void CheckLevelState()
    {
        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                if(Map[i][j]!=null && Map[i][j].slotType == SlotType.Target)
                {
                    
                    if (Map[i][j].InsideBubbleType == BubbleType.Null)
                    {
                        if(RemainedDisappearBubble == 0 && RemainedNormalBubble == 0)
                        {
                            EventManager.instance.Fire(new LevelFinish(LevelIndex, false));
                        }
                        return;
                    }
                }
            }
        }

        EventManager.instance.Fire(new LevelFinish(LevelIndex, true));
    }

    private void OnPlace(Place P)
    {
        if (gameObject.activeSelf)
        {
            PlaceBubble(P.Pos, P.Type);
        }
    }

    private void OnMotionFinish(MotionFinish M)
    {
        if (gameObject.activeSelf)
        {
            Synchronize();
            CheckLevelState();
        }
    }

    private void OnLevelLoaded(LevelLoaded L)
    {
        if (L.Index == LevelIndex && gameObject.activeSelf)
        {
            RemainedDisappearBubble = DisappearBubbleInitNum;
            RemainedNormalBubble = NormalBubbleInitNum;
            EventManager.instance.Fire(new BubbleNumSet(BubbleType.Disappear,RemainedDisappearBubble));
            EventManager.instance.Fire(new BubbleNumSet(BubbleType.Normal,RemainedNormalBubble));
            CheckAvailableBubble();
        }
    }
}
