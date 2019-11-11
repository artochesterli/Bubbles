using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
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

    }

    private void OnDisable()
    {

    }

    private void Update()
    {

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
