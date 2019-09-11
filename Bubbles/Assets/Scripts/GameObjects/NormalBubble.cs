using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
    public float FinishWaitTime;
    public float PowerUpShockWaveGap;
    public float ShockWaveTime;
    public float ShockWaveInitSize;
    public float ShockWaveEndSize;

    public float MoveBackTime;
    public float MoveBackDis;
    public float MoveBackPause;
    public Vector2 MoveOutTimeMinMax;

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
        if (Input.GetKeyDown(KeyCode.H))
        {
            StartCoroutine(PerformShockWave());
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            StartCoroutine(MoveOut());
        }
    }

    private IEnumerator MoveOut()
    {
        float HalfWidth = Camera.main.pixelWidth / Camera.main.pixelHeight * Camera.main.orthographicSize / 2;
        float HalfHeight = Camera.main.orthographicSize / 2;

        MoveOutBasicDirection = Quaternion.Euler(0, 0, Random.Range(0.0f, 360.0f)) * Vector2.right;

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
            transform.position -= (Vector3)MoveOutBasicDirection * BackSpeed * Time.deltaTime;
            yield return null;
        }

        while(TimeCount < MoveBackTime)
        {
            TimeCount += Time.deltaTime;
            BackSpeed = MaxBackSpeed * 2 * (MoveBackTime - TimeCount) / MoveBackTime;
            if (BackSpeed < 0)
            {
                BackSpeed = 0;
            }
            transform.position -= (Vector3)MoveOutBasicDirection * BackSpeed * Time.deltaTime;
            yield return null;
        }

        yield return new WaitForSeconds(MoveBackPause);

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
            TimeScale += Time.deltaTime * 2 / MoveOutTime;
            Vector2 v1 = Vector2.Lerp(StartPoint, MidPoint, TimeCount / MoveOutTime);
            Vector2 v2 = Vector2.Lerp(MidPoint, EndPoint, TimeCount / MoveOutTime);
            transform.position = Vector2.Lerp(v1, v2, TimeCount / MoveOutTime);
            GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / MoveOutTime);
            yield return null;
        }



    }

    private IEnumerator PerformShockWave()
    {
        yield return new WaitForSeconds(FinishWaitTime);
        GameObject StableEfffect = transform.Find("StableEffect").gameObject;

        StableEfffect.GetComponent<ParticleSystem>().Stop();
        StableEfffect.GetComponent<ParticleSystem>().Clear();

        GameObject FinishEffect = transform.Find("FinishEffect").gameObject;

        GameObject PowerUpEffect = transform.Find("PowerUp").gameObject;
        GameObject ShockWave= transform.Find("ShockWave").gameObject;

        PowerUpEffect.GetComponent<ParticleSystem>().Play();
        ParticleSystem.Burst PowerUpBurst = PowerUpEffect.GetComponent<ParticleSystem>().emission.GetBurst(0);

        float TimeCount = 0;
        Color color = ShockWave.GetComponent<SpriteRenderer>().color;
        float PowerUpTime = PowerUpBurst.repeatInterval * (PowerUpBurst.cycleCount - 1) + PowerUpEffect.GetComponent<ParticleSystem>().startLifetime;
        ShockWave.transform.localScale = Vector3.one * ShockWaveInitSize;


        while (TimeCount < PowerUpTime)
        {
            TimeCount += Time.deltaTime;
            ShockWave.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 0), new Color(color.r, color.g, color.b, 1), TimeCount / PowerUpTime);
            yield return null;
        }
        yield return new WaitForSeconds(PowerUpShockWaveGap);


        TimeCount = 0;
        
        while (TimeCount < ShockWaveTime)
        {
            TimeCount += Time.deltaTime;
            ShockWave.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / ShockWaveTime);
            ShockWave.transform.localScale = Vector3.Lerp(Vector3.one * ShockWaveInitSize, Vector3.one * ShockWaveEndSize, TimeCount / ShockWaveTime);
            yield return null;
        }


    }

    private IEnumerator FinishEffect()
    {
        yield return new WaitForSeconds(FinishWaitTime);
        transform.Find("StableEffect").GetComponent<ParticleSystem>().Stop();
        transform.Find("StableEffect").GetComponent<ParticleSystem>().Clear();

        transform.Find("FinishEffect").GetComponent<ParticleSystem>().Play();
    }
}
