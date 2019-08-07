using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAuraGenerationTask : Task
{
    private readonly Vector3 StartLocation;
    private readonly float GenerationTime;

    private float TimeCount;
    private GameObject Obj;

    public TeleportAuraGenerationTask(Vector3 startLocation,float generationTime)
    {
        StartLocation = startLocation;
        GenerationTime = generationTime;
    }

    protected override void Init()
    {
        Obj = (GameObject)GameObject.Instantiate(Resources.Load("Prefabs/Effect/TeleportTimeAura"), StartLocation, Quaternion.Euler(0, 0, 0));
        Obj.transform.localScale = Vector3.zero;
        Obj.name = "TeleportAura";
    }

    internal override void Update()
    {
        TimeCount += Time.deltaTime;
        Obj.transform.position = Vector3.Lerp(StartLocation, Vector3.zero, TimeCount / GenerationTime);
        Obj.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, TimeCount / GenerationTime);

        if (TimeCount > GenerationTime)
        {
            SetState(TaskState.Success);
        }
    }
}
