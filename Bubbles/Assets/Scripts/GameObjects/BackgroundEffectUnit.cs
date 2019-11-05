using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundEffectUnit : MonoBehaviour
{
    public float AppearFadeTime;
    public float StableTime;
    public float MaxAlpha;
    public float Speed;
    public Vector2 Direction;
    public float Size;

    public Color color;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Life());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetAttribute(float appearfadetime,float stabletime,float maxalpha,float speed, float size, Vector2 dir)
    {
        AppearFadeTime = appearfadetime;
        StableTime = stabletime;
        MaxAlpha = maxalpha;
        Speed = speed;
        Size = size;
        Direction = dir;
    }

    private IEnumerator Life()
    {
        

        float TimeCount = 0;

        while (TimeCount < AppearFadeTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 0), new Color(color.r, color.g, color.b, MaxAlpha), TimeCount / AppearFadeTime);
            transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one * Size, TimeCount / AppearFadeTime);
            transform.position += Speed * (Vector3)Direction*Time.deltaTime;
            yield return null;
        }

        TimeCount = 0;

        while (TimeCount < StableTime)
        {
            TimeCount += Time.deltaTime;
            transform.position += Speed * (Vector3)Direction*Time.deltaTime;
            yield return null;
        }


        TimeCount = 0;

        while (TimeCount < AppearFadeTime)
        {
            TimeCount += Time.deltaTime;
            GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, MaxAlpha), new Color(color.r, color.g, color.b, 0), TimeCount / AppearFadeTime);
            transform.localScale = Vector3.Lerp(Vector3.one * Size, Vector3.zero, TimeCount / AppearFadeTime);
            transform.position += Speed * (Vector3)Direction * Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
