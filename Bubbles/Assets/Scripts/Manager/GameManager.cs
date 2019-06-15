using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Play,
    Show,
    Pause,
}

public class GameManager : MonoBehaviour
{
    public static GameState State;

    // Start is called before the first frame update
    void Start()
    {
        State = GameState.Play;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
