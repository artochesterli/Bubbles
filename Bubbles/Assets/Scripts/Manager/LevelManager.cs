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
    public float RoundEndInterval;
    public float RollBackTime;

    private List<List<SlotInfo>> Map;
    private Vector2 PivotOffset;
    private SlotInfo TeleportSlot1, TeleportSlot2;

    public static int RemainedDisappearBubble;
    public static int RemainedNormalBubble;

    private SerialTasks BubbleMotionTasks = new SerialTasks();

    private List<List<BubbleChangeInfo>> ChangeInfoList = new List<List<BubbleChangeInfo>>();

    private Dictionary<GameObject, Vector2Int> OriginBubblePosDic = new Dictionary<GameObject, Vector2Int>();
    private Dictionary<GameObject, Vector2Int> ChangedBubblePosDic = new Dictionary<GameObject, Vector2Int>();

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

        if(GameManager.State==GameState.Play && Input.GetMouseButtonDown(1) && ChangeInfoList.Count > 0)
        {
            StartCoroutine(RollBack());
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
        ChangeInfoList.Add(new List<BubbleChangeInfo>());

        OriginBubblePosDic.Clear();
        ChangedBubblePosDic.Clear();

        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                if(Map[i][j]!=null && Map[i][j].InsideBubbleType != BubbleType.Null)
                {
                    OriginBubblePosDic.Add(Map[i][j].ConnectedBubble, new Vector2Int(i, j));
                    ChangedBubblePosDic.Add(Map[i][j].ConnectedBubble, new Vector2Int(i, j));
                }
            }
        }

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
        Map[Pos.x][Pos.y].ConnectedBubble.transform.Find("StableEffect").GetComponent<ParticleSystem>().Stop();
        Map[Pos.x][Pos.y].ConnectedBubble.transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Play();
        Map[Pos.x][Pos.y].ConnectedBubble.transform.parent = AllBubble.transform;
        Map[Pos.x][Pos.y].ConnectedBubble.transform.localScale = Vector3.one * GetComponent<BubbleMotionData>().NormalScale;

        var Data = GetComponent<BubbleMotionData>();

        Vector3 Start = Map[Pos.x][Pos.y].ConnectedBubble.transform.localPosition;
        Vector3 End = Map[Pos.x][Pos.y].Location;
        BubbleMotionTasks.Add(new TransformTask(Map[Pos.x][Pos.y].ConnectedBubble, Start, End, DropMoveTime));

        if (Map[Pos.x][Pos.y] == TeleportSlot1)
        {
            if (TeleportSlot2.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = Map[Pos.x][Pos.y].ConnectedBubble;
                Teleport(Bubble, TeleportSlot2);
                Pos = TeleportSlot2.Pos;
                
            }
        }
        else if (Map[Pos.x][Pos.y] == TeleportSlot2)
        {
            if (TeleportSlot1.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = Map[Pos.x][Pos.y].ConnectedBubble;
                Teleport(Bubble, TeleportSlot1);
                Pos = TeleportSlot1.Pos;
            }
        }

        ChangeInfoList[ChangeInfoList.Count - 1].Add(new BubbleChangeInfo(Map[Pos.x][Pos.y].ConnectedBubble, Type, true, Pos, Pos, Map[Pos.x][Pos.y].Location, Map[Pos.x][Pos.y].Location));

        List<Vector2Int> PosList = new List<Vector2Int>();
        PosList.Add(Pos);

        BubbleInflate(PosList, true);

        foreach(GameObject Bubble in OriginBubblePosDic.Keys)
        {
            Vector2Int From = OriginBubblePosDic[Bubble];
            Vector2Int To = ChangedBubblePosDic[Bubble];
            Vector3 BeginPos = Map[From.x][From.y].Location;
            Vector3 EndPos = Map[To.x][To.y].Location;
            ChangeInfoList[ChangeInfoList.Count - 1].Add(new BubbleChangeInfo(Bubble,Bubble.GetComponent<Bubble>().Type, false, From, To, BeginPos, EndPos));
        }
    }

    private void CheckAvailableBubble()
    {
        if (RemainedDisappearBubble > 0)
        {
            GameManager.HeldBubbleType = BubbleType.Disappear;
        }
        else if (RemainedNormalBubble > 0)
        {
            GameManager.HeldBubbleType = BubbleType.Normal;
        }
        else
        {
            GameManager.HeldBubbleType = BubbleType.Null;
        }
    }

    private void BubbleInflate(List<Vector2Int> PosList, bool Drop)
    {
        if (PosList.Count == 0)
        {
            ParallelTasks BubbleRecoverTasks = new ParallelTasks();
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
                            BubbleRecoverTasks.Add(new DisappearTask(Bubble, Data.RecoverTime, new Vector2Int(i, j), Map, BubbleType.Disappear, false));
                            Map[i][j].ConnectedBubble = null;
                            Map[i][j].InsideBubbleType = BubbleType.Null;
                            Map[i][j].InsideBubbleState = BubbleState.Stable;
                        }
                        else
                        {
                            BubbleRecoverTasks.Add(new RecoverTask(Bubble, Data.RecoverTime, Data.ExhaustScale, Data.NormalScale, Bubble.GetComponent<Bubble>().ExhaustColor, Bubble.GetComponent<Bubble>().NormalColor, Map, new Vector2Int(i, j)));
                        }
                    }
                }
            }
            BubbleMotionTasks.Add(BubbleRecoverTasks);
            BubbleMotionTasks.Add(new MotionFinishTask());
            return;
        }

        List<Vector2Int> NewPosList = new List<Vector2Int>();
        Dictionary<GameObject, Vector2Int> InflateDic = new Dictionary<GameObject, Vector2Int>();


        SetBubbleMovement(PosList, InflateDic,false);
        
        foreach(KeyValuePair<GameObject,Vector2Int> entry in InflateDic)
        {
            NewPosList.Add(entry.Value);
        }

        if (NewPosList.Count > 0)
        {
            BubbleMotionTasks.Add(new WaitTask(MotionInterval));
        }
        else
        {
            BubbleMotionTasks.Add(new WaitTask(RoundEndInterval));
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
                //BubbleMotionTasks.Add(new InflateTask(Bubble, Data.TeleportScale * Vector3.one, Data.OriScale * Vector3.one, Data.InflateTime*Data.OriScale/Data.InflatedScale, PosList[i], Map));
                //BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale * Vector3.one, Data.InflatedScale * Vector3.one, Data.InflateTime* (Data.InflatedScale-Data.OriScale / Data.InflatedScale), PosList[i], Map, BubbleTaskMode.Immediate));
            }
            else
            {
                BubbleInflateMoveBlocked.Add(new ReleaseTask(Bubble, Data.MotionTime, Data.NormalScale, Data.ExhaustScale, Bubble.GetComponent<Bubble>().NormalColor, Bubble.GetComponent<Bubble>().ExhaustColor, Map, PosList[i]));
                //BubbleInflateMoveBlocked.Add(new InflateTask(Bubble, Data.OriScale * Vector3.one, Data.InflatedScale * Vector3.one, Data.InflateTime, PosList[i], Map, BubbleTaskMode.Immediate));
            }

            if (PosList[i].x < Map.Count - 1)
            {
                if (AvailableForPush(Map[PosList[i].x + 1][PosList[i].y]))
                {
                    Moves.Add(new MoveInfo(Direction.Right, new Vector2Int(PosList[i].x + 1, PosList[i].y), Map[PosList[i].x + 1][PosList[i].y].Location));
                }

            }

            if (PosList[i].x > 0)
            {
                if (AvailableForPush(Map[PosList[i].x - 1][PosList[i].y]))
                {
                    Moves.Add(new MoveInfo(Direction.Left, new Vector2Int(PosList[i].x - 1, PosList[i].y), Map[PosList[i].x - 1][PosList[i].y].Location));
                }
            }

            if (PosList[i].y < Map[PosList[i].x].Count - 1)
            {
                if (AvailableForPush(Map[PosList[i].x][PosList[i].y + 1]))
                {
                    Moves.Add(new MoveInfo(Direction.Up, new Vector2Int(PosList[i].x, PosList[i].y + 1), Map[PosList[i].x][PosList[i].y + 1].Location));
                }
            }

            if (PosList[i].y > 0)
            {
                if (AvailableForPush(Map[PosList[i].x][PosList[i].y - 1]))
                {
                    Moves.Add(new MoveInfo(Direction.Down, new Vector2Int(PosList[i].x, PosList[i].y - 1), Map[PosList[i].x][PosList[i].y - 1].Location));
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

                BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Moves[k].CurrentLocation, dir, Data.BlockedDis, Data.MotionTime, Moves[k].CurrentPos, Map));
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

                    BubbleInflateMoveBlocked.Add(new BlockedTask(Bubble, Moves[k].CurrentLocation, dir, Data.ConflictBlockedDis, Data.MotionTime, Moves[k].CurrentPos, Map));
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

                    BubbleInflateMoveBlocked.Add(new MoveTask(Bubble, Moves[k].CurrentLocation, Moves[k].TargetLocation, Data.MotionTime, Moves[k].CurrentPos, Moves[k].TargetPos, Bubble.GetComponent<Bubble>().Type, Map));

                    ChangedBubblePosDic[Bubble] = Moves[k].TargetPos;
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

        BubbleMotionTasks.Add(BubbleInflateMoveBlocked);

        List<Vector2Int> TeleportPos = new List<Vector2Int>();

        if (Slot1Active && !Slot2Active || !Slot1Active && Slot2Active)
        {
            if(Slot1Active && TeleportSlot2.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = TeleportSlot1.ConnectedBubble;

                if (InflateDic.ContainsKey(Bubble))
                {
                    InflateDic.Remove(Bubble);
                }

                ChangedBubblePosDic[Bubble] = TeleportSlot2.Pos;

                TeleportPos.Add(TeleportSlot2.Pos);
                Teleport(TeleportSlot1.ConnectedBubble, TeleportSlot2);
            }
            else if(Slot2Active && TeleportSlot1.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = TeleportSlot2.ConnectedBubble;

                if (InflateDic.ContainsKey(Bubble))
                {
                    InflateDic.Remove(Bubble);
                }

                ChangedBubblePosDic[Bubble] = TeleportSlot1.Pos;

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

        if (Target == TeleportSlot1)
        {
            //BubbleMotionTasks.Add(new DeflateTask(Obj, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot2.Pos, Map));
            //BubbleMotionTasks.Add(new MoveTask(Obj, TeleportSlot2.Location, (TeleportSlot1.Location - TeleportSlot2.Location).normalized, (TeleportSlot1.Location - TeleportSlot2.Location).magnitude, 0, TeleportSlot2.Pos, TeleportSlot1.Pos, TeleportSlot2.InsideBubbleType, Map, BubbleTaskMode.Immediate));
        }
        else
        {
            //BubbleMotionTasks.Add(new DeflateTask(Obj, Data.OriScale * Vector3.one, Data.TeleportScale * Vector3.one, Data.DeflateTime, TeleportSlot1.Pos, Map));
            //BubbleMotionTasks.Add(new MoveTask(Obj, TeleportSlot1.Location, (TeleportSlot2.Location - TeleportSlot1.Location).normalized, (TeleportSlot2.Location - TeleportSlot1.Location).magnitude, 0, TeleportSlot1.Pos, TeleportSlot2.Pos, TeleportSlot1.InsideBubbleType, Map, BubbleTaskMode.Immediate));
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

    private IEnumerator RollBack()
    {

        GameManager.State = GameState.Show;

        List<BubbleChangeInfo> list = ChangeInfoList[ChangeInfoList.Count - 1];

        ParallelTasks RollBackTask = new ParallelTasks();


        for(int i = 0; i < list.Count; i++)
        {
            if (list[i].Placed)
            {
                if (list[i].Type == BubbleType.Disappear)
                {
                    RemainedDisappearBubble++;
                    EventManager.instance.Fire(new BubbleNumSet(BubbleType.Disappear, RemainedDisappearBubble));
                }
                else
                {
                    RollBackTask.Add(new DisappearTask(list[i].Bubble, RollBackTime, list[i].To, Map, list[i].Type, true));
                }
            }
            else
            {
                if (list[i].From.x != list[i].To.x || list[i].From.y != list[i].To.y)
                {
                    SerialTasks serialTasks = new SerialTasks();
                    serialTasks.Add(new DisappearTask(list[i].Bubble, RollBackTime, list[i].To, Map, list[i].Type, false));
                    serialTasks.Add(new TransformTask(list[i].Bubble, list[i].EndPos, list[i].BeginPos, 0));
                    serialTasks.Add(new AppearTask(list[i].Bubble, RollBackTime, list[i].From, Map, list[i].Type));
                    RollBackTask.Add(serialTasks);
                }
            }
        }


        while (!RollBackTask.IsFinished)
        {
            RollBackTask.Update();
            yield return null;
        }

        ChangeInfoList.Remove(list);
        GameManager.State = GameState.Play;
    }
}
