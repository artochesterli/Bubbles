using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalBubble : MonoBehaviour
{
    public float FinishWaitTime;

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

    private IEnumerator FinishEffect()
    {
        yield return new WaitForSeconds(FinishWaitTime);
        transform.Find("FinishEffect").GetComponent<ParticleSystem>().Play();
    }
}
