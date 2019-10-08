using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSlotObject : MonoBehaviour
{
    public GameObject Effect;

    public float FilledEffectSize;
    public float EmptyEffectSize;
    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.levelState == LevelState.Play)
        {
            SetEffect();
        }
    }

    private void OnLevelFinish(LevelFinish L)
    {
        Effect.GetComponent<ParticleSystem>().Stop();
    }

    private void SetEffect()
    {
        ParticleSystem.SizeOverLifetimeModule SizeModule = Effect.GetComponent<ParticleSystem>().sizeOverLifetime;
        if (GetComponent<SlotObject>().ConnectedSlotInfo.InsideBubbleType == BubbleType.Normal)
        {
            SizeModule.sizeMultiplier = FilledEffectSize;
        }
        else
        {
            SizeModule.sizeMultiplier = EmptyEffectSize;
        }
    }
}
