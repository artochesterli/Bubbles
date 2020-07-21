using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosInfo
{
    public Vector2 LegalPos;
    public Dictionary<Direction, float> DicDirOffset;
    public Dictionary<Direction, bool> DicDirMoveTask;

    public PosInfo(Vector2 Pos)
    {
        LegalPos = Pos;

        DicDirOffset = new Dictionary<Direction, float>();
        DicDirOffset.Add(Direction.Right, 0);
        DicDirOffset.Add(Direction.Left, 0);
        DicDirOffset.Add(Direction.Up, 0);
        DicDirOffset.Add(Direction.Down, 0);

        DicDirMoveTask = new Dictionary<Direction, bool>();
        DicDirMoveTask.Add(Direction.Right, false);
        DicDirMoveTask.Add(Direction.Left, false);
        DicDirMoveTask.Add(Direction.Up, false);
        DicDirMoveTask.Add(Direction.Down, false);
    }
}

public class NormalBubble : MonoBehaviour
{
    public float IntendMoveDis;
    public float IntendMoveTime;
    public Direction IntendMoveDir;
    public PosInfo SelfPosInfo;

    public GameObject PowerUpEffectPrefab;
    public GameObject ShockWave;
    public float PowerUpTime;
    public float PowerUpInterval;
    public int PowerUpNumber;
    public float PowerUpInitScale;
    public float PowerUpShockWaveGap;
    public float PowerUpSelfScale;
    public float PowerUpSelfInflatedScale;
    public float ShockWaveTime;
    public float ShockWaveInitSize;
    public float ShockWaveEndSize;
    public float ShockWaveAlpha;

    public float MoveBackTime;
    public float MoveBackDis;
    public float MoveBackPause;
    public float MoveOutAcTimePercentage;
    public float MoveOutMaxTimeScale;
    public Vector2 MoveOutTimeMinMax;
    public float MoveOutMinDis;
    

    public Vector2 MoveOutMidPointHorizontalPercentageMinMax;
    public Vector2 MoveOutMidPointVerticalOffsetMinMax;

    public float Size;

    private Vector2 MoveOutBasicDirection;

    private const float RayDis = 20;

    private void OnEnable()
    {
        EventManager.instance.AddHandler<CallBackToSelectLevel>(OnBackToSelectLevel);
        EventManager.instance.AddHandler<UpdateConfig>(OnUpdateConfig);

        GetComponent<AudioSource>().volume = GameManager.CurrentConfig.SoundEffectVol;

        SelfPosInfo = new PosInfo(transform.localPosition);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<CallBackToSelectLevel>(OnBackToSelectLevel);
        EventManager.instance.RemoveHandler<UpdateConfig>(OnUpdateConfig);
    }

    private void Update()
    {
        MaintainIntendMove();
    }

    private void MaintainIntendMove()
    {
        if (IntendMoveDir != Direction.Null)
        {
            if (!SelfPosInfo.DicDirMoveTask[IntendMoveDir])
            {
                SelfPosInfo.DicDirOffset[IntendMoveDir] += IntendMoveDis / IntendMoveTime*Time.deltaTime;
                if(SelfPosInfo.DicDirOffset[IntendMoveDir] > IntendMoveDis)
                {
                    SelfPosInfo.DicDirOffset[IntendMoveDir] = IntendMoveDis;
                }
            }


            Dictionary<Direction, float> Temp = new Dictionary<Direction, float>(SelfPosInfo.DicDirOffset);

            foreach (KeyValuePair<Direction, float> item in SelfPosInfo.DicDirOffset)
            {
                if (item.Key != IntendMoveDir)
                {
                    Temp[item.Key] -= IntendMoveDis / IntendMoveTime * Time.deltaTime;
                    if (Temp[item.Key] < 0)
                    {
                        Temp[item.Key] = 0;
                    }
                }
            }

            SelfPosInfo.DicDirOffset = Temp;
        }
        else
        {
            Dictionary<Direction, float> Temp = new Dictionary<Direction, float>(SelfPosInfo.DicDirOffset);

            foreach (KeyValuePair<Direction, float> item in SelfPosInfo.DicDirOffset)
            {
                Temp[item.Key] -= IntendMoveDis / IntendMoveTime * Time.deltaTime;
                if (Temp[item.Key] < 0)
                {
                    Temp[item.Key] = 0;
                }
            }

            SelfPosInfo.DicDirOffset = Temp;
        }

        transform.localPosition = SelfPosInfo.LegalPos + Vector2.right * SelfPosInfo.DicDirOffset[Direction.Right] + Vector2.left * SelfPosInfo.DicDirOffset[Direction.Left]
            + Vector2.up * SelfPosInfo.DicDirOffset[Direction.Up] + Vector2.down * SelfPosInfo.DicDirOffset[Direction.Down];
    }

    private void OnUpdateConfig(UpdateConfig e)
    {
        GetComponent<AudioSource>().volume = GameManager.CurrentConfig.SoundEffectVol;
    }

