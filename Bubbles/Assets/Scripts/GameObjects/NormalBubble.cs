using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
    public float LightingWaitTime;
    public float LightingTime;

    public float OriScale;
    public float LightingScale;


    private void OnEnable()
    {
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDisable()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
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
            //StartCoroutine(Fade());
        }
    }
}
