using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static int RemainedDisappearBubble;
    public static int RemainedNormalBubble;
    public static GameObject TeleportAura;

    public int LevelIndex;
    public int DisappearBubbleInitNum;
    public int NormalBubbleInitNum;

    public GameObject AllSlot;
    public GameObject AllBubble;
    public float MotionInterval;
    public float TeleportWaitTime;
    public float TeleportMotionInterval;
    public float RoundEndInterval;
    public float RollBackTime;
    public float MapAppearWaitTime;
    public float MapAppearTime;
    public float MapUnitAppearTime;


    public float TeleportAuraGenerationTime;
    public float TeleportAuraDisappearTime;
    public float BeforeAuraDisappearWaitTime;
    public float AfterAuraDisappearWaitTime;
    public float AuraSize;

    private List<List<SlotInfo>> Map;
    private Vector2 PivotOffset;
    private SlotInfo TeleportSlot1, TeleportSlot2;

    private List<List<GameObject>> MapAppearSequence=new List<List<GameObject>>();
    private SerialTasks MapAppearTask = new SerialTasks();

    private SerialTasks BubbleMotionTasks = new SerialTasks();

    private List<List<BubbleChangeInfo>> ChangeInfoList = new List<List<BubbleChangeInfo>>();

    private Dictionary<GameObject, Vector2Int> OriginBubblePosDic = new Dictionary<GameObject, Vector2Int>();
    private Dictionary<GameObject, Vector2Int> ChangedBubblePosDic = new Dictionary<GameObject, Vector2Int>();

    private Vector3 SlotScale = Vector3.one;
    private const float DropMoveTime = 0.1f;
    

    private void OnEnable()
    {
        CursorManager.AllSlot = AllSlot;
        if (Map == null)
        {
            Map = new List<List<SlotInfo>>();
            GetMapInfo();
            SetMapAppearSequence();
            SetMapAppearTask();
        }

        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.AddHandler<LevelLoaded>(OnLevelLoaded);

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
        if (GameManager.State == GameState.Init)
        {
            MapAppearTask.Update();
            if (MapAppearTask.State == Task.TaskState.Success)
            {
                GameManager.State = GameState.Play;
            }
        }

        BubbleMotionTasks.Update();

        if(GameManager.State==GameState.Play && Input.GetMouseButtonDown(1) && ChangeInfoList.Count > 0)
        {
            StartCoroutine(RollBack());
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
            Map[x][y] = new SlotInfo(new Vector2Int(x,y), child.GetComponent<SlotObject>().Type, BubbleType.Null, BubbleState.Stable, null, child.localPosition, child.gameObject);
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

    private void SetMapAppearSequence()
    {
        List<SlotInfo> AllSlotInfo = new List<SlotInfo>();
        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                if (Map[i][j] != null)
                {
                    AllSlotInfo.Add(Map[i][j]);
                }
            }
        }

        int num = AllSlotInfo.Count;
        for(int i = 0; i < num; i++)
        {
            List<GameObject> AppearUnit = new List<GameObject>();
            int index = Random.Range(0, AllSlotInfo.Count);
            if (AllSlotInfo[index].ConnectedBubble != null)
            {
                AppearUnit.Add(AllSlotInfo[index].ConnectedBubble);
                Color BubbleColor = AllSlotInfo[index].ConnectedBubble.GetComponent<SpriteRenderer>().color;
                AllSlotInfo[index].ConnectedBubble.GetComponent<SpriteRenderer>().color = new Color(BubbleColor.r, BubbleColor.g, BubbleColor.b, 0);
            }
            AppearUnit.Add(AllSlotInfo[index].Entity);
            Color SlotColor = AllSlotInfo[index].Entity.GetComponent<SpriteRenderer>().color;
            AllSlotInfo[index].Entity.GetComponent<SpriteRenderer>().color = new Color(SlotColor.r, SlotColor.g, SlotColor.b, 0);

            AllSlotInfo.RemoveAt(index);
            MapAppearSequence.Add(AppearUnit);
        }

    }

    private void SetMapAppearTask()
    {
        GameManager.State = GameState.Init;

        ParallelTasks MapUnitAppearTask = new ParallelTasks();

        float TimeUnit = MapAppearTime / MapAppearSequence.Count;

        for(int i = 0; i < MapAppearSequence.Count; i++)
        {
            SerialTasks UnitAppear = new SerialTasks();
            UnitAppear.Add(new WaitTask(i * TimeUnit));

            ParallelTasks UnitActualAppear = new ParallelTasks();
            for(int j = 0; j < MapAppearSequence[i].Count; j++)
            {
                UnitActualAppear.Add(new AppearTask(MapAppearSequence[i][j], MapUnitAppearTime, false, Vector2Int.zero));
            }

            UnitAppear.Add(UnitActualAppear);
            MapUnitAppearTask.Add(UnitAppear);
        }

        MapAppearTask.Add(MapUnitAppearTask);

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

        bool TeleportAffected = false;

        if (Map[Pos.x][Pos.y] == TeleportSlot1)
        {
            if (TeleportSlot2.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = Map[Pos.x][Pos.y].ConnectedBubble;
                Teleport(Bubble, TeleportSlot2,null);
                Pos = TeleportSlot2.Pos;
                TeleportAffected = true;
            }
            else
            {
                TeleportSlotShake();
            }
        }
        else if (Map[Pos.x][Pos.y] == TeleportSlot2)
        {
            if (TeleportSlot1.InsideBubbleType == BubbleType.Null)
            {
                GameObject Bubble = Map[Pos.x][Pos.y].ConnectedBubble;
                Teleport(Bubble, TeleportSlot1,null);
                Pos = TeleportSlot1.Pos;
                TeleportAffected = true;
            }
            else
            {
                TeleportSlotShake();
            }
        }

        ChangeInfoList[ChangeInfoList.Count - 1].Add(new BubbleChangeInfo(Map[Pos.x][Pos.y].ConnectedBubble, Type, true, Pos, Pos, Map[Pos.x][Pos.y].Location, Map[Pos.x][Pos.y].Location));

        List<Vector2Int> PosList = new List<Vector2Int>();
        PosList.Add(Pos);

        BubbleInflate(PosList, true,TeleportAffected);

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

    private void BubbleInflate(List<Vector2Int> PosList, bool Drop,bool TeleportAffected)
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
                            BubbleRecoverTasks.Add(new RecoverTask(Bubble, Data.RecoverTime, Data.ExhaustScale, Data.NormalScale, Bubble.GetComponent<Bubble>().ExhaustColor, Bubble.GetComponent<Bubble>().NormalColor, Map, new Vector2Int(i, j),Data.DefaultEnergyColor));
                        }
                    }
                }
            }
            BubbleMotionTasks.Add(BubbleRecoverTasks);
            BubbleMotionTasks.Add(new MotionFinishTask());
            return;
        }

        List<Vector2Int> NewPosList = new List<Vector2Int>();
        Dictionary<GameObject, Vector2Int> ActivateDic = new Dictionary<GameObject, Vector2Int>();

        SetBubbleMovement(PosList, ActivateDic, TeleportAffected);
        
        foreach(KeyValuePair<GameObject,Vector2Int> entry in ActivateDic)
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

        BubbleInflate(NewPosList, false, false);

    }

    private bool AvailableForPush(SlotInfo S)
    {
        return S != null && S.InsideBubbleType != BubbleType.Null && (S.InsideBubbleState == BubbleState.Activated || S.InsideBubbleState == BubbleState.Stable);
    }

    private void SetBubbleMovement(List<Vector2Int> PosList, Dictionary<GameObject, Vector2Int> ActivateDic, bool TeleportAffected)
    {
        if (PosList.Count == 0)
        {
            if (TeleportAffected)
            {
                BubbleMotionTasks.Add(new WaitTask(BeforeAuraDisappearWaitTime));
                BubbleMotionTasks.Add(new TeleportAuraDisappearTask(TeleportAuraDisappearTime, AuraSize));

                if (ActivateDic.Count > 0)
                {
                    BubbleMotionTasks.Add(new WaitTask(AfterAuraDisappearWaitTime));
                }
            }
            return;
        }

        if (TeleportAffected)
        {
            BubbleMotionTasks.Add(new WaitTask(TeleportMotionInterval));
        }

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

            BubbleInflateMoveBlocked.Add(new ReleaseTask(Bubble, Data.MotionTime, Data.NormalScale, Data.ExhaustScale, Bubble.GetComponent<Bubble>().NormalColor, Bubble.GetComponent<Bubble>().ExhaustColor, Map, PosList[i]));

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

        PosList.Clear();

        bool Slot1Active = false;
        bool Slot2Active = false;

        Dictionary<GameObject, Vector2Int> AffectedBubblePosDic = new Dictionary<GameObject, Vector2Int>();


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

            Vector2Int Target = Moves[k].TargetPos;

            if (x < 0 || y < 0 || x >= Map.Count || y >= Map[x].Count || Map[x][y] == null || Map[x][y].InsideBubbleType != BubbleType.Null)
            {

                ParallelTasks BubbleBlocked = new ParallelTasks();

                if (TeleportAffected)
                {
                    BubbleBlocked.Add(new AffectTask(Bubble, Data.AffectedEnergyColor));
                }
                BubbleBlocked.Add(GetEnergyFillTask(Bubble,TeleportAffected));
                BubbleBlocked.Add(new BlockedTask(Bubble, Moves[k].CurrentLocation, dir, Data.BlockedDis, Data.MotionTime, Moves[k].CurrentPos, Map));

                BubbleInflateMoveBlocked.Add(BubbleBlocked);

                Target = Moves[k].CurrentPos;
            }
            else
            {
                if (PosTarget[x][y] > 1)
                {
                    if (ActivateDic.ContainsKey(Bubble))
                    {
                        ActivateDic[Bubble] = Moves[k].CurrentPos;
                    }
                    else
                    {
                        ActivateDic.Add(Bubble, Moves[k].CurrentPos);
                    }

                    ParallelTasks BubbleBlocked = new ParallelTasks();

                    if (TeleportAffected)
                    {
                        BubbleBlocked.Add(new AffectTask(Bubble, Data.AffectedEnergyColor));
                    }
                    BubbleBlocked.Add(GetEnergyFillTask(Bubble,TeleportAffected));
                    BubbleBlocked.Add(new BlockedTask(Bubble, Moves[k].CurrentLocation, dir, Data.ConflictBlockedDis, Data.MotionTime, Moves[k].CurrentPos, Map));

                    BubbleInflateMoveBlocked.Add(BubbleBlocked);

                    Target = Moves[k].CurrentPos;
                }
                else
                {
                    if (ActivateDic.ContainsKey(Bubble))
                    {
                        ActivateDic[Bubble] = Moves[k].TargetPos;
                    }
                    else
                    {
                        ActivateDic.Add(Bubble, Moves[k].TargetPos);
                    }

                    ParallelTasks BubbleMove = new ParallelTasks();

                    if (TeleportAffected)
                    {
                        BubbleMove.Add(new AffectTask(Bubble, Data.AffectedEnergyColor));
                    }
                    BubbleMove.Add(GetEnergyFillTask(Bubble,TeleportAffected));
                    BubbleMove.Add(new MoveTask(Bubble, Moves[k].CurrentLocation, Moves[k].TargetLocation, Data.MotionTime, Moves[k].CurrentPos, Moves[k].TargetPos, Bubble.GetComponent<Bubble>().Type, Map));

                    BubbleInflateMoveBlocked.Add(BubbleMove);

                    ChangedBubblePosDic[Bubble] = Moves[k].TargetPos;
                }
            }

            if (AffectedBubblePosDic.ContainsKey(Bubble))
            {
                AffectedBubblePosDic[Bubble] = Target;
            }
            else
            {
                AffectedBubblePosDic.Add(Bubble, Target);
            }

            if (TeleportAffected)
            {
                if (ActivateDic.ContainsKey(Bubble))
                {
                    ActivateDic.Remove(Bubble);
                }
            }
            else
            {
                if (ActivateDic.ContainsKey(Bubble))
                {
                    ActivateDic[Bubble] = Target;
                }
                else
                {
                    ActivateDic.Add(Bubble, Target);
                }
            }

            Vector2Int v = AffectedBubblePosDic[Bubble];

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

        bool HaveTeleport=TeleportAffected;

        if (Slot1Active && !Slot2Active || !Slot1Active && Slot2Active)
        {
            if(Slot1Active)
            {
                if (TeleportSlot2.InsideBubbleType == BubbleType.Null)
                {
                    GameObject Bubble = TeleportSlot1.ConnectedBubble;

                    if (ActivateDic.ContainsKey(Bubble))
                    {
                        ActivateDic.Remove(Bubble);
                    }

                    AffectedBubblePosDic.Remove(Bubble);
                    PosList.Add(TeleportSlot2.Pos);
                    
                    ChangedBubblePosDic[Bubble] = TeleportSlot2.Pos;

                    Teleport(TeleportSlot1.ConnectedBubble, TeleportSlot2, BubbleInflateMoveBlocked);

                    HaveTeleport = true;
                }
                else
                {
                    TeleportSlotShake();
                }
            }
            else if(Slot2Active)
            {
                if (TeleportSlot1.InsideBubbleType == BubbleType.Null)
                {

                    GameObject Bubble = TeleportSlot2.ConnectedBubble;

                    if (ActivateDic.ContainsKey(Bubble))
                    {
                        ActivateDic.Remove(Bubble);
                    }

                    AffectedBubblePosDic.Remove(Bubble);
                    PosList.Add(TeleportSlot1.Pos);

                    ChangedBubblePosDic[Bubble] = TeleportSlot1.Pos;

                    Teleport(TeleportSlot2.ConnectedBubble, TeleportSlot1, BubbleInflateMoveBlocked);

                    HaveTeleport = true;
                }
                else
                {
                    TeleportSlotShake();
                }
            }
        }
        else if (Slot1Active && Slot2Active)
        {
            TeleportSlotShake();
        }

        if (TeleportAffected)
        {
            foreach(KeyValuePair<GameObject, Vector2Int> entry in AffectedBubblePosDic)
            {
                PosList.Add(entry.Value);
            }
            
        }

        SetBubbleMovement(PosList, ActivateDic, HaveTeleport);


    }

    private void TeleportSlotShake()
    {
        var Data = GetComponent<BubbleMotionData>();

        ParallelTasks ShakeTasks = new ParallelTasks();
        ShakeTasks.Add(new ShakeTask(TeleportSlot1.Entity, Data.TeleportSlotShakeDis, Data.TeleportSlotShakeTime, Data.TeleportSlotShakeCycle));
        SerialTasks Slot1ColorChange = new SerialTasks();
        Slot1ColorChange.Add(new ColorChangeTask(TeleportSlot1.Entity, TeleportSlot1.Entity.GetComponent<SlotObject>().DefaultColor, TeleportSlot1.Entity.GetComponent<SlotObject>().SelectedColor, Data.TeleportSlotShakeTime / 2));
        Slot1ColorChange.Add(new ColorChangeTask(TeleportSlot1.Entity, TeleportSlot1.Entity.GetComponent<SlotObject>().SelectedColor, TeleportSlot1.Entity.GetComponent<SlotObject>().DefaultColor, Data.TeleportSlotShakeTime / 2));
        ShakeTasks.Add(Slot1ColorChange);

        ShakeTasks.Add(new ShakeTask(TeleportSlot2.Entity, Data.TeleportSlotShakeDis, Data.TeleportSlotShakeTime, Data.TeleportSlotShakeCycle));
        SerialTasks Slot2ColorChange = new SerialTasks();
        Slot2ColorChange.Add(new ColorChangeTask(TeleportSlot2.Entity, TeleportSlot2.Entity.GetComponent<SlotObject>().DefaultColor, TeleportSlot2.Entity.GetComponent<SlotObject>().SelectedColor, Data.TeleportSlotShakeTime / 2));
        Slot2ColorChange.Add(new ColorChangeTask(TeleportSlot2.Entity, TeleportSlot2.Entity.GetComponent<SlotObject>().SelectedColor, TeleportSlot2.Entity.GetComponent<SlotObject>().DefaultColor, Data.TeleportSlotShakeTime / 2));
        ShakeTasks.Add(Slot1ColorChange);

        BubbleMotionTasks.Add(ShakeTasks);
    }

    private void Teleport(GameObject Obj, SlotInfo Target, ParallelTasks BubbleMovementTask)
    {
        var Data = GetComponent<BubbleMotionData>();


        ParallelTasks TeleportTask = new ParallelTasks();
        SerialTasks BubbleTeleportTask = new SerialTasks();
        SerialTasks TeleportAuraTask = new SerialTasks();

        if (Target == TeleportSlot1)
        {
            if (BubbleMovementTask != null)
            {
                BubbleTeleportTask.Add(new WaitTask(Data.MotionTime));
                TeleportAuraTask.Add(new WaitTask(Data.MotionTime));
            }
            BubbleTeleportTask.Add(new AffectTask(Obj, Data.AffectedEnergyColor));
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, Data.NormalScale, 0, Data.TeleportTime/2));
            BubbleTeleportTask.Add(new MoveTask(Obj, TeleportSlot2.Location, TeleportSlot1.Location, 0, TeleportSlot2.Pos, TeleportSlot1.Pos, TeleportSlot2.InsideBubbleType, Map));
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, 0, Data.NormalScale, Data.TeleportTime/2));

            TeleportAuraTask.Add(new WaitTask(Data.TeleportTime));
            TeleportAuraTask.Add(new TeleportAuraGenerationTask(TeleportSlot1.Location, TeleportAuraGenerationTime));
        }
        else
        {
            if (BubbleMovementTask != null)
            {
                BubbleTeleportTask.Add(new WaitTask(Data.MotionTime));
                TeleportAuraTask.Add(new WaitTask(Data.MotionTime));
            }
            BubbleTeleportTask.Add(new AffectTask(Obj, Data.AffectedEnergyColor));
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, Data.NormalScale, 0, Data.TeleportTime/2));
            BubbleTeleportTask.Add(new MoveTask(Obj, TeleportSlot1.Location, TeleportSlot2.Location, 0, TeleportSlot1.Pos, TeleportSlot2.Pos, TeleportSlot1.InsideBubbleType, Map));
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, 0, Data.NormalScale, Data.TeleportTime/2));


            TeleportAuraTask.Add(new WaitTask(Data.TeleportTime));
            TeleportAuraTask.Add(new TeleportAuraGenerationTask(TeleportSlot2.Location, TeleportAuraGenerationTime));
        }

        bool DuringBubbleMovement = BubbleMovementTask != null;

        TeleportTask.Add(BubbleTeleportTask);
        TeleportTask.Add(TeleportAuraTask);

        TeleportTask.Add(GetTeleportSlotTask(TeleportSlot1.Entity, DuringBubbleMovement));
        TeleportTask.Add(GetTeleportSlotTask(TeleportSlot2.Entity, DuringBubbleMovement));



        if (BubbleMovementTask == null)
        {
            BubbleMotionTasks.Add(TeleportTask);
            BubbleMotionTasks.Add(new WaitTask(TeleportWaitTime));
        }
        else
        {
            BubbleMovementTask.Add(TeleportTask);
            BubbleMotionTasks.Add(new WaitTask(TeleportWaitTime));
        }
    }

    private SerialTasks GetTeleportSlotTask(GameObject Obj,bool DuringBubbleMovement)
    {
        var Data = GetComponent<BubbleMotionData>();
        SerialTasks TeleportSlotTask = new SerialTasks();

        if (DuringBubbleMovement)
        {
            TeleportSlotTask.Add(new WaitTask(Data.MotionTime));
        }

        ParallelTasks ScaleRotationChangeFirstHalf = new ParallelTasks();
        ScaleRotationChangeFirstHalf.Add(new RotationTask(Obj, 90, Data.TeleportTime / 2));
        ScaleRotationChangeFirstHalf.Add(new ColorChangeTask(Obj, Obj.GetComponent<SlotObject>().DefaultColor, Obj.GetComponent<SlotObject>().SelectedColor, 0));
        
        TeleportSlotTask.Add(ScaleRotationChangeFirstHalf);

        ParallelTasks ScaleRotationChangeSecondHalf = new ParallelTasks();
        ScaleRotationChangeSecondHalf.Add(new RotationTask(Obj, 90, Data.TeleportTime / 2));
        ScaleRotationChangeSecondHalf.Add(new ColorChangeTask(Obj, Obj.GetComponent<SlotObject>().SelectedColor, Obj.GetComponent<SlotObject>().DefaultColor, 0));
        TeleportSlotTask.Add(ScaleRotationChangeSecondHalf);

        return TeleportSlotTask;
    }

    private SerialTasks GetEnergyFillTask(GameObject Obj,bool TeleportAffected)
    {
        var Data = GetComponent<BubbleMotionData>();

        Color TargetColor = Data.DefaultEnergyColor;
        if (TeleportAffected)
        {
            TargetColor = Data.AffectedEnergyColor;
        }

        SerialTasks EnergyFillTask = new SerialTasks();
        EnergyFillTask.Add(new ColorChangeTask(Obj, Obj.GetComponent<Bubble>().NormalColor, TargetColor, Data.MotionTime / 2));
        EnergyFillTask.Add(new ColorChangeTask(Obj, TargetColor, Obj.GetComponent<Bubble>().NormalColor,  Data.MotionTime / 2));

        return EnergyFillTask;
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
                        return;
                    }
                }
            }
        }

        EventManager.instance.Fire(new LevelFinish(LevelIndex));
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
                    RollBackTask.Add(new DisappearTask(list[i].Bubble, RollBackTime / 2, list[i].To, Map, list[i].Type, true));
                }
                EventManager.instance.Fire(new BubbleSelected(list[i].Type));
                GameManager.HeldBubbleType = list[i].Type;
            }
            else
            {
                if (list[i].From.x != list[i].To.x || list[i].From.y != list[i].To.y)
                {
                    SerialTasks serialTasks = new SerialTasks();
                    serialTasks.Add(new DisappearTask(list[i].Bubble, RollBackTime / 2, list[i].To, Map, list[i].Type, false));
                    serialTasks.Add(new TransformTask(list[i].Bubble, list[i].EndPos, list[i].BeginPos, 0));
                    serialTasks.Add(new AppearTask(list[i].Bubble, RollBackTime / 2, true, list[i].From, Map, list[i].Type));
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
