using System.Collections;
using UnityEngine;
//using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    private static bool Loading;
    private static int LoadingSceneIndex;

    private static readonly int maxSceneIndex = 32;//4debug
    private static int loadedMaxSceneIndex;
    private static int[] loadCount;

    [SerializeField] GameObject setActiveTarget;

    void Awake()
    {
        if (Instance)
        {
            enabled = false;
            gameObject.SetActive(false);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        loadedMaxSceneIndex = 0;
        loadCount = new int[maxSceneIndex];
        for (int i = 0; i < maxSceneIndex; ++i)
        {
            loadCount[i] = 0;
        }
    }

    void OnDestroy()
    {
        if(Instance == this)
        {
            string str = "LoadedSceneCounter\n";
            for (int i = 0; i <= loadedMaxSceneIndex; ++i)
            {
                str += string.Format("{0}: {1}\n",i,loadCount[i]);
            }
            Debug.Log(str);
        }
    }

    public void LoadScene(int index)
    {
        if (Loading) { return; }
        LoadingSceneIndex = index;
        StartCoroutine(ASyncLoad());
    }
    /*public void LoadScene(string sceneName)
    {
        if (Loading) { return; }
        LoadingSceneIndex = SceneManager.GetSceneByName(sceneName).buildIndex;//<-これは対象が違うのでダメ
        if (LoadingSceneIndex < 0)
        {
            Debug.LogErrorFormat("入力された名前のSceneが見つかりません: {0}",sceneName);
            return;
        }
        StartCoroutine(ASyncLoad());
    }*/

    IEnumerator ASyncLoad()
    {
        Loading = true;

        WaitForSeconds setActiveWait = new WaitForSeconds(0.5f);

        setActiveTarget.SetActive(true);

        yield return setActiveWait;

        //Load
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(LoadingSceneIndex, new LoadSceneParameters(LoadSceneMode.Single));
        while (true)
        {
            yield return null;
            if (asyncLoad.isDone)
            {
                break;
            }
        }

        yield return new WaitForSeconds(0.5f);//ラグ回避

        setActiveTarget.SetActive(false);

        yield return setActiveWait;

        loadCount[LoadingSceneIndex]++;
        if (loadedMaxSceneIndex < LoadingSceneIndex)
        {
            loadedMaxSceneIndex = LoadingSceneIndex;
        }
        Loading = false;

        yield break;
    }
}
