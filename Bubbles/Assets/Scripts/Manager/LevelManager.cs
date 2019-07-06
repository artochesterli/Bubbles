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
    public float TeleportWait;

    private List<List<SlotInfo>> Map;
    private Vector2 PivotOffset;
    private SlotInfo TeleportSlot1, TeleportSlot2;

    public static int RemainedDisappearBubble;
    public static int RemainedNormalBubble;

    private SerialTasks BubbleMotionTasks = new SerialTasks();

    private List<SerialTasks> BackTaskList = new List<SerialTasks>();
    private List<Task> CurrentBackTasks = new List<Task>();

    private List<BubbleType> UsedBubble = new List<BubbleType>();

    private Vector3 SlotScale = Vector3.one;
    private const float DropMoveTime = 0.1f;
    

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

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<LevelLoaded>(OnLevelLoaded);
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

        if(GameManager.State==GameState.Play && Input.GetMouseButtonDown(1) && BackTaskList.Count > 0)
        {
            
            StartCoroutine(Back());
        }
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
            Map[x][y] = new SlotInfo(new Vector2Int(x,y), child.GetComponent<SlotObject>().Type, BubbleType.Null, BubbleState.Stable, null, child.localPosition);
            child.GetComponent<SlotObject>().ConnectedSlotInfo = Map[x][y];
            child.GetComponent<SlotObject>().ConnectedMap = Map;
            child.GetComponent<SlotObject>().MapPivotOffset = PivotOffset;
            if (Map[x][y].slotType == SlotType.Teleport)
            {
                if (TeleportSlot1 == null)
                {
                    TeleportSlot1 = Map[x][y];
                }
                else
                {
                    TeleportSlot2 = Map[x][y];
                }
            }
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
        CurrentBackTasks.Clear();

        UsedBubble.Add(Type);

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
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Activated;
        Map[Pos.x][Pos.y].ConnectedBubble.GetComponent<Bubble>().State = BubbleState.Activated;
        Map[Pos.x][Pos.y].ConnectedBubble.transform.parent = AllBubble.transform;
        Map[Pos.x][Pos.y].ConnectedBubble.transform.localScale = Vector3.one * GetComponent<BubbleMotionData>().OriScale;

        AddDropMoveTask(Map[Pos.x][Pos.y].ConnectedBubble, Pos);

        var Data = GetComponent<BubbleMotionData>();

        CurrentBackTasks.Insert(0, new DisappearTask(Map[Pos.x][Pos.y].ConnectedBubble, GetComponent<BubbleMotionData>().DisappearTime, Pos, Map, Type));

        if (Map[Pos.x][Pos.y] == TeleportSlot1)
        {
            if (TeleportSlot2.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = Map[Pos.x][Pos.y].ConnectedBubble;
                CurrentBackTasks.Insert(0, new InflateTask(Bubble, Data.TeleportScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime, Pos, Map));
                CurrentBackTasks.Insert(0, new MoveTask(Bubble, TeleportSlot2.Location, (TeleportSlot1.Location - TeleportSlot2.Location).normalized, (TeleportSlot1.Location - TeleportSlot2.Location).magnitude, 0, TeleportSlot2.Pos, Pos, TeleportSlot1.InsideBubbleType, Map, BubbleTaskMode.Delay));
                CurrentBackTasks.Insert(0, new DeflateTask(Bubble, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot2.Pos, Map));
                Teleport(Bubble, TeleportSlot2);
                Pos = TeleportSlot2.Pos;
                
            }
        }
        else if (Map[Pos.x][Pos.y] == TeleportSlot2)
        {
            if (TeleportSlot1.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = Map[Pos.x][Pos.y].ConnectedBubble;
                CurrentBackTasks.Insert(0, new InflateTask(Bubble, Data.TeleportScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime, Pos, Map));
                CurrentBackTasks.Insert(0, new MoveTask(Bubble, TeleportSlot1.Location, (TeleportSlot2.Location - TeleportSlot1.Location).normalized, (TeleportSlot2.Location - TeleportSlot1.Location).magnitude, 0, TeleportSlot1.Pos, Pos, TeleportSlot2.InsideBubbleType, Map, BubbleTaskMode.Delay));
                CurrentBackTasks.Insert(0, new DeflateTask(Bubble, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot1.Pos, Map));
                Teleport(Bubble, TeleportSlot1);
                Pos = TeleportSlot1.Pos;
            }
        }

        List<Vector2Int> PosList = new List<Vector2Int>();
        PosList.Add(Pos);

        BubbleInflate(PosList, true);

        BackTaskList.Add(new SerialTasks());
        for (int i = 0; i < CurrentBackTasks.Count; i++)
        {
            BackTaskList[BackTaskList.Count - 1].Add(CurrentBackTasks[i]);
        }
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
        BubbleMotionTasks.Add(new MoveTask(Bubble, Start, (End - Start).normalized, (End - Start).magnitude, DropMoveTime,Vector2Int.zero,Vector2Int.zero));
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
                    if(Map[i][j]!=null && Map[i][j].InsideBubbleType!=BubbleType.Null && Map[i][j].InsideBubbleState == BubbleState.Exhausted)
                    {
                        GameObject Bubble = Map[i][j].ConnectedBubble;
                        var Data = GetComponent<BubbleMotionData>();
                        

                        if(Map[i][j].InsideBubbleType == BubbleType.Disappear)
                        {
                            BubbleDeflateTasks.Add(new DeflateTask(Bubble, Data.InflatedScale * Vector3.one, Vector3.zero, Data.DeflateTime, new Vector2Int(i,j), Map, BubbleTaskMode.Immediate));
                            Map[i][j].ConnectedBubble = null;
                            Map[i][j].InsideBubbleType = BubbleType.Null;
                            Map[i][j].InsideBubbleState = BubbleState.Stable;
                        }
                        else
                        {
                            BubbleDeflateTasks.Add(new DeflateTask(Bubble, Data.InflatedScale * Vector3.one, Data.OriScale * Vector3.one, Data.DeflateTime, new Vector2Int(i, j), Map, BubbleTaskMode.Immediate));
                        }
                    }
                }
            }
            BubbleMotionTasks.Add(BubbleDeflateTasks);
            BubbleMotionTasks.Add(new MotionFinishTask());
            return;
        }

        List<Vector2Int> NewPosList = new List<Vector2Int>();
        Dictionary<GameObject, Vector2Int> InflateDic = new Dictionary<GameObject, Vector2Int>();


        SetBubbleMovement(PosList, InflateDic,false);

        BubbleMotionTasks.Add(new WaitTask(MotionInterval));
        
        foreach(KeyValuePair<GameObject,Vector2Int> entry in InflateDic)
        {
            NewPosList.Add(entry.Value);
        }

        BubbleInflate(NewPosList, false);
    }

    private bool AvailableForPush(SlotInfo S)
    {
        return S != null && S.InsideBubbleType != BubbleType.Null && (S.InsideBubbleState == BubbleState.Activated || S.InsideBubbleState == BubbleState.Stable);
    }

    private void SetBubbleMovement(List<Vector2Int> PosList, Dictionary<GameObject, Vector2Int> InflateDic, bool IsTeleport)
    {
        ParallelTasks BackMovementSet = new ParallelTasks();

        ParallelTasks BubbleInflateMoveBlocked = new ParallelTasks();

        List<List<int>> PosTarget = new List<List<int>>();

        List<MoveInfo> Moves = new List<MoveInfo>();

        var Data = GetComponent<BubbleMotionData>();

        for (int i = 0; i < Map.Count; i++)
        {
            PosTarget.Add(new List<int>());
            for (int j = 0; j < Map[i].Count; j++)
            {
                PosTarget[i].Add(0);
            }
        }

        for (int i = 0; i < PosList.Count; i++)
        {
            GameObject Bubble = Map[PosList[i].x][PosList[i].y].ConnectedBubble;

            if (IsTeleport)
            {
                BubbleMotionTasks.Add(new InflateTask(Bubble, Data.TeleportScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime*Data.OriScale/Data.InflatedScale, PosList[i], Map));
                BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale * Vector3.one, Data.InflatedScale * Vector3.one, Data.InflateTime* (Data.InflatedScale-Data.OriScale / Data.InflatedScale), PosList[i], Map, BubbleTaskMode.Immediate));
            }
            else
            {
                BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale * Vector3.one, Data.InflatedScale * Vector3.one, Data.InflateTime, PosList[i], Map, BubbleTaskMode.Immediate));
            }

            if (PosList[i].x < Map.Count - 1)
            {
                if (AvailableForPush(Map[PosList[i].x + 1][PosList[i].y]))
                {
                    Moves.Add(new MoveInfo(Direction.Right, new Vector2Int(PosList[i].x + 1, PosList[i].y)));
                }

            }

            if (PosList[i].x > 0)
            {
                if (AvailableForPush(Map[PosList[i].x - 1][PosList[i].y]))
                {
                    Moves.Add(new MoveInfo(Direction.Left, new Vector2Int(PosList[i].x - 1, PosList[i].y)));
                }
            }

            if (PosList[i].y < Map[PosList[i].x].Count - 1)
            {
                if (AvailableForPush(Map[PosList[i].x][PosList[i].y + 1]))
                {
                    Moves.Add(new MoveInfo(Direction.Up, new Vector2Int(PosList[i].x, PosList[i].y + 1)));
                }
            }

            if (PosList[i].y > 0)
            {
                if (AvailableForPush(Map[PosList[i].x][PosList[i].y - 1]))
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

        bool Slot1Active = false;
        bool Slot2Active = false;
        

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

            if (x < 0 || y < 0 || x >= Map.Count || y >= Map[x].Count || Map[x][y] == null || Map[x][y].InsideBubbleType != BubbleType.Null)
            {
                if (InflateDic.ContainsKey(Bubble))
                {
                    InflateDic[Bubble] = Moves[k].CurrentPos;
                }
                else
                {
                    InflateDic.Add(Bubble, Moves[k].CurrentPos);
                }

                BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, dir, Data.BlockedDis, Data.BlockedTime, Moves[k].CurrentPos, Map));
            }
            else
            {
                if (PosTarget[x][y] > 1)
                {
                    if (InflateDic.ContainsKey(Bubble))
                    {
                        InflateDic[Bubble] = Moves[k].CurrentPos;
                    }
                    else
                    {
                        InflateDic.Add(Bubble, Moves[k].CurrentPos);
                    }

                    BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Bubble.transform.localPosition, dir, Data.ConflictBlockedDis, Data.BlockedTime, Moves[k].CurrentPos, Map));
                }
                else
                {
                    if (InflateDic.ContainsKey(Bubble))
                    {
                        InflateDic[Bubble] = Moves[k].TargetPos;
                    }
                    else
                    {
                        InflateDic.Add(Bubble, Moves[k].TargetPos);
                    }

                    BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Bubble.transform.localPosition, dir, Data.MoveDis, Data.MoveTime, Moves[k].CurrentPos, Moves[k].TargetPos, Bubble.GetComponent<Bubble>().Type, Map, BubbleTaskMode.Immediate));

                    BackMovementSet.Add(new MoveTask(Bubble, Bubble.transform.localPosition + (Vector3)dir * Data.MoveDis, -dir, Data.MoveDis, Data.MoveTime, Moves[k].TargetPos, Moves[k].CurrentPos, Bubble.GetComponent<Bubble>().Type, Map, BubbleTaskMode.Delay));
                }
            }

            Vector2Int v = InflateDic[Bubble];

            if (Map[v.x][v.y] == TeleportSlot1)
            {
                Slot1Active = true;
            }

            if (Map[v.x][v.y] == TeleportSlot2)
            {
                Slot2Active = true;
            }

        }

        CurrentBackTasks.Insert(0, BackMovementSet);

        BubbleMotionTasks.Add(BubbleInflateMoveBlocked);

        List<Vector2Int> TeleportPos = new List<Vector2Int>();

        if (Slot1Active && !Slot2Active || !Slot1Active && Slot2Active)
        {
            if(Slot1Active && TeleportSlot2.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = TeleportSlot1.ConnectedBubble;

                CurrentBackTasks.Insert(0, new InflateTask(Bubble, Data.TeleportScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime, TeleportSlot1.Pos, Map));
                CurrentBackTasks.Insert(0, new MoveTask(Bubble, TeleportSlot2.Location, (TeleportSlot1.Location - TeleportSlot2.Location).normalized, (TeleportSlot1.Location - TeleportSlot2.Location).magnitude, 0, TeleportSlot2.Pos, TeleportSlot1.Pos, TeleportSlot1.InsideBubbleType, Map, BubbleTaskMode.Delay));
                CurrentBackTasks.Insert(0, new DeflateTask(Bubble, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot2.Pos, Map));

                if (InflateDic.ContainsKey(Bubble))
                {
                    InflateDic.Remove(Bubble);
                }

                TeleportPos.Add(TeleportSlot2.Pos);
                Teleport(TeleportSlot1.ConnectedBubble, TeleportSlot2);
            }
            else if(Slot2Active && TeleportSlot1.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = TeleportSlot2.ConnectedBubble;

                CurrentBackTasks.Insert(0, new InflateTask(Bubble, Data.TeleportScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime, TeleportSlot2.Pos, Map));
                CurrentBackTasks.Insert(0, new MoveTask(Bubble, TeleportSlot1.Location, (TeleportSlot2.Location - TeleportSlot1.Location).normalized, (TeleportSlot2.Location - TeleportSlot1.Location).magnitude, 0, TeleportSlot1.Pos, TeleportSlot2.Pos, TeleportSlot2.InsideBubbleType, Map, BubbleTaskMode.Delay));
                CurrentBackTasks.Insert(0, new DeflateTask(Bubble, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot1.Pos, Map));

                if (InflateDic.ContainsKey(Bubble))
                {
                    InflateDic.Remove(Bubble);
                }

                TeleportPos.Add(TeleportSlot1.Pos);
                Teleport(TeleportSlot2.ConnectedBubble, TeleportSlot1);
            }
        }

        if (TeleportPos.Count == 1)
        {
            SetBubbleMovement(TeleportPos, InflateDic, true);
        }
        
    }

    private void Teleport(GameObject Obj, SlotInfo Target)
    {
        var Data = GetComponent<BubbleMotionData>();
        BubbleMotionTasks.Add(new WaitTask(TeleportWait));

        if (Target == TeleportSlot1)
        {
            BubbleMotionTasks.Add(new DeflateTask(Obj, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot2.Pos, Map));
            BubbleMotionTasks.Add(new MoveTask(Obj, TeleportSlot2.Location, (TeleportSlot1.Location - TeleportSlot2.Location).normalized, (TeleportSlot1.Location - TeleportSlot2.Location).magnitude, 0, TeleportSlot2.Pos, TeleportSlot1.Pos, TeleportSlot2.InsideBubbleType, Map, BubbleTaskMode.Immediate));
        }
        else
        {
            BubbleMotionTasks.Add(new DeflateTask(Obj, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot1.Pos, Map));
            BubbleMotionTasks.Add(new MoveTask(Obj, TeleportSlot1.Location, (TeleportSlot2.Location - TeleportSlot1.Location).normalized, (TeleportSlot2.Location - TeleportSlot1.Location).magnitude, 0, TeleportSlot1.Pos, TeleportSlot2.Pos, TeleportSlot1.InsideBubbleType, Map, BubbleTaskMode.Immediate));
        }
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
                    Map[i][j].InsideBubbleState = BubbleState.Stable;
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
        GameManager.State = GameState.Play;
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

    private IEnumerator Back()
    {
        GameManager.HeldBubbleType = UsedBubble[UsedBubble.Count - 1];

        GameManager.State = GameState.Show;

        SerialTasks BackTask = BackTaskList[BackTaskList.Count - 1];
        while (!BackTask.IsFinished)
        {
            BackTask.Update();
            yield return null;
        }

        UsedBubble.RemoveAt(UsedBubble.Count - 1);
        BackTaskList.Remove(BackTask);
        GameManager.State = GameState.Play;
    }
}