    private void OnBackToSelectLevel(CallBackToSelectLevel e)
    {
        transform.Find("ActivateEffect").GetComponent<ParticleSystem>().Stop();
    }

    public SerialTasks GetShockWavePowerUpTask()
    {
        SerialTasks PowerUpTask = new SerialTasks();

        PowerUpTask.Add(new ShockWavePowerUpTask(gameObject, PowerUpEffectPrefab, ShockWave, PowerUpSelfScale, PowerUpTime, PowerUpInterval, PowerUpNumber));
        PowerUpTask.Add(new WaitTask(PowerUpShockWaveGap));

        return PowerUpTask;
    }

    public ShockWaveEmitTask GetShockWaveEmitTask()
    {
        return new ShockWaveEmitTask(gameObject, ShockWave, ShockWaveTime, ShockWaveEndSize, PowerUpSelfInflatedScale, PowerUpSelfScale);
    }

    public SerialTasks GetMoveOutPrepareTask()
    {
        SerialTasks MoveOutPrepareTask = new SerialTasks();

        List<Vector2> IntersectionList = new List<Vector2>();
        List<float> IntersectionAngle = new List<float>();

        float halfheight = Camera.main.orthographicSize;
        float halfwidth = Camera.main.orthographicSize * Camera.main.pixelWidth / Camera.main.pixelHeight;

        bool cut = transform.position.x + MoveOutMinDis > halfwidth;

        Utility.CircleGetIntersection(IntersectionList, transform.position, MoveOutMinDis, true, -halfwidth, -halfheight, halfheight);
        Utility.CircleGetIntersection(IntersectionList, transform.position, MoveOutMinDis, true, halfwidth, -halfheight, halfheight);
        Utility.CircleGetIntersection(IntersectionList, transform.position, MoveOutMinDis, false, -halfheight, -halfwidth, halfwidth);
        Utility.CircleGetIntersection(IntersectionList, transform.position, MoveOutMinDis, false, halfheight, -halfwidth, halfwidth);

        MoveOutBasicDirection = Utility.GetRandomDirectionOfCuttedCircle(IntersectionList, transform.position, cut);

        MoveOutPrepareTask.Add(new MoveOutBackAccelerationTask(gameObject, MoveBackDis, MoveBackTime, MoveOutBasicDirection, PowerUpSelfInflatedScale));
        MoveOutPrepareTask.Add(new MoveOutBackDecelerationTask(gameObject, MoveBackDis, MoveBackTime, MoveOutBasicDirection, PowerUpSelfInflatedScale));

        return MoveOutPrepareTask;
    }

    public SerialTasks GetMoveOutEscapeTask()
    {
        SerialTasks MoveOutEscape= new SerialTasks();

        int layermask = 1 << LayerMask.NameToLayer("Border");

        RaycastHit2D Hit = Physics2D.Raycast(transform.position, MoveOutBasicDirection, RayDis, layermask);

        Vector2 StartPoint = transform.position;
        Vector2 EndPoint = Hit.point;
        if (Hit.collider.gameObject.name == "Up" || Hit.collider.gameObject.name == "Down")
        {
            EndPoint += MoveOutBasicDirection * Mathf.Abs(1 / MoveOutBasicDirection.y) * Size / 2;
        }
        else
        {
            EndPoint += MoveOutBasicDirection * Mathf.Abs(1 / MoveOutBasicDirection.x) * Size / 2;
        }


        Vector2 MidPoint = Vector3.Lerp(StartPoint, EndPoint, Random.Range(MoveOutMidPointHorizontalPercentageMinMax.x, MoveOutMidPointHorizontalPercentageMinMax.y));

        if (Hit.collider.gameObject.name == "Up")
        {
            if (MoveOutBasicDirection.x > 0)
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, 90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
            else
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, -90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
        }
        else if (Hit.collider.gameObject.name == "Down")
        {
            if (MoveOutBasicDirection.x > 0)
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, -90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
            else
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, 90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
        }
        else if (Hit.collider.gameObject.name == "Left")
        {
            if (MoveOutBasicDirection.y > 0)
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, 90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
            else
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, -90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
        }
        else
        {
            if (MoveOutBasicDirection.y > 0)
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, -90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
            else
            {
                MidPoint += (Vector2)(Quaternion.Euler(0, 0, 90) * MoveOutBasicDirection) * Random.Range(MoveOutMidPointVerticalOffsetMinMax.x, MoveOutMidPointVerticalOffsetMinMax.y);
            }
        }

        float MoveOutTime = Random.Range(MoveOutTimeMinMax.x, MoveOutTimeMinMax.y);

        MoveOutEscape.Add(new MoveOutEscapeTask(gameObject, StartPoint, EndPoint, MidPoint, MoveOutTime, MoveOutMaxTimeScale, MoveOutAcTimePercentage));

        return MoveOutEscape;

    }

}
