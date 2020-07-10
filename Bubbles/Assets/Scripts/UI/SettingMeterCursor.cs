using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingMeterCursor : MonoBehaviour
{
    public GameObject Meter;
    public float ControlBufferY;

    public bool PlaySound;

    private float Min;
    private float Max;

    private bool Hold;


    // Start is called before the first frame update
    void Start()
    {
        GetMeterMinMax();
    }

    // Update is called once per frame
    void Update()
    {
        if (!InControl())
        {
            if(Hold && PlaySound)
            {
                EventManager.instance.Fire(new UpdateConfig());
                GetComponent<AudioSource>().volume = GameManager.CurrentConfig.SoundEffectVol;
                GetComponent<AudioSource>().Play();
            }
            Hold = false;
        }

        FollowMouse();
    }

    private void OnMouseDown()
    {
        Hold = true;
    }

    private bool InControl()
    {
        return Input.mousePosition.y - Screen.height / 2 >= GetComponent<RectTransform>().localPosition.y - ControlBufferY && Input.mousePosition.y - Screen.height / 2 <= GetComponent<RectTransform>().localPosition.y + ControlBufferY;
    }

    private void OnMouseUp()
    {
        if (Hold && PlaySound)
        {
            EventManager.instance.Fire(new UpdateConfig());
            GetComponent<AudioSource>().volume = GameManager.CurrentConfig.SoundEffectVol;
            GetComponent<AudioSource>().Play();
        }
        Hold = false;
    }

    private void FollowMouse()
    {
        if (Hold)
        {
            float MouseX = Input.mousePosition.x - Screen.width/2;

            GetComponent<RectTransform>().localPosition = new Vector3(Mathf.Clamp(MouseX, Min, Max), GetComponent<RectTransform>().localPosition.y, GetComponent<RectTransform>().localPosition.z);
        }
    }

    private void GetMeterMinMax()
    {
        Min = Meter.GetComponent<RectTransform>().localPosition.x;
        Max = Min + Meter.GetComponent<RectTransform>().rect.width;
    }

    public void SetPos(float value)
    {
        GetComponent<RectTransform>().localPosition = new Vector3(Mathf.Lerp(Min, Max, value), GetComponent<RectTransform>().localPosition.y, GetComponent<RectTransform>().localPosition.z);
    }

    public float GetValue()
    {
        return (GetComponent<RectTransform>().localPosition.x - Min) / (Max - Min);
    }
}
