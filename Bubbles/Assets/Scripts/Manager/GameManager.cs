using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public static int CurrentLevel;

    public int MinLevelIndex;
    public int MaxLevelIndex;

    public GameObject Mask;
    public float MaskFadeTime;

    private GameObject AllLevel;
    private List<GameObject> SortedLevelList;
    private GameObject CopiedLevel;

    // Start is called before the first frame update
    void Start()
    {
        Init();

        EventManager.instance.AddHandler<Place>(OnPlace);
        EventManager.instance.AddHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.AddHandler<LevelFinish>(OnLevelFinish);
    }

    private void OnDestroy()
    {
        EventManager.instance.RemoveHandler<Place>(OnPlace);
        EventManager.instance.RemoveHandler<MotionFinish>(OnMotionFinish);
        EventManager.instance.RemoveHandler<LevelFinish>(OnLevelFinish);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(LoadLevel(CurrentLevel + 1));
            CurrentLevel++;
        }
    }

    private void Init()
    {
        State = GameState.Play;
        SortedLevelList = new List<GameObject>();
        GetLevelInfo();
        CopiedLevel = Instantiate(SortedLevelList[CurrentLevel-MinLevelIndex]);
        CopiedLevel.transform.parent = AllLevel.transform;
        CopiedLevel.SetActive(false);
        EventManager.instance.Fire(new LevelLoaded(CurrentLevel));
        StartCoroutine(ScreenAppear());
    }

    private void GetLevelInfo()
    {
        List<int> IndexList = new List<int>();
        AllLevel = GameObject.Find("AllLevel").gameObject;
        foreach(Transform child in AllLevel.transform)
        {
            SortedLevelList.Add(child.gameObject);
            IndexList.Add(child.GetComponent<LevelManager>().LevelIndex);
        }

        for(int i = 0; i < IndexList.Count; i++)
        {
            for(int j = 0; j < IndexList.Count - i - 1; j++)
            {
                if (IndexList[j] > IndexList[j + 1])
                {
                    int temp = IndexList[j];
                    IndexList[j] = IndexList[j + 1];
                    IndexList[j + 1] = temp;
                    GameObject g = SortedLevelList[j];
                    SortedLevelList[j] = SortedLevelList[j + 1];
                    SortedLevelList[j + 1] = g;
                }
            }
        }

        for(int i = 0; i < SortedLevelList.Count; i++)
        {
            if (SortedLevelList[i].activeSelf)
            {
                CurrentLevel = IndexList[i];
            }
        }

    }

    private void OnPlace(Place P)
    {
        State = GameState.Show;
    }

    private void OnMotionFinish(MotionFinish M)
    {
        State = GameState.Play;
    }

    private void OnLevelFinish(LevelFinish L)
    {
        State = GameState.Show;
        if (L.Success)
        {
            StartCoroutine(LoadLevel(L.Index + 1));
        }
        else
        {
            StartCoroutine(LoadLevel(L.Index));
        }
    }

    private IEnumerator LoadLevel(int index)
    {
        if (index <= MaxLevelIndex)
        {
            yield return StartCoroutine(ScreenFade());

            if (index == CurrentLevel)
            {
                Destroy(SortedLevelList[index - MinLevelIndex]);
                SortedLevelList[index - MinLevelIndex] = CopiedLevel;
                CopiedLevel.SetActive(true);
                CopiedLevel = Instantiate(CopiedLevel);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
            }
            else
            {
                Destroy(SortedLevelList[CurrentLevel - MinLevelIndex]);
                SortedLevelList[CurrentLevel- MinLevelIndex] = CopiedLevel;
                CopiedLevel = Instantiate(SortedLevelList[index - MinLevelIndex]);
                CopiedLevel.transform.parent = AllLevel.transform;
                CopiedLevel.SetActive(false);
                SortedLevelList[index - MinLevelIndex].SetActive(true);
            }
            
            CurrentLevel = index;
            State = GameState.Play;
            EventManager.instance.Fire(new LevelLoaded(CurrentLevel));

            yield return StartCoroutine(ScreenAppear());
        }
    }

    private IEnumerator ScreenFade()
    {
        float TimeCount = 0;
        while (TimeCount < MaskFadeTime)
        {
            TimeCount += Time.deltaTime;
            Mask.GetComponent<Image>().color = Color.Lerp(new Color(0, 0, 0, 0), Color.black, TimeCount / MaskFadeTime);
            yield return null;
        }
    }

    private IEnumerator ScreenAppear()
    {
        float TimeCount = 0;
        while (TimeCount < MaskFadeTime)
        {
            TimeCount += Time.deltaTime;
            Mask.GetComponent<Image>().color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), TimeCount / MaskFadeTime);
            yield return null;
        }
    }

}

