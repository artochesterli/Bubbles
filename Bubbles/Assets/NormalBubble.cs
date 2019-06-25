using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
    public float LightingWaitTime;
    public float LightingTime;
    public float FadeWaitTime;
    public float FadeTime;

    public float OriScale;
    public float LightingScale;
    public float FadeScale;

    public Color FadeColor;

    private void OnEnable()
    {
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    private IEnumerator Fade()
    {
        yield return new WaitForSeconds(FadeWaitTime);

        float TimeCount = 0;
        Color CurrentColor = GetComponent<SpriteRenderer>().color;
        while (TimeCount < FadeTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<SpriteRenderer>().color = Color.Lerp(CurrentColor, FadeColor, TimeCount / FadeTime);
            transform.localScale = Vector3.Lerp(OriScale * Vector3.one, FadeScale * Vector3.one, TimeCount / FadeTime);
            yield return null;
        }
    }

    private IEnumerator Lighting()
    {
        yield return new WaitForSeconds(LightingWaitTime);

        float TimeCount = 0;
        while (TimeCount < LightingTime)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(OriScale * Vector3.one, LightingScale * Vector3.one, TimeCount / LightingTime);
            yield return null;
        }
    }

    private void OnLevelFinish(LevelFinish L)
    {
        if (L.Success)
        {
            StartCoroutine(Lighting());
        }
        else
        {
            StartCoroutine(Fade());
        }
    }
}
