using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackButton : MonoBehaviour
{
    public float MaxScale;

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
        GetComponent<AudioSource>().Play();

        if (GameManager.CurrentConfig.Vibration)
        {
            Taptic.Light();
        }
        switch (GameManager.gameState)
        {
            case GameState.Level:
                EventManager.instance.Fire(new CallBackToSelectLevel());
                break;
            case GameState.SelectLevelMenu:
                EventManager.instance.Fire(new CallBackToMainMenu());
                break;
            case GameState.Info:
                EventManager.instance.Fire(new CallBackToMainMenu());
                break;
            case GameState.Setting:
                EventManager.instance.Fire(new CallBackToMainMenu());
                break;
        }
    }
}
