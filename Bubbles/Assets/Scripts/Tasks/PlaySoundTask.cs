using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundTask : Task
{
    private readonly GameObject Obj;
    private readonly AudioSource Source;

    public PlaySoundTask(GameObject obj, AudioSource source)
    {
        Obj = obj;
        Source = source;
    }


}
