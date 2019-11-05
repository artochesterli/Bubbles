using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEffectGenerator : MonoBehaviour
{
    public Vector2 MapZoneX;
    public Vector2 MapZoneY;
    public float UseableBubbleZoneHeight;
    public Vector2 StableTimeMinMax;
    public Vector2 AppearFadeTimeMinMax;
    public Vector2 MaxAlphaMinMax;
    public Vector2Int NumberMinMax;
    public Vector2 SizeMinMax;
    public Vector2 SpeedMinMax;
    public Vector2 IntervalMinMax;
    public int Maxunit;

    private float Timer;
    private Vector2 LeftRange;
    private Vector2 RightRange;
    private Vector2 UpRange;
    private Vector2 DownRange;

    private float halfwidth;
    private float halfheight;

    private Vector2 EdgeZoneX;
    private Vector2 EdgeZoneY;

    // Start is called before the first frame update
    void Start()
    {
        halfwidth = Camera.main.orthographicSize * (float)Camera.main.pixelWidth / Camera.main.pixelHeight;
        halfheight = Camera.main.orthographicSize;

        EdgeZoneX = new Vector2(-halfwidth, halfwidth);
        EdgeZoneY = new Vector2(-halfheight, halfheight);

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

        Vector2 Pos = Utility.RandomPosFromEdgeArea(EdgeZoneX, EdgeZoneY, MapZoneX, MapZoneY);

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
