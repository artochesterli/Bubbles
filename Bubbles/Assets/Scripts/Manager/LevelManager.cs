using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BorderSlotType
{
    Edge,
    Corner,
    Stem,
    Spine,
}

public class LevelManager : MonoBehaviour
{
    public static int RemainedDisappearBubble;
    public static int RemainedNormalBubble;

    public GameObject DisappearBubblePrefab;
    public GameObject NormalBubblePrefab;

    public int LevelIndex;
    public int DisappearBubbleInitNum;
    public int NormalBubbleInitNum;

    public GameObject AllSlot;
    public GameObject AllBubble;
    public float MotionInterval;

    public float RoundEndInterval;
    public float RollBackTime;

    public float PlaceScale;

    public float MapAppearUnitGapProbability;
    public Vector2 MapAppearUnitGapMinMax;
    public float MapAppearLayerInterval;
    public float MapAppearBubbleWaitTime;
    public float MapSlotInitPosOffsetFactor;


    public float MapUnitAppearTime;

    public float FinishWaitTime;


    private List<List<SlotInfo>> Map;
    private List<SlotInfo> TemporarySlot = new List<SlotInfo>(); 
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
        UsableCircle.AllSlot = AllSlot;

        CursorManager.AllSlot = AllSlot;
        if (Map == null)
        {
            Map = new List<List<SlotInfo>>();
            GetMapInfo();
        }

        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.AddHandler<LevelLoaded>(OnLevelLoaded);
        EventManager.instance.AddHandler<RollBack>(OnRollBack);

    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<LevelLoaded>(OnLevelLoaded);
        EventManager.instance.RemoveHandler<RollBack>(OnRollBack);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<LevelLoaded>(OnLevelLoaded);
        EventManager.instance.RemoveHandler<RollBack>(OnRollBack);
    }

    void Update()
    {

        if (GameManager.levelState == LevelState.Executing)
        {
            BubbleMotionTasks.Update();
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
        Vector2Int Size = new Vector2Int(Mathf.RoundToInt(MaxX - MinX) + 1, Mathf.RoundToInt(MaxY - MinY) + 1);

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

    public SerialTasks GetMapAppearTask()
    {
        MapAppearTask = new SerialTasks();

        List<SlotInfo> AllSlotInfo = new List<SlotInfo>();
        List<int> AllSlotLayer = new List<int>();
        List<SlotInfo> AllSlotWithBubble = new List<SlotInfo>();

        Vector2 MapCenter = new Vector2((Map.Count-1) / 2.0f, (Map[0].Count-1) / 2.0f);

        for(int i = 0; i < Map.Count; i++)
        {
            for(int j = 0; j < Map[i].Count; j++)
            {
                if (Map[i][j] != null)
                {
                    Color SlotColor = Map[i][j].Entity.GetComponent<SpriteRenderer>().color;
                    Map[i][j].Entity.GetComponent<SpriteRenderer>().color = new Color(SlotColor.r, SlotColor.g, SlotColor.b, 0);
                    Map[i][j].Entity.transform.localPosition = Map[i][j].Location * MapSlotInitPosOffsetFactor;
                    if (Map[i][j].InsideBubbleType != BubbleType.Null)
                    {
                        AllSlotWithBubble.Add(Map[i][j]);
                        Color BubbleColor = Map[i][j].ConnectedBubble.GetComponent<SpriteRenderer>().color;
                        Map[i][j].ConnectedBubble.GetComponent<SpriteRenderer>().color = new Color(BubbleColor.r, BubbleColor.g, BubbleColor.b, 0);
                    }

                    AllSlotInfo.Add(Map[i][j]);

                    int XLayer;
                    int YLayer;

                    if (Map.Count % 2 == 1)
                    {
                        XLayer = Mathf.RoundToInt(Mathf.Abs(i - MapCenter.x));
                    }
                    else
                    {
                        XLayer = Mathf.RoundToInt(Mathf.Abs(i  - MapCenter.x)+0.5f);
                        
                    }

                    if (Map[i].Count%2 == 1)
                    {
                        YLayer = Mathf.RoundToInt(Mathf.Abs(j - MapCenter.y));
                    }
                    else
                    {
                        YLayer = Mathf.RoundToInt(Mathf.Abs(j  - MapCenter.y) + 0.5f);
                    }

                    AllSlotLayer.Add(Mathf.Max(XLayer, YLayer));
                }
            }
        }

        List<List<SlotInfo>> SortedSlotInfo = new List<List<SlotInfo>>();
        List<List<int>> SortedSlotLayer = new List<List<int>>();

        for (int i = 0; i < AllSlotLayer.Count; i++)
        {
            for(int j = 0; j < AllSlotLayer.Count - i - 1; j++)
            {
                if (AllSlotLayer[j] < AllSlotLayer[j + 1])
                {
                    int temp = AllSlotLayer[j];
                    AllSlotLayer[j] = AllSlotLayer[j + 1];
                    AllSlotLayer[j + 1] = temp;

                    SlotInfo S = AllSlotInfo[j];
                    AllSlotInfo[j] = AllSlotInfo[j + 1];
                    AllSlotInfo[j + 1] = S;
                }
            }

            if(SortedSlotLayer.Count==0 || SortedSlotLayer[SortedSlotLayer.Count-1][SortedSlotLayer[SortedSlotLayer.Count - 1].Count - 1] < AllSlotLayer[AllSlotLayer.Count - i - 1])
            {
                SortedSlotLayer.Add(new List<int>());
                SortedSlotLayer[SortedSlotLayer.Count - 1].Add(AllSlotLayer[AllSlotLayer.Count - i - 1]);
                SortedSlotInfo.Add(new List<SlotInfo>());
                SortedSlotInfo[SortedSlotInfo.Count - 1].Add(AllSlotInfo[AllSlotInfo.Count - i- 1]);
            }
            else
            {
                SortedSlotLayer[SortedSlotLayer.Count - 1].Add(AllSlotLayer[AllSlotLayer.Count - i - 1]);
                SortedSlotInfo[SortedSlotInfo.Count - 1].Add(AllSlotInfo[AllSlotInfo.Count -i - 1]);
            }

        }

        float Delay = 0;
        ParallelTasks AllSlotAppear = new ParallelTasks();

        for (int i = 0; i < SortedSlotInfo.Count; i++)
        {
            int num = SortedSlotInfo[i].Count;

            List<SlotInfo> RearrangedSlotInfo = new List<SlotInfo>();
            List<int> RearrangedLayer = new List<int>();

            for (int j = 0; j < num; j++)
            {
                int index = Random.Range(0, SortedSlotInfo[i].Count);
                RearrangedSlotInfo.Add(SortedSlotInfo[i][index]);
                RearrangedLayer.Add(SortedSlotLayer[i][index]);
                SortedSlotInfo[i].RemoveAt(index);
                SortedSlotLayer[i].RemoveAt(index);
            }

            SortedSlotInfo[i].Clear();
            SortedSlotLayer[i].Clear();

            SortedSlotInfo[i] = RearrangedSlotInfo;
            SortedSlotLayer[i] = RearrangedLayer;

            ParallelTasks LayerSlotTasks = new ParallelTasks();


            for (int j = 0; j < SortedSlotInfo[i].Count; j++)
            {

                ParallelTasks SlotTask = new ParallelTasks();
                SlotTask.Add(new AppearTask(SortedSlotInfo[i][j].Entity, MapUnitAppearTime, false, SortedSlotInfo[i][j].Pos));
                SlotTask.Add(new TransformTask(SortedSlotInfo[i][j].Entity, SortedSlotInfo[i][j].Location * MapSlotInitPosOffsetFactor, SortedSlotInfo[i][j].Location, MapUnitAppearTime));

                if (j > 0)
                {
                    if (Random.Range(0.0f, 1.0f)<MapAppearUnitGapProbability)
                    {
                        Delay += Random.Range(MapAppearUnitGapMinMax.x, MapAppearUnitGapMinMax.y);
                    }
                }

                SerialTasks temp = new SerialTasks();
                temp.Add(new WaitTask(Delay));
                temp.Add(SlotTask);

                LayerSlotTasks.Add(temp);
            }

            AllSlotAppear.Add(LayerSlotTasks);

        }


        ParallelTasks AllBubbleAppear = new ParallelTasks();

        for(int i = 0; i < AllSlotWithBubble.Count; i++)
        {
            AllBubbleAppear.Add(new AppearTask(AllSlotWithBubble[i].ConnectedBubble, MapUnitAppearTime, false, AllSlotWithBubble[i].Pos));
        }

        MapAppearTask.Add(AllSlotAppear);
        MapAppearTask.Add(AllBubbleAppear);

        return MapAppearTask;

    }

    public SerialTasks GetMapDisappearTask()
    {
        ParallelTasks SlotDisappearTasks = new ParallelTasks();

        ParallelTasks BubbleDisappearTasks = new ParallelTasks();

        foreach(Transform child in AllBubble.transform)
        {
            Color color = child.GetComponent<SpriteRenderer>().color;
            BubbleDisappearTasks.Add(new ColorChangeTask(child.gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), MapUnitAppearTime));
        }

        foreach(Transform child in AllSlot.transform)
        {
            Color color = child.GetComponent<SpriteRenderer>().color;
            SlotDisappearTasks.Add(new ColorChangeTask(child.gameObject, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), MapUnitAppearTime));
            SlotInfo Info = child.GetComponent<SlotObject>().ConnectedSlotInfo;
            SlotDisappearTasks.Add(new TransformTask(child.gameObject, Info.Location, Info.Location * MapSlotInitPosOffsetFactor, MapUnitAppearTime));
        }

        return new SerialTasks();
    }

    private void PlaceBubble(GameObject UseableBubble, Vector2Int Pos, BubbleType Type)
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

        Vector3 GeneratePos = UseableBubble.transform.position;

        switch (Type)
        {
            case BubbleType.Disappear:
                Map[Pos.x][Pos.y].ConnectedBubble = (GameObject)Instantiate(DisappearBubblePrefab, GeneratePos, Quaternion.Euler(0, 0, 0));
                break;
            case BubbleType.Normal:
                Map[Pos.x][Pos.y].ConnectedBubble = (GameObject)Instantiate(NormalBubblePrefab, GeneratePos, Quaternion.Euler(0, 0, 0));
                break;
        }


        Map[Pos.x][Pos.y].InsideBubbleType = Type;
        Map[Pos.x][Pos.y].InsideBubbleState = BubbleState.Activated;

        Map[Pos.x][Pos.y].ConnectedBubble.GetComponent<Bubble>().State = BubbleState.Activated;
        Map[Pos.x][Pos.y].ConnectedBubble.transform.Find("StableEffect").GetComponent<ParticleSystem>().Stop();
        Map[Pos.x][Pos.y].ConnectedBubble.transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Play();
        Map[Pos.x][Pos.y].ConnectedBubble.transform.parent = AllBubble.transform;

        Map[Pos.x][Pos.y].ConnectedBubble.transform.localScale = Vector3.one * PlaceScale;

        if(Map[Pos.x][Pos.y].slotType == SlotType.Target && Type == BubbleType.Normal)
        {
            Map[Pos.x][Pos.y].Entity.GetComponent<TargetSlotObject>().SetBubbleInside(true);
            Map[Pos.x][Pos.y].Entity.GetComponent<TargetSlotObject>().ClearInsideParticleInfo();
        }

        var Data = GetComponent<BubbleMotionData>();

        Vector3 Start = Map[Pos.x][Pos.y].ConnectedBubble.transform.localPosition;
        Vector3 End = Map[Pos.x][Pos.y].Location;

        ParallelTasks PlaceTask = new ParallelTasks();
        PlaceTask.Add(new TransformTask(Map[Pos.x][Pos.y].ConnectedBubble, Start, End, DropMoveTime));
        PlaceTask.Add(new ScaleChangeTask(Map[Pos.x][Pos.y].ConnectedBubble, PlaceScale, GetComponent<BubbleMotionData>().NormalScale, DropMoveTime));
        BubbleMotionTasks.Add(PlaceTask);

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
                TeleportSlotBlocked();
                BubbleMotionTasks.Add(new WaitTask(MotionInterval));
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
                TeleportSlotBlocked();
                BubbleMotionTasks.Add(new WaitTask(MotionInterval));
            }
        }

        ChangeInfoList[ChangeInfoList.Count - 1].Add(new BubbleChangeInfo(Map[Pos.x][Pos.y].ConnectedBubble, Type, true, UseableBubble, Pos, Pos, Map[Pos.x][Pos.y].Location, Map[Pos.x][Pos.y].Location));

        List<Vector2Int> PosList = new List<Vector2Int>();
        if (!TeleportAffected)
        {
            PosList.Add(Pos);
        }
        else
        {
            BubbleMotionTasks.Add(new WaitTask(RoundEndInterval));
        }


        BubbleInflate(PosList, true, TeleportAffected);

        foreach(GameObject Bubble in OriginBubblePosDic.Keys)
        {
            Vector2Int From = OriginBubblePosDic[Bubble];
            Vector2Int To = ChangedBubblePosDic[Bubble];
            Vector3 BeginPos = Map[From.x][From.y].Location;
            Vector3 EndPos = Map[To.x][To.y].Location;
            ChangeInfoList[ChangeInfoList.Count - 1].Add(new BubbleChangeInfo(Bubble,Bubble.GetComponent<Bubble>().Type, false, null, From, To, BeginPos, EndPos));
        }
    }

    private ParallelTasks GetRecoverTasks()
    {
        ParallelTasks BubbleRecoverTasks = new ParallelTasks();
        for (int i = 0; i < Map.Count; i++)
        {
            for (int j = 0; j < Map[i].Count; j++)
            {
                if (Map[i][j] != null && Map[i][j].InsideBubbleType != BubbleType.Null && Map[i][j].InsideBubbleState != BubbleState.Stable)
                {
                    GameObject Bubble = Map[i][j].ConnectedBubble;
                    var Data = GetComponent<BubbleMotionData>();


                    switch (Map[i][j].InsideBubbleType)
                    {
                        case BubbleType.Disappear:
                            BubbleRecoverTasks.Add(new DisappearTask(Bubble, Data.RecoverTime, new Vector2Int(i, j), Map, BubbleType.Disappear, false));
                            Map[i][j].ConnectedBubble = null;
                            Map[i][j].InsideBubbleType = BubbleType.Null;
                            Map[i][j].InsideBubbleState = BubbleState.Stable;
                            break;
                        case BubbleType.Normal:
                            if (Map[i][j].InsideBubbleState == BubbleState.Exhausted)
                            {
                                BubbleRecoverTasks.Add(new RecoverTask(Bubble, Data.RecoverTime, Data.ExhaustScale, Data.NormalScale, Bubble.GetComponent<Bubble>().ExhaustColor, Bubble.GetComponent<Bubble>().NormalColor, Map, new Vector2Int(i, j), Data.DefaultEnergyColor));
                            }
                            else
                            {
                                BubbleRecoverTasks.Add(new RecoverTask(Bubble, 0, Data.NormalScale, Data.NormalScale, Bubble.GetComponent<Bubble>().NormalColor, Bubble.GetComponent<Bubble>().NormalColor, Map, new Vector2Int(i, j), Data.DefaultEnergyColor));
                            }
                            break;
                    }
                }
            }
        }
        return BubbleRecoverTasks;
    }

    private void BubbleInflate(List<Vector2Int> PosList, bool Drop,bool TeleportAffected)
    {
        if (PosList.Count == 0)
        {
            BubbleMotionTasks.Add(GetRecoverTasks());
            BubbleMotionTasks.Add(new MotionFinishTask());
            return;
        }

        List<Vector2Int> NewPosList = new List<Vector2Int>();
        Dictionary<GameObject, Vector2Int> ActivateDic = new Dictionary<GameObject, Vector2Int>();

        SetBubbleMovement(PosList, ActivateDic);
        
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

    private void SetBubbleMovement(List<Vector2Int> PosList, Dictionary<GameObject, Vector2Int> ActivateDic)
    {

        if (PosList.Count == 0)
        {
            return;
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

            List<bool> ShootParticles = new List<bool>();
            for(int j = 0; j < 4; j++)
            {
                ShootParticles.Add(false);
            }

            NearByPushAvailable(PosList[i], ShootParticles, Moves);

            BubbleInflateMoveBlocked.Add(new ReleaseTask(Bubble, Data.MotionTime, Data.NormalScale, Data.ExhaustScale, Bubble.GetComponent<Bubble>().NormalColor, Bubble.GetComponent<Bubble>().ExhaustColor, Map, PosList[i], ShootParticles));
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

                BubbleBlocked.Add(GetEnergyFillTask(Bubble));
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

                    BubbleBlocked.Add(GetEnergyFillTask(Bubble));
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

                    BubbleMove.Add(GetEnergyFillTask(Bubble));
                    BubbleMove.Add(new MoveTask(Bubble, Moves[k].CurrentLocation, Moves[k].TargetLocation, Data.MotionTime, Moves[k].CurrentPos, Moves[k].TargetPos, Bubble.GetComponent<Bubble>().Type, Map));
                    if (Map[Moves[k].TargetPos.x][Moves[k].TargetPos.y].slotType == SlotType.Target)
                    {
                        BubbleMove.Add(GetTargetSlotFilledTask(Map[Moves[k].TargetPos.x][Moves[k].TargetPos.y].Entity));
                        Map[Moves[k].TargetPos.x][Moves[k].TargetPos.y].Entity.GetComponent<TargetSlotObject>().ClearInsideParticleInfo();
                        Map[Moves[k].TargetPos.x][Moves[k].TargetPos.y].Entity.GetComponent<TargetSlotObject>().SetBubbleInside(true);
                    }
                    else if(Map[Moves[k].CurrentPos.x][Moves[k].CurrentPos.y].slotType == SlotType.Target)
                    {
                        Map[Moves[k].CurrentPos.x][Moves[k].CurrentPos.y].Entity.GetComponent<TargetSlotObject>().SetBubbleInside(false);
                    }

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

            if (ActivateDic.ContainsKey(Bubble))
            {
                ActivateDic[Bubble] = Target;
            }
            else
            {
                ActivateDic.Add(Bubble, Target);
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
                }
                else
                {
                    TeleportSlotBlocked();
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
                }
                else
                {
                    TeleportSlotBlocked();
                }
            }
        }
        else if (Slot1Active && Slot2Active)
        {
            TeleportSlotBlocked();
        }


    }

    private bool NearByPushAvailable(Vector2Int Pos,List<bool> DirectionAvailable = null, List<MoveInfo> Moves=null)
    {
        bool Push = false;

        if (Pos.x < Map.Count - 1)
        {
            if (AvailableForPush(Map[Pos.x + 1][Pos.y]))
            {
                if (Moves != null)
                {
                    Moves.Add(new MoveInfo(Direction.Right, new Vector2Int(Pos.x + 1, Pos.y), Map[Pos.x + 1][Pos.y].Location));
                }
                if (DirectionAvailable!=null)
                {
                    DirectionAvailable[0] = true;
                }
                Push = true;
            }
        }

        if (Pos.x > 0)
        {
            if (AvailableForPush(Map[Pos.x - 1][Pos.y]))
            {
                if (Moves != null)
                {
                    Moves.Add(new MoveInfo(Direction.Left, new Vector2Int(Pos.x - 1, Pos.y), Map[Pos.x - 1][Pos.y].Location));
                }
                if (DirectionAvailable != null)
                {
                    DirectionAvailable[1] = true;
                }
                Push = true;
            }
        }

        if (Pos.y < Map[Pos.x].Count - 1)
        {
            if (AvailableForPush(Map[Pos.x][Pos.y + 1]))
            {
                if (Moves != null)
                {
                    Moves.Add(new MoveInfo(Direction.Up, new Vector2Int(Pos.x, Pos.y + 1), Map[Pos.x][Pos.y + 1].Location));
                }
                if (DirectionAvailable != null)
                {
                    DirectionAvailable[2] = true;
                }
                Push = true;
            }
        }

        if (Pos.y > 0)
        {
            if (AvailableForPush(Map[Pos.x][Pos.y - 1]))
            {
                if (Moves != null)
                {
                    Moves.Add(new MoveInfo(Direction.Down, new Vector2Int(Pos.x, Pos.y - 1), Map[Pos.x][Pos.y - 1].Location));
                }
                if (DirectionAvailable != null)
                {
                    DirectionAvailable[3] = true;
                }
                Push = true;
            }
        }

        return Push;
    }

    private void TeleportSlotBlocked()
    {
        var Data = GetComponent<BubbleMotionData>();

        ParallelTasks TeleportBlockedTasks = new ParallelTasks();

        SerialTasks Slot1RotateBackTasks = new SerialTasks();
        Slot1RotateBackTasks.Add(new RotationTask(TeleportSlot1.Entity, Data.TeleportSlotBlockedRotationAngle, Data.TeleportSlotBlockedRotationTime / 2));
        Slot1RotateBackTasks.Add(new RotationTask(TeleportSlot1.Entity, -Data.TeleportSlotBlockedRotationAngle, Data.TeleportSlotBlockedRotationTime / 2));


        SerialTasks Slot1ColorChange = new SerialTasks();
        Slot1ColorChange.Add(new ColorChangeTask(TeleportSlot1.Entity, TeleportSlot1.Entity.GetComponent<SlotObject>().DefaultColor, TeleportSlot1.Entity.GetComponent<SlotObject>().SelectedColor, Data.TeleportSlotBlockedRotationTime / 2));
        Slot1ColorChange.Add(new ColorChangeTask(TeleportSlot1.Entity, TeleportSlot1.Entity.GetComponent<SlotObject>().SelectedColor, TeleportSlot1.Entity.GetComponent<SlotObject>().DefaultColor, Data.TeleportSlotBlockedRotationTime / 2));

        SerialTasks Slot2RotateBackTasks = new SerialTasks();
        Slot2RotateBackTasks.Add(new RotationTask(TeleportSlot2.Entity, Data.TeleportSlotBlockedRotationAngle, Data.TeleportSlotBlockedRotationTime / 2));
        Slot2RotateBackTasks.Add(new RotationTask(TeleportSlot2.Entity, -Data.TeleportSlotBlockedRotationAngle, Data.TeleportSlotBlockedRotationTime / 2));

        SerialTasks Slot2ColorChange = new SerialTasks();
        Slot2ColorChange.Add(new ColorChangeTask(TeleportSlot2.Entity, TeleportSlot2.Entity.GetComponent<SlotObject>().DefaultColor, TeleportSlot2.Entity.GetComponent<SlotObject>().SelectedColor, Data.TeleportSlotBlockedRotationTime / 2));
        Slot2ColorChange.Add(new ColorChangeTask(TeleportSlot2.Entity, TeleportSlot2.Entity.GetComponent<SlotObject>().SelectedColor, TeleportSlot2.Entity.GetComponent<SlotObject>().DefaultColor, Data.TeleportSlotBlockedRotationTime / 2));

        TeleportBlockedTasks.Add(Slot1RotateBackTasks);
        TeleportBlockedTasks.Add(Slot2RotateBackTasks);
        TeleportBlockedTasks.Add(Slot1ColorChange);
        TeleportBlockedTasks.Add(Slot2ColorChange);

        BubbleMotionTasks.Add(TeleportBlockedTasks);
    }

    private void Teleport(GameObject Obj, SlotInfo Target, ParallelTasks BubbleMovementTask)
    {
        var Data = GetComponent<BubbleMotionData>();

        ParallelTasks TeleportTask = new ParallelTasks();
        SerialTasks BubbleTeleportTask = new SerialTasks();

        if (Target == TeleportSlot1)
        {
            if (BubbleMovementTask != null)
            {
                BubbleTeleportTask.Add(new WaitTask(Data.MotionTime));
            }
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, Data.NormalScale, 0, Data.TeleportTime/2));
            BubbleTeleportTask.Add(new MoveTask(Obj, TeleportSlot2.Location, TeleportSlot1.Location, 0, TeleportSlot2.Pos, TeleportSlot1.Pos, TeleportSlot2.InsideBubbleType, Map, true));
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, 0, Data.ExhaustScale, Data.TeleportTime / 2));
        }
        else
        {
            if (BubbleMovementTask != null)
            {
                BubbleTeleportTask.Add(new WaitTask(Data.MotionTime));
            }
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, Data.NormalScale, 0, Data.TeleportTime/2));
            BubbleTeleportTask.Add(new MoveTask(Obj, TeleportSlot1.Location, TeleportSlot2.Location, 0, TeleportSlot1.Pos, TeleportSlot2.Pos, TeleportSlot1.InsideBubbleType, Map, true));
            BubbleTeleportTask.Add(new ScaleChangeTask(Obj, 0, Data.ExhaustScale, Data.TeleportTime / 2));

        }

        bool DuringBubbleMovement = BubbleMovementTask != null;

        TeleportTask.Add(BubbleTeleportTask);

        TeleportTask.Add(GetTeleportSlotTask(TeleportSlot1.Entity, DuringBubbleMovement));
        TeleportTask.Add(GetTeleportSlotTask(TeleportSlot2.Entity, DuringBubbleMovement));



        if (BubbleMovementTask == null)
        {
            BubbleMotionTasks.Add(TeleportTask);
        }
        else
        {
            BubbleMovementTask.Add(TeleportTask);
        }
    }

    private SerialTasks GetTargetSlotFilledTask(GameObject Obj)
    {
        var TargetSlotdata = Obj.GetComponent<TargetSlotObject>();

        SerialTasks TargetSlotFilledTask = new SerialTasks();

        TargetSlotFilledTask.Add(new WaitTask(TargetSlotdata.ParticleGoOutDelay));

        ParallelTasks ParticleOutTask = new ParallelTasks();

        List<InsideParticle> ParticleList = Obj.GetComponent<TargetSlotObject>().GetInsideParticles();

        for (int i = 0; i < ParticleList.Count; i++)
        {
            SerialTasks Unit = new SerialTasks();
            ParallelTasks UnitMove = new ParallelTasks();

            Vector3 Begin = ParticleList[i].Obj.transform.localPosition;
            Vector2 Dir = ParticleList[i].Obj.transform.localPosition.normalized;
            Vector3 End = (Vector2)Begin + Dir * Random.Range(TargetSlotdata.ParticleGoOutDisMin, TargetSlotdata.ParticleGoOutDisMax);
            UnitMove.Add(new TransformTask(ParticleList[i].Obj, Begin, End, TargetSlotdata.ParticleGoOutTime));

            Color color = ParticleList[i].Obj.GetComponent<SpriteRenderer>().color;

            UnitMove.Add(new ColorChangeTask(ParticleList[i].Obj, new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TargetSlotdata.ParticleGoOutTime));
            UnitMove.Add(new ScaleChangeTask(ParticleList[i].Obj, ParticleList[i].Obj.transform.localScale.x, ParticleList[i].Obj.transform.localScale.x*2, TargetSlotdata.ParticleGoOutTime));


            Unit.Add(new ScaleChangeTask(ParticleList[i].Obj, ParticleList[i].Obj.transform.localScale.x, 0, TargetSlotdata.ParticleGoOutTime));
            //Unit.Add(UnitMove);
            Unit.Add(new DestroySelfTask(ParticleList[i].Obj));

            ParticleOutTask.Add(Unit);
        }

        TargetSlotFilledTask.Add(ParticleOutTask);



        return TargetSlotFilledTask;

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

    private SerialTasks GetEnergyFillTask(GameObject Obj)
    {
        var Data = GetComponent<BubbleMotionData>();

        Color TargetColor = Data.DefaultEnergyColor;

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
            PlaceBubble(P.UseableBubble, P.Pos, P.Type);
        }
    }

    private void OnMotionFinish(MotionFinish M)
    {
        GameManager.levelState = LevelState.Play;
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

        }
    }

    private void OnRollBack(RollBack R)
    {
        if (GameManager.levelState == LevelState.Play && ChangeInfoList.Count > 0)
        {
            StartCoroutine(RollBack());
        }
    }

    private IEnumerator RollBack()
    {

        GameManager.levelState = LevelState.Executing;

        List<BubbleChangeInfo> list = ChangeInfoList[ChangeInfoList.Count - 1];

        ParallelTasks RollBackTask = new ParallelTasks();

        for(int i = 0; i < list.Count; i++)
        {
            if (list[i].Placed)
            {
                switch (list[i].Type)
                {
                    case BubbleType.Normal:
                        RollBackTask.Add(new DisappearTask(list[i].Bubble, RollBackTime / 2, list[i].To, Map, list[i].Type, true));
                        break;
                }

                if (Map[list[i].From.x][list[i].From.y].slotType == SlotType.Target)
                {
                    Map[list[i].From.x][list[i].From.y].Entity.GetComponent<TargetSlotObject>().SetBubbleInside(false);
                }

                Color color = list[i].UseableBubble.GetComponent<SpriteRenderer>().color;
                list[i].UseableBubble.SetActive(true);
                RollBackTask.Add(new ColorChangeTask(list[i].UseableBubble, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), RollBackTime/2));
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

                    if (Map[list[i].To.x][list[i].To.y].slotType == SlotType.Target)
                    {
                        Map[list[i].To.x][list[i].To.y].Entity.GetComponent<TargetSlotObject>().SetBubbleInside(false);
                    }

                    if(Map[list[i].From.x][list[i].From.y].slotType == SlotType.Target)
                    {

                        Map[list[i].From.x][list[i].From.y].Entity.GetComponent<TargetSlotObject>().ClearInsideParticleInfo();
                        Map[list[i].From.x][list[i].From.y].Entity.GetComponent<TargetSlotObject>().SetBubbleInside(true);
                    }
                }
            }
        }


        while (!RollBackTask.IsFinished)
        {
            RollBackTask.Update();
            yield return null;
        }


        ChangeInfoList.Remove(list);
        GameManager.levelState = LevelState.Play;
    }

}
