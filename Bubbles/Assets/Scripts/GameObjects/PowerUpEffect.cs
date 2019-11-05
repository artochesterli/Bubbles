using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpEffect : MonoBehaviour
{
    public float InitScale;
    public float EndScale;
    public float PlayTime;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Animation());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Animation()
    {
        float TimeCount = 0;
        transform.localScale = Vector3.one * InitScale;
        Color color = GetComponent<SpriteRenderer>().color;
        while (TimeCount < PlayTime)
        {
            TimeCount += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one * InitScale, Vector3.one * EndScale, TimeCount / PlayTime);
            GetComponent<SpriteRenderer>().color = Color.Lerp(new Color(color.r, color.g, color.b, 0), new Color(color.r, color.g, color.b, 1), TimeCount / PlayTime);
            yield return null;
        }
        Destroy(gameObject);
    }
}
