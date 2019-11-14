using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsideParticle
{
    public GameObject Obj;
    public float LifeTimeCount;

    public InsideParticle(GameObject obj)
    {
        Obj = obj;
    }
}

public class TargetSlotObject : MonoBehaviour
{
    public GameObject InsideParticlePrefab;

    public float BreathCycle;
    public float BreathMaxSize;


    public float ParticleGenerateIntervalMin;
    public float ParticleGenerateIntervalMax;
    public float ParticleGenerationZoneSize;
    public float ParticleGenerationZoneEdgeWidth;

    public float ParticleMinSize;
    public float ParticleMaxSize;
    public float ParticleMaxAlpha;
    public float ParticleLifeTime;
    public float ParticleStartFadeProportion;

    public float ParticleGoOutTime;
    public float ParticleGoOutDisMin;
    public float ParticleGoOutDisMax;
    public float ParticleGoOutDelay;


    private float ParticleGenerationTimeCount;
    private float CurrentGenerationTime;
    private List<InsideParticle> InsideParticles;
    private bool BubbleInside;


    private float BreathTimer;
    private bool BreathInflate;
    // Start is called before the first frame update
    void Start()
    {
        InsideParticles = new List<InsideParticle>();
    }

    private void OnDestroy()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!BubbleInside)
        {
            //UpdateEffect();
            //GenerateParticles();
        }

        if(GameManager.levelState == LevelState.Play || GameManager.levelState == LevelState.Executing)
        {
            //Breath();
        }
    }

    private void Breath()
    {
        BreathTimer += Time.deltaTime;
        if (BreathInflate)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * BreathMaxSize, BreathTimer / (BreathCycle / 2));
            if(BreathTimer >= BreathCycle / 2)
            {
                BreathTimer = 0;
                BreathInflate = false;
            }
        }
        else
        {
            transform.localScale = Vector3.Lerp(Vector3.one * BreathMaxSize, Vector3.one,  BreathTimer / (BreathCycle / 2));
            if (BreathTimer >= BreathCycle / 2)
            {
                BreathTimer = 0;
                BreathInflate = true;
            }
        }

        //transform.localScale = Vector3.one * (1 + (BreathMaxSize - 1)/2 + (BreathMaxSize - 1) / 2 * Mathf.Sin(BreathTimer / BreathCycle * 2 * Mathf.PI));
    }

    public List<InsideParticle> GetInsideParticles()
    {
        return InsideParticles;
    }

    public void SetBubbleInside(bool value)
    {
        BubbleInside = value;
    }

    public void ClearInsideParticleInfo()
    {
        ParticleGenerationTimeCount = 0;
        InsideParticles.Clear();
    }

    private void GenerateParticles()
    {
        ParticleGenerationTimeCount -= Time.deltaTime;
        if (ParticleGenerationTimeCount<=0)
        {
            Vector2 Zone = new Vector2(-ParticleGenerationZoneSize / 2, ParticleGenerationZoneSize / 2);

            Vector2 Pos = Utility.RandomPosFromEdgeArea(Zone,Zone, new Vector2(-ParticleGenerationZoneSize / 2 + ParticleGenerationZoneEdgeWidth, ParticleGenerationZoneSize / 2 - ParticleGenerationZoneEdgeWidth), new Vector2(-ParticleGenerationZoneSize / 2 + ParticleGenerationZoneEdgeWidth, ParticleGenerationZoneSize / 2 - ParticleGenerationZoneEdgeWidth));

            GameObject Particle = (GameObject)Instantiate(InsideParticlePrefab, transform);
            Particle.transform.position = (Vector2)transform.position + Pos;
            Particle.transform.localScale = Vector3.one * Random.Range(ParticleMinSize,ParticleMaxSize);
            Particle.transform.eulerAngles = new Vector3(0, 0, Random.Range(0f, 360f));
            Color color = Particle.GetComponent<SpriteRenderer>().color;
            Particle.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, 0);
            InsideParticles.Add(new InsideParticle(Particle));

            ParticleGenerationTimeCount = Random.Range(ParticleGenerateIntervalMin, ParticleGenerateIntervalMax);
        }
    }

    private void UpdateEffect()
    {
        List<InsideParticle> RemoveList = new List<InsideParticle>();

        for(int i = 0; i < InsideParticles.Count; i++)
        {
            InsideParticles[i].LifeTimeCount += Time.deltaTime;
            GameObject Obj = InsideParticles[i].Obj;
            //Obj.transform.localScale = Vector3.one * Mathf.Lerp(0, ParticleMaxSize, InsideParticles[i].LifeTimeCount/ParticleLifeTime);
            Color color = Obj.GetComponent<SpriteRenderer>().color;

            if(InsideParticles[i].LifeTimeCount <= ParticleLifeTime*ParticleStartFadeProportion)
            {
                Obj.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(0, ParticleMaxAlpha, InsideParticles[i].LifeTimeCount/ (ParticleLifeTime * ParticleStartFadeProportion)));
            }
            else
            {
                Obj.GetComponent<SpriteRenderer>().color = new Color(color.r, color.g, color.b, Mathf.Lerp(ParticleMaxAlpha, 0, (InsideParticles[i].LifeTimeCount- ParticleLifeTime * ParticleStartFadeProportion) / (ParticleLifeTime * (1 - ParticleStartFadeProportion))));
            }

            if(InsideParticles[i].LifeTimeCount >= ParticleLifeTime)
            {
                RemoveList.Add(InsideParticles[i]);
            }
        }

        for(int i = 0; i < RemoveList.Count; i++)
        {
            Destroy(RemoveList[i].Obj);
            InsideParticles.Remove(RemoveList[i]);
        }
    }
}
