using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetSlotObject : MonoBehaviour
{
    public GameObject Effect;
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
        
    }

    private void OnLevelFinish(LevelFinish L)
    {
        Effect.GetComponent<ParticleSystem>().Stop();
    }
}
