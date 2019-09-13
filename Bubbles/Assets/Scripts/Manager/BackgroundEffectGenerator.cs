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

        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, true, MapZoneX.x, MapZoneY.x, MapZoneY.y);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, true, MapZoneX.y, MapZoneY.x, MapZoneY.y);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, true, -halfwidth, -halfheight, halfheight);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, true, halfwidth, -halfheight, halfheight);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, false, MapZoneY.x, MapZoneX.x, MapZoneX.y);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, false, MapZoneY.y, MapZoneX.x, MapZoneX.y);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, false, -halfheight, -halfwidth, halfwidth);
        Utility.CircleGetIntersection(Intersectionlist, Pos, dis, false, halfheight, -halfwidth, halfwidth);

        Vector2 Direction=Utility.GetRandomDirectionOfCuttedCircle(Intersectionlist, Pos, cut);

        GameObject Unit = (GameObject)Instantiate(Resources.Load("Prefabs/Effect/BackgroundEffectUnit"), Pos, Quaternion.Euler(0, 0, 0));
        Unit.GetComponent<BackgroundEffectUnit>().SetAttribute(AppearFadeTime, StableTime, MaxAlpha, Speed, Size, Direction);
        Unit.transform.parent = transform;
    }

    

    
}
