using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockWavePowerUpTask : Task
{
    private readonly GameObject Obj;
    private readonly GameObject PowerUpEffectPrefab;
    private readonly GameObject ShockWave;
    private readonly float PowerUpSelfScale;
    private readonly float PowerUpTime;
    private readonly float PowerUpInterval;
    private readonly int PowerUpNumber;

    private Color color;
    private float TimeCount;
    private int Number;
    private float ProcessTime;

    public ShockWavePowerUpTask(GameObject obj, GameObject prefab, GameObject shockwave, float scale, float time, float interval, int number)
    {
        Obj = obj;
        PowerUpEffectPrefab = prefab;
        ShockWave = shockwave;
        PowerUpSelfScale = scale;
        PowerUpTime = time;
        PowerUpInterval = interval;
        PowerUpNumber = number;
    }

    protected override void Init()
    {
        base.Init();
        color = ShockWave.GetComponent<SpriteRenderer>().color;
        ProcessTime = PowerUpTime + (PowerUpNumber - 1) * PowerUpInterval;
    }

    internal override void Update()
    {
        base.Update();

        TimeCount += Time.deltaTime;

        if (TimeCount >= Number * PowerUpInterval && Number < PowerUpNumber)
        {
            Number++;
            GameObject PowerUp = GameObject.Instantiate(PowerUpEffectPrefab, Obj.transform.position, Quaternion.Euler(0, 0, 0));
            PowerUp.GetComponent<PowerUpEffect>().PlayTime = PowerUpTime;
            PowerUp.GetComponent<PowerUpEffect>().EndScale = Vector3.Lerp(Vector3.one, Vector3.one * PowerUpSelfScale, (TimeCount + PowerUpTime) / ProcessTime).x;
        }
        ShockWave.GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 0), new Color(color.r, color.g, color.b, 1), TimeCount / ProcessTime);
        Obj.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * PowerUpSelfScale, TimeCount / ProcessTime);

        if (TimeCount >= ProcessTime)
        {
            SetState(TaskState.Success);
        }
    }
}
