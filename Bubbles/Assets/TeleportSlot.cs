using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportSlot : MonoBehaviour
{
    public float FillRotationSpeed;
    public GameObject Fill;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.levelState == LevelState.Play)
        {
            Fill.transform.Rotate(new Vector3(0, 0, -FillRotationSpeed * Time.deltaTime));
        }
    }
}
