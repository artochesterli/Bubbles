using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
    public float FinishWaitTime;
    public float SlotDisAppearTime;
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
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnLevelFinish(LevelFinish L)
    {
        StartCoroutine(FinishEffect());

    }

    private void Update()
    {

    }

    public IEnumerator MoveOut()
    {
        float HalfWidth = Camera.main.pixelWidth / Camera.main.pixelHeight * Camera.main.orthographicSize / 2;
        float HalfHeight = Camera.main.orthographicSize / 2;



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


        float TimeCount = 0;

        Vector2 CurrentPos = transform.position;
        float BackSpeed = 0;
        float MaxBackSpeed = MoveBackDis / MoveBackTime;


        while (TimeCount < MoveBackTime / 2)
        {
            TimeCount += Time.deltaTime;
            if (TimeCount < MoveBackTime / 2)
            {
                BackSpeed = MaxBackSpeed * 2 * TimeCount / MoveBackTime;
            }
            else
            {
                BackSpeed = MaxBackSpeed * 2 * (MoveBackTime - TimeCount) / MoveBackTime;
            }
            transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfInflatedScale, Vector3.one, TimeCount / MoveBackTime);
            transform.position -= (Vector3)MoveOutBasicDirection * BackSpeed * Time.deltaTime;
            yield return null;
        }

        while (TimeCount < MoveBackTime)
        {
            TimeCount += Time.deltaTime;
            BackSpeed = MaxBackSpeed * 2 * (MoveBackTime - TimeCount) / MoveBackTime;
            if (BackSpeed < 0)
            {
                BackSpeed = 0;
            }
            transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfInflatedScale, Vector3.one, TimeCount / MoveBackTime);
            transform.position -= (Vector3)MoveOutBasicDirection * BackSpeed * Time.deltaTime;
            yield return null;
        }

        int layermask = 1 << LayerMask.NameToLayer("Border");

        RaycastHit2D Hit = Physics2D.Raycast(transform.position, MoveOutBasicDirection,RayDis,layermask);

        Vector2 StartPoint = transform.position;
        Vector2 EndPoint = Hit.point;
        if(Hit.collider.gameObject.name == "Up" || Hit.collider.gameObject.name == "Down")
        {
            EndPoint += MoveOutBasicDirection * Mathf.Abs(1 / MoveOutBasicDirection.y)*Size/2;
        }
        else
        {
            EndPoint += MoveOutBasicDirection * Mathf.Abs(1 / MoveOutBasicDirection.x) * Size / 2;
        }


        Vector2 MidPoint = Vector3.Lerp(StartPoint,EndPoint,Random.Range(MoveOutMidPointHorizontalPercentageMinMax.x, MoveOutMidPointHorizontalPercentageMinMax.y));

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
        else if(Hit.collider.gameObject.name == "Down")
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
        else if(Hit.collider.gameObject.name == "Left")
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

        TimeCount = 0;

        float MoveOutTime = Random.Range(MoveOutTimeMinMax.x, MoveOutTimeMinMax.y);

        Color color = GetComponent<SpriteRenderer>().color;

        float TimeScale=0;


        while (TimeCount < MoveOutTime)
        {
            TimeCount += Time.deltaTime * TimeScale;
            if (TimeCount < MoveOutTime * MoveOutAcTimePercentage)
            {
                TimeScale += MoveOutMaxTimeScale / (MoveOutAcTimePercentage * MoveOutTime) * Time.deltaTime;
            }
            else
            {
                TimeScale -= (2 * MoveOutMaxTimeScale - (2 - MoveOutMaxTimeScale * MoveOutAcTimePercentage) / (1 - MoveOutAcTimePercentage)) / ((1 - MoveOutAcTimePercentage) * MoveOutTime) * Time.deltaTime;
            }
            Vector2 v1 = Vector2.Lerp(StartPoint, MidPoint, TimeCount / MoveOutTime);
            Vector2 v2 = Vector2.Lerp(MidPoint, EndPoint, TimeCount / MoveOutTime);
            transform.position = Vector2.Lerp(v1, v2, TimeCount / MoveOutTime);
            GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / MoveOutTime);
            yield return null;
        }


    }

    private IEnumerator PerformShockWave()
    {
        GameObject StableEfffect = transform.Find("StableEffect").gameObject;

        StableEfffect.GetComponent<ParticleSystem>().Stop();
        StableEfffect.GetComponent<ParticleSystem>().Clear();

        GameObject ShockWave= transform.Find("ShockWave").gameObject;

        float TimeCount = 0;
        Color color = ShockWave.GetComponent<SpriteRenderer>().color;
        ShockWave.transform.localScale = Vector3.one * ShockWaveInitSize;

        float PowerUpProcess = PowerUpTime + (PowerUpNumber - 1) * PowerUpInterval;

        int Number = 0;
        while (TimeCount < PowerUpProcess)
        {
            TimeCount += Time.deltaTime;
            if (TimeCount >= Number * PowerUpInterval && Number<PowerUpNumber)
            {
                Number++;
                GameObject PowerUp = (GameObject)Instantiate(Resources.Load("Prefabs/Effect/PowerUpEffect"), transform.position, Quaternion.Euler(0, 0, 0));
                PowerUp.GetComponent<PowerUpEffect>().PlayTime = PowerUpTime;
                PowerUp.GetComponent<PowerUpEffect>().EndScale= Vector3.Lerp(Vector3.one, Vector3.one * PowerUpSelfScale, (TimeCount+PowerUpTime) / PowerUpProcess).x;
            }
            ShockWave.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 0), new Color(color.r, color.g, color.b, 1), TimeCount / PowerUpProcess);
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * PowerUpSelfScale, TimeCount / PowerUpProcess);
            yield return null;
        }
        yield return new WaitForSeconds(PowerUpShockWaveGap);


        TimeCount = 0;

        float InflateTime = ShockWaveTime * (PowerUpSelfInflatedScale-PowerUpSelfScale)/(ShockWaveEndSize-PowerUpSelfScale);
        float DeflateTime = ShockWaveTime * (PowerUpSelfInflatedScale - 1) / (ShockWaveEndSize - PowerUpSelfScale);

        while (TimeCount < ShockWaveTime)
        {
            TimeCount += Time.deltaTime;
            ShockWave.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / ShockWaveTime);

            transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfScale, Vector3.one * PowerUpSelfInflatedScale, TimeCount / InflateTime);

            ShockWave.transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfScale, Vector3.one * ShockWaveEndSize, TimeCount / ShockWaveTime)/transform.localScale.x;
            yield return null;
        }

    }

    private IEnumerator FinishEffect()
    {
        yield return new WaitForSeconds(FinishWaitTime);

        //GameObject InTargetEffect = transform.Find("InTargetEffect").gameObject;
        //InTargetEffect.GetComponent<ParticleSystem>().Stop();
        //InTargetEffect.GetComponent<ParticleSystem>().Clear();

        yield return StartCoroutine(PerformShockWave());
        yield return new WaitForSeconds(SlotDisAppearTime);
        yield return StartCoroutine(MoveOut());


    }
}
