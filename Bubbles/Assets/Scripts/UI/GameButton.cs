using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameButton : MonoBehaviour
{
    public GameObject Border;
    public GameObject Inside;
    public ColorChangeType Type;

    public float AppearTime;
    public float UnselectedDisappearTime;
    public float SelectedInflationTime;
    public float SelectedDisappearTime;
    public float SelectedMaxScale;

    // Start is called before the first frame update
    void Start()
    {
        EventManager.instance.AddHandler<UpdateConfig>(OnUpdateConfig);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<UpdateConfig>(OnUpdateConfig);
    }

    private void OnUpdateConfig(UpdateConfig e)
    {
        GetComponent<AudioSource>().volume = GameManager.CurrentConfig.SoundEffectVol;
    }

    public SerialTasks GetSelectedDisappearTask()
    {
        return Utility.GetButtonSelectedDisappearTask(Border, Inside, 1, SelectedMaxScale, SelectedInflationTime, SelectedDisappearTime, Type);
    }

    public ParallelTasks GetUnselectedDisappearTask()
    {
        return Utility.GetButtonUnselectedDisappearTask(Border, Inside, Type, UnselectedDisappearTime);
    }

    public ParallelTasks GetAppearTask()
    {
        return Utility.GetButtonAppearTask(Border, Inside, Type, AppearTime);
    }


}
