using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAuraDisappearTask : Task
{
    public float DisappearTime;
    public float TargetScale;

    private float TimeCount;
    private GameObject Aura;

    public TeleportAuraDisappearTask(float time,float targetScale)
    {
        DisappearTime = time;
        TargetScale = targetScale;
    }

    protected override void Init()
    {
        Aura = GameObject.Find("TeleportAura").gameObject;
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Aura.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * TargetScale, TimeCount / DisappearTime);
        if (TimeCount > DisappearTime)
        {
            GameObject.Destroy(Aura);
            SetState(TaskState.Success);
        }
    }
}
