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
    public static BubbleType HeldBubbleType;

    // Start is called before the first frame update
    void Start()
    {
        State = GameState.Play;
        HeldBubbleType = BubbleType.Disappear;
        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnPlace(Place P)
    {
        State = GameState.Show;
    }

    private void OnMotionFinish(MotionFinish M)
    {
        State = GameState.Play;
    }
}

