using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEffectGenerator : MonoBehaviour
{
    public Vector2 MapZoneX;
    public Vector2 MapZoneY;
    public Vector2 StableTimeMinMax;
    public Vector2 AppearFadeTimeMinMax;
    public Vector2 MaxAlphaMinMax;
    public Vector2Int NumberMinMax;
    public Vector2 SizeMinMax;
    public Vector2 SpeedMinMax;
    public Vector2 IntervalMinMax;
    public int Maxunit;

    private float Timer;
    private Vector2 XLeftInterval;
    private Vector2 XRightInterval;
    private Vector2 YUpInterval;
    private Vector2 YDownInterval;

    private float halfwidth;
    private float halfheight;

    // Start is called before the first frame update
    void Start()
    {
        halfwidth = Camera.main.orthographicSize * (float)Camera.main.pixelWidth / Camera.main.pixelHeight;
        halfheight = Camera.main.orthographicSize;
        XLeftInterval = new Vector2(-halfwidth, MapZoneX.x);
        XRightInterval = new Vector2(MapZoneX.y, halfwidth);
        YUpInterval = new Vector2(MapZoneY.y, halfheight);
        YDownInterval = new Vector2(-halfheight, MapZoneY.x);
    }

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer <= 0)
        {
            Timer = Random.Range(IntervalMinMax.x, IntervalMinMax.y);
            GenerateUnit();
        }
    }

    private void GenerateUnit()
    {
        int minnum = NumberMinMax.x;
        int maxnum = NumberMinMax.y;
        if (Maxunit - transform.childCount < maxnum)
        {
            maxnum = Maxunit - transform.childCount;
        }
        if (minnum > maxnum)
        {
            minnum = maxnum;
        }

        int num = Random.Range(minnum, maxnum + 1);

        for (int i = 0; i < num; i++)
        {
            GenerateSingleUnit();
        }
    }

    private void GenerateSingleUnit()
    {
        float StableTime = Random.Range(StableTimeMinMax.x, StableTimeMinMax.y);
        float AppearFadeTime = Random.Range(AppearFadeTimeMinMax.x, AppearFadeTimeMinMax.y);
        float Size = Random.Range(SizeMinMax.x, SizeMinMax.y);
        float Speed = Random.Range(SpeedMinMax.x, SpeedMinMax.y);
        float MaxAlpha = Random.Range(MaxAlphaMinMax.x, MaxAlphaMinMax.y);

        float XIntervalLength = XLeftInterval.y - XLeftInterval.x + XRightInterval.y - XRightInterval.x;
        float YIntervalLength = YUpInterval.y - YUpInterval.x + YDownInterval.y - YDownInterval.x;

        float XMarginRecArea = XIntervalLength * 2 * halfheight;
        float YMarginRecArea = YIntervalLength * (MapZoneX.y - MapZoneX.x);

        Vector2 Pos;

        if (Random.Range(0, XMarginRecArea + YMarginRecArea) > XMarginRecArea)
        {
            Pos = new Vector2(Random.Range(MapZoneX.x, MapZoneX.y), Random.Range(0, YIntervalLength));
            if (Pos.y < YDownInterval.y - YDownInterval.x)
            {
                Pos.y += YDownInterval.x;
            }
            else
            {
                Pos.y = Pos.y + YDownInterval.x + MapZoneY.y - MapZoneY.x;
            }
        }
        else
        {
            Pos = new Vector2(Random.Range(0, XIntervalLength), Random.Range(-halfheight, halfheight));
            if (Pos.x < XLeftInterval.y - XLeftInterval.x)
            {
                Pos.x += XLeftInterval.x;
            }
            else
            {
                Pos.x = Pos.x + XLeftInterval.x + MapZoneX.y - MapZoneX.x;
            }
        }

        float dis = Speed * (2 * AppearFadeTime + StableTime);

        List<float> IntersectionAnglelist = new List<float>();
        List<Vector2> Intersectionlist = new List<Vector2>();

        bool cut;

        if (Pos.x < MapZoneX.x)
        {
            cut = MapZoneX.x - Pos.x < dis && Pos.y <= MapZoneY.y && Pos.y >= MapZoneY.x || halfwidth - Pos.x < dis;
        }
        else
        {
            cut = halfwidth - Pos.x < dis;
        }

        

        GetIntersection(Intersectionlist, Pos, dis, true, MapZoneX.x, MapZoneY.x, MapZoneY.y);
        GetIntersection(Intersectionlist, Pos, dis, true, MapZoneX.y, MapZoneY.x, MapZoneY.y);
        GetIntersection(Intersectionlist, Pos, dis, true, -halfwidth, -halfheight, halfheight);
        GetIntersection(Intersectionlist, Pos, dis, true, halfwidth, -halfheight, halfheight);
        GetIntersection(Intersectionlist, Pos, dis, false, MapZoneY.x, MapZoneX.x, MapZoneX.y);
        GetIntersection(Intersectionlist, Pos, dis, false, MapZoneY.y, MapZoneX.x, MapZoneX.y);
        GetIntersection(Intersectionlist, Pos, dis, false, -halfheight, -halfwidth, halfwidth);
        GetIntersection(Intersectionlist, Pos, dis, false, halfheight, -halfwidth, halfwidth);

        for (int i = 0; i < Intersectionlist.Count; i++)
        {
            float Angle = Mathf.Atan2(Intersectionlist[i].y-Pos.y, Intersectionlist[i].x-Pos.x)*Mathf.Rad2Deg;
            if (Angle < 0)
            {
                Angle += 360;
            }
            IntersectionAnglelist.Add(Angle);
        }

        for(int i = 0; i < IntersectionAnglelist.Count; i++)
        {
            for(int j = 0; j < IntersectionAnglelist.Count - i - 1; j++)
            {
                if (IntersectionAnglelist[j] > IntersectionAnglelist[j + 1])
                {
                    float temp = IntersectionAnglelist[j];
                    IntersectionAnglelist[j] = IntersectionAnglelist[j + 1];
                    IntersectionAnglelist[j + 1] = temp;
                }
            }
        }

        List<Vector2> AvailableAngleIntervals = new List<Vector2>();
        float LastAngle = 0;
        for(int i = 0; i < IntersectionAnglelist.Count; i++)
        {
            if (!cut)
            {
                AvailableAngleIntervals.Add(new Vector2(LastAngle, IntersectionAnglelist[i]));
                
            }
            cut = !cut;
            LastAngle = IntersectionAnglelist[i];
        }

        if (!cut)
        {
            if (IntersectionAnglelist.Count > 0)
            {
                AvailableAngleIntervals.Add(new Vector2(LastAngle, IntersectionAnglelist[0]+360));
                AvailableAngleIntervals.RemoveAt(0);
            }
            else
            {
                AvailableAngleIntervals.Add(new Vector2(0.0f, 360.0f));
            }
        }

        float AngleSum = 0;
        for(int i = 0; i < AvailableAngleIntervals.Count; i++)
        {
            AngleSum += AvailableAngleIntervals[i].y - AvailableAngleIntervals[i].x;

        }
        float SpeedAngle = Random.Range(0.0f, AngleSum);
        SpeedAngle = SumToTrueValue(SpeedAngle, AvailableAngleIntervals);
        float DirX = 1;
        float DirY = Mathf.Abs(Mathf.Tan(SpeedAngle * Mathf.Deg2Rad));

        while (SpeedAngle > 360)
        {
            SpeedAngle -= 360;
        }

        if(SpeedAngle >= 90 && SpeedAngle < 180)
        {
            DirX = -DirX;
        }
        else if(SpeedAngle>=180 && SpeedAngle < 270)
        {
            DirX = -DirX;
            DirY = -DirY;
        }
        else if(SpeedAngle >=270 && SpeedAngle<360)
        {
            DirY = -DirY;
        }

        Vector2 Direction = new Vector2(DirX,DirY);

        Direction.Normalize();

        GameObject Unit = (GameObject)Instantiate(Resources.Load("Prefabs/Effect/BackgroundEffectUnit"), Pos, Quaternion.Euler(0, 0, 0));
        Unit.GetComponent<BackgroundEffectUnit>().SetAttribute(AppearFadeTime, StableTime, MaxAlpha, Speed, Size, Direction);
        Unit.transform.parent = transform;
    }

    private float SumToTrueValue(float value, List<Vector2> Intervals)
    {
        float sum = 0;
        float answer = 0;
        for(int i = 0; i < Intervals.Count; i++)
        {
            if (value - sum <= Intervals[i].y - Intervals[i].x)
            {
                answer = Intervals[i].x + value - sum;
                break;
            }
            else
            {
                sum += Intervals[i].y - Intervals[i].x;
            }
        }
        return answer;
    }

    private void GetIntersection(List<Vector2> Intersectionlist, Vector2 Center, float radius, bool vertical, float value, float min, float max)
    {
        if (vertical)
        {
            if (Mathf.Abs(value - Center.x) < radius)
            {
                float y1 = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - Center.x, 2)) + Center.y;
                float y2 = -Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - Center.x, 2)) + Center.y;
                if(y1>=min && y1 <= max)
                {
                    Intersectionlist.Add(new Vector2(value, y1));
                }
                if (y2 >= min && y2 <= max)
                {
                    Intersectionlist.Add(new Vector2(value, y2));
                }
            }
        }
        else
        {
            if (Mathf.Abs(value - Center.y) < radius)
            {
                float x1 = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - Center.y, 2)) + Center.x;
                float x2 = -Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - Center.y, 2)) + Center.x;
                if (x1 > min && x1 < max)
                {
                    Intersectionlist.Add(new Vector2(x1,value));
                }
                if (x2 > min && x2 < max)
                {
                    Intersectionlist.Add(new Vector2(x2, value));
                }
            }
        }
    }
}
