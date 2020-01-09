using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWaveEmitTask : Task
{
    private readonly GameObject Obj;
    private readonly GameObject ShockWave;
    private readonly float ShockWaveTime;
    private readonly float ShockWaveEndSize;
    private readonly float PowerUpSelfInflatedScale;
    private readonly float PowerUpSelfScale;

    private Color color;
    private float TimeCount;
    private float InflateTime;

    public ShockWaveEmitTask(GameObject obj, GameObject shockwave, float time, float size, float inflatedscale, float scale)
    {
        Obj = obj;
        ShockWave = shockwave;
        ShockWaveTime = time;
        ShockWaveEndSize = size;
        PowerUpSelfInflatedScale = inflatedscale;
        PowerUpSelfScale = scale;
    }

    protected override void Init()
    {
        base.Init();
        color = ShockWave.GetComponent<SpriteRenderer>().color;
        InflateTime = ShockWaveTime * (PowerUpSelfInflatedScale - PowerUpSelfScale) / (ShockWaveEndSize - PowerUpSelfScale);

        GameManager.ActivatedLevel.GetComponent<AudioSource>().Play();
    }

    internal override void Update()
    {
        base.Update();
        TimeCount += Time.deltaTime;
        ShockWave.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 1), new Color(color.r, color.g, color.b, 0), TimeCount / ShockWaveTime);

        Obj.transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfScale, Vector3.one * PowerUpSelfInflatedScale, TimeCount / InflateTime);

        ShockWave.transform.localScale = Vector3.Lerp(Vector3.one * PowerUpSelfScale, Vector3.one * ShockWaveEndSize, TimeCount / ShockWaveTime) / Obj.transform.localScale.x;

        if(TimeCount >= ShockWaveTime)
        {
            SetState(TaskState.Success);
        }
    }
}
