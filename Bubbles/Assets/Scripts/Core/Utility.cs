using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    public static SerialTasks GetButtonSelectedDisappearTask(GameObject BorderImage, GameObject InsideContent, float StartScale, float EndScale, float InflationTime, float DeflationTime, bool ContentImage)
    {
        SerialTasks DisappearTask = new SerialTasks();

        ParallelTasks InflationTask = new ParallelTasks();

        InflationTask.Add(new ScaleChangeTask(BorderImage, StartScale, EndScale, InflationTime));
        InflationTask.Add(new ScaleChangeTask(InsideContent, StartScale, EndScale, InflationTime));

        DisappearTask.Add(InflationTask);

        ParallelTasks DeflationTask = new ParallelTasks();

        DeflationTask.Add(new ScaleChangeTask(BorderImage, EndScale, StartScale,  DeflationTime));
        DeflationTask.Add(new ScaleChangeTask(InsideContent, EndScale, StartScale,  DeflationTime));

        Color BorderImageColor = BorderImage.GetComponent<Image>().color;
        Color InsideContentColor;
        ColorChangeType InsideContentChangeType;
        if (ContentImage)
        {
            InsideContentChangeType = ColorChangeType.Image;
            InsideContentColor = InsideContent.GetComponent<Image>().color;
        }
        else
        {
            InsideContentChangeType = ColorChangeType.Text;
            InsideContentColor = InsideContent.GetComponent<Text>().color;
        }

        DeflationTask.Add(new ColorChangeTask(BorderImage, ColorWithAlpha(BorderImageColor, 1), ColorWithAlpha(BorderImageColor, 0), DeflationTime, ColorChangeType.Image));
        DeflationTask.Add(new ColorChangeTask(InsideContent, ColorWithAlpha(InsideContentColor, 1), ColorWithAlpha(InsideContentColor, 0), DeflationTime, InsideContentChangeType));

        DisappearTask.Add(DeflationTask);

        return DisappearTask;
    }

    public static SerialTasks GetButtonSelectedDisappearTask(GameObject BorderImage, GameObject InsideContent, GameObject SelectedEffect,
        bool ContentIsImage, float SelectedEffectScale, float SelectedEffectTime, float DisappearTime)
    {
        SerialTasks SelectedDisappearTask = new SerialTasks();

        ParallelTasks SelectedEffectTask = new ParallelTasks();


        SelectedEffectTask.Add(new ScaleChangeTask(SelectedEffect, 1, SelectedEffectScale, SelectedEffectTime));

        Color EffectColor = SelectedEffect.GetComponent<Image>().color;
        SelectedEffectTask.Add(new ColorChangeTask(SelectedEffect, ColorWithAlpha(EffectColor, 1), ColorWithAlpha(EffectColor, 0), SelectedEffectTime, ColorChangeType.Image));

        SelectedDisappearTask.Add(SelectedEffectTask);

        ParallelTasks SelfDisappearTask = new ParallelTasks();

        Color BorderImageColor = BorderImage.GetComponent<Image>().color;
        Color InsideContentColor;
        ColorChangeType InsideContentChangeType;
        if (ContentIsImage)
        {
            InsideContentColor = InsideContent.GetComponent<Image>().color;
            InsideContentChangeType = ColorChangeType.Image;
        }
        else
        {
            InsideContentColor = InsideContent.GetComponent<Text>().color;
            InsideContentChangeType = ColorChangeType.Text;
        }


        SelfDisappearTask.Add(new ColorChangeTask(BorderImage, ColorWithAlpha(BorderImageColor, 1), ColorWithAlpha(BorderImageColor, 0), DisappearTime, ColorChangeType.Image));

        SelfDisappearTask.Add(new ColorChangeTask(InsideContent, ColorWithAlpha(InsideContentColor, 1), ColorWithAlpha(InsideContentColor, 0), DisappearTime, InsideContentChangeType));

        SelectedDisappearTask.Add(SelfDisappearTask);

        return SelectedDisappearTask;
    }

    public static ParallelTasks GetButtonUnselectedDisappearTask(GameObject BorderImage, GameObject InsideContent, bool ContentIsImage, float DisappearTime)
    {
        ParallelTasks UnselectedDisappearTask = new ParallelTasks();

        Color BorderImageColor = BorderImage.GetComponent<Image>().color;
        Color InsideContentColor;
        ColorChangeType InsideContentChangeType;
        if (ContentIsImage)
        {
            InsideContentColor = InsideContent.GetComponent<Image>().color;
            InsideContentChangeType = ColorChangeType.Image;
        }
        else
        {
            InsideContentColor = InsideContent.GetComponent<Text>().color;
            InsideContentChangeType = ColorChangeType.Text;
        }

        UnselectedDisappearTask.Add(new ColorChangeTask(BorderImage, ColorWithAlpha(BorderImageColor, 1), ColorWithAlpha(BorderImageColor, 0), DisappearTime, ColorChangeType.Image));

        UnselectedDisappearTask.Add(new ColorChangeTask(InsideContent, ColorWithAlpha(InsideContentColor, 1), ColorWithAlpha(InsideContentColor, 0), DisappearTime, InsideContentChangeType));

        return UnselectedDisappearTask;
    }

    public static ParallelTasks GetButtonAppearTask(GameObject BorderImage, GameObject InsideContent, bool ContentIsImage, float AppearTime)
    {
        ParallelTasks AppearTask = new ParallelTasks();

        Color BorderImageColor = BorderImage.GetComponent<Image>().color;
        Color InsideContentColor;
        ColorChangeType InsideContentChangeType;
        if (ContentIsImage)
        {
            InsideContentColor = InsideContent.GetComponent<Image>().color;
            InsideContentChangeType = ColorChangeType.Image;
        }
        else
        {
            InsideContentColor = InsideContent.GetComponent<Text>().color;
            InsideContentChangeType = ColorChangeType.Text;
        }

        AppearTask.Add(new ColorChangeTask(BorderImage, ColorWithAlpha(BorderImageColor, 0), ColorWithAlpha(BorderImageColor, 1), AppearTime, ColorChangeType.Image));

        AppearTask.Add(new ColorChangeTask(InsideContent, ColorWithAlpha(InsideContentColor, 0), ColorWithAlpha(InsideContentColor, 1), AppearTime, InsideContentChangeType));

        return AppearTask;
    }

    public static ParallelTasks GetTextAppearTask(GameObject Text,float AppearTime)
    {
        Color color = Text.GetComponent<Text>().color;

        ParallelTasks Tasks = new ParallelTasks();

        Tasks.Add(new ColorChangeTask(Text, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), AppearTime, ColorChangeType.Text));

        return Tasks;
    }

    public static ParallelTasks GetTextDisappearTask(GameObject Text,float DisappearTime)
    {
        Color color = Text.GetComponent<Text>().color;

        ParallelTasks Tasks = new ParallelTasks();

        Tasks.Add(new ColorChangeTask(Text, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Text));

        return Tasks;
    }

    public static ParallelTasks GetImageAppearTask(GameObject Image, float AppearTime)
    {
        Color color = Image.GetComponent<Image>().color;

        ParallelTasks Tasks = new ParallelTasks();

        Tasks.Add(new ColorChangeTask(Image, Utility.ColorWithAlpha(color, 0), Utility.ColorWithAlpha(color, 1), AppearTime, ColorChangeType.Image));

        return Tasks;
    }

    public static ParallelTasks GetImageDisappearTask(GameObject Image, float DisappearTime)
    {
        Color color = Image.GetComponent<Image>().color;

        ParallelTasks Tasks = new ParallelTasks();

        Tasks.Add(new ColorChangeTask(Image, Utility.ColorWithAlpha(color, 1), Utility.ColorWithAlpha(color, 0), DisappearTime, ColorChangeType.Image));

        return Tasks;
    }
}

