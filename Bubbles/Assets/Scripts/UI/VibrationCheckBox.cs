using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VibrationCheckBox : MonoBehaviour
{
    public GameObject CheckMark;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnMouseDown()
    {
        if (GameManager.CurrentConfig.Vibration)
        {
            GameManager.CurrentConfig.Vibration = false;
            CheckMark.SetActive(false);
        }
        else
        {
            GameManager.CurrentConfig.Vibration = true;
            CheckMark.SetActive(true);
            Taptic.Light();
        }
    }

    public void Set()
    {
        if (GameManager.CurrentConfig.Vibration)
        {
            CheckMark.SetActive(true);
        }
        else
        {
            CheckMark.SetActive(false);
        }
    }
}
