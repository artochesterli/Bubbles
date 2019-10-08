using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AffectTask : Task
{
    private readonly GameObject Obj;
    private readonly Color EnergyColor;

    public AffectTask(GameObject obj, Color energyColor)
    {
        Obj = obj;
        EnergyColor = energyColor;
    }

    protected override void Init()
    {
        GameObject ActivateEffect = Obj.transform.Find("ActivateEffect").gameObject;
        GameObject ReleaseEffect = Obj.transform.Find("ReleaseEffect").gameObject;
        GameObject EmptyEffect = Obj.transform.Find("EmptyReleaseEffect").gameObject;

        ActivateEffect.GetComponent<ParticleSystem>().startColor = EnergyColor;
        EmptyEffect.GetComponent<ParticleSystem>().startColor = EnergyColor;
        foreach (Transform child in ReleaseEffect.transform)
        {
            child.GetComponent<ParticleSystem>().startColor = EnergyColor;
        }

        SetState(TaskState.Success);
    }
}
