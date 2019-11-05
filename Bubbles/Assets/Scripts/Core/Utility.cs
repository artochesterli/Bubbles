using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility
{
    public static Color ColorWithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    public static void CircleGetIntersection(List<Vector2> Intersectionlist, Vector2 Center, float radius, bool vertical, float value, float min, float max)
    {
        if (vertical)
        {
            if (Mathf.Abs(value - Center.x) < radius)
            {
                float y1 = Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - Center.x, 2)) + Center.y;
                float y2 = -Mathf.Sqrt(Mathf.Pow(radius, 2) - Mathf.Pow(value - Center.x, 2)) + Center.y;
                if (y1 >= min && y1 <= max)
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
                    Intersectionlist.Add(new Vector2(x1, value));
                }
                if (x2 > min && x2 < max)
                {
                    Intersectionlist.Add(new Vector2(x2, value));
                }
            }
        }
    }

    public static Vector2 GetRandomDirectionOfCuttedCircle(List<Vector2> Intersectionlist, Vector2 Center, bool cut)
    {
        List<float> IntersectionAnglelist = new List<float>();

        for (int i = 0; i < Intersectionlist.Count; i++)
        {
            float Angle = Mathf.Atan2(Intersectionlist[i].y - Center.y, Intersectionlist[i].x - Center.x) * Mathf.Rad2Deg;
            if (Angle < 0)
            {
                Angle += 360;
            }
            IntersectionAnglelist.Add(Angle);
        }

        for (int i = 0; i < IntersectionAnglelist.Count; i++)
        {
            for (int j = 0; j < IntersectionAnglelist.Count - i - 1; j++)
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
        for (int i = 0; i < IntersectionAnglelist.Count; i++)
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
                AvailableAngleIntervals.Add(new Vector2(LastAngle, IntersectionAnglelist[0] + 360));
                AvailableAngleIntervals.RemoveAt(0);
            }
            else
            {
                AvailableAngleIntervals.Add(new Vector2(0.0f, 360.0f));
            }
        }

        float AngleSum = 0;
        for (int i = 0; i < AvailableAngleIntervals.Count; i++)
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

        if (SpeedAngle >= 90 && SpeedAngle < 180)
        {
            DirX = -DirX;
        }
        else if (SpeedAngle >= 180 && SpeedAngle < 270)
        {
            DirX = -DirX;
            DirY = -DirY;
        }
        else if (SpeedAngle >= 270 && SpeedAngle < 360)
        {
            DirY = -DirY;
        }

        Vector2 v= new Vector2(DirX, DirY);

        return v.normalized;
    }

    public static float SumToTrueValue(float value, List<Vector2> Intervals)
    {
        float sum = 0;
        float answer = 0;
        for (int i = 0; i < Intervals.Count; i++)
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

    public static Vector2 RandomPosFromEdgeArea(Vector2 EdgeZoneX, Vector2 EdgeZoneY, Vector2 HollowZoneX, Vector2 HollowZoneY)
    {
        Vector2 LeftRange = new Vector2(EdgeZoneX.x, HollowZoneX.x);
        Vector2 RightRange = new Vector2(HollowZoneX.y, EdgeZoneX.y);
        Vector2 UpRange = new Vector2(HollowZoneY.y, EdgeZoneY.y);
        Vector2 DownRange = new Vector2(EdgeZoneY.x, HollowZoneY.x);

        float XRangeLength = LeftRange.y - LeftRange.x + RightRange.y - RightRange.x;
        float YRangeLength = UpRange.y - UpRange.x + DownRange.y - DownRange.x;

        float XMarginRecArea = XRangeLength * (EdgeZoneY.y-EdgeZoneY.x);
        float YMarginRecArea = YRangeLength * (HollowZoneX.y - HollowZoneX.x);

        Vector2 Pos;

        if (Random.Range(0, XMarginRecArea + YMarginRecArea) > XMarginRecArea)
        {
            Pos = new Vector2(Random.Range(HollowZoneX.x, HollowZoneX.y), Random.Range(0, YRangeLength));
            if (Pos.y < DownRange.y - DownRange.x)
            {
                Pos.y += DownRange.x;
            }
            else
            {
                Pos.y = Pos.y + DownRange.x + HollowZoneY.y - HollowZoneY.x;
            }
        }
        else
        {
            Pos = new Vector2(Random.Range(0, XRangeLength), Random.Range(EdgeZoneY.x, EdgeZoneY.y));
            if (Pos.x < LeftRange.y - LeftRange.x)
            {
                Pos.x += LeftRange.x;
            }
            else
            {
                Pos.x = Pos.x + LeftRange.x + HollowZoneX.y - HollowZoneX.x;
            }
        }

        return Pos;
    }
}

