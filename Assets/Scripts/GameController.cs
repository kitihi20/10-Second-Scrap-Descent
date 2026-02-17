using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("player")]
    [SerializeField] PlayerController player;
    [SerializeField] PlayerCameraController playerCamera;
    [SerializeField] PlayerUI playerUI;

    [Header("items")]
    [SerializeField] ItemSpawner items_spawner;

    [Header("enemys")]
    [SerializeField] EnemyAreaSpawner[] enemy_spawner1;
    [SerializeField] EnemyAreaSpawner[] enemy_spawner2;

    [Header("level")]
    [SerializeField] Transform titleCameraPos;
    [SerializeField] GameObject titleCanvas;
    [SerializeField] GameObject fallAreas;
    [SerializeField] Transform fallStartPosition;
    [SerializeField] GameObject battleAreas;
    [SerializeField] Transform battleStartPosition;

    [SerializeField] Transform endingCameraPos;

    [SerializeField] GameObject resultCanvas;
    [SerializeField] TMP_Text resultText;


    [Header("param")]
    [SerializeField] GameState nowState;


    float dtime;

    PlayerInput plInput;
    
    void Start()
    {
        plInput = PlayerInput.instance;
        StartCoroutine(GameFlow());
    }

    void Update()
    {
        dtime = Time.deltaTime;
    }

    void SetState(GameState s, bool forceMode=false)
    {
        if(nowState == s && !forceMode){ return; }
        nowState = s;

        switch(nowState)
        {
            case GameState.title:
                //player
                player.SetPlayerMode(PlayerState.idle);
                player.SetPlayerPosition(Vector3.zero);
                playerCamera.SetIdleTransform(titleCameraPos);
                playerCamera.SetTarget(player.GetCenterTra());
                playerCamera.SetMode(PlayerState.idle);
                //ui
                titleCanvas.SetActive(true);
                resultCanvas.SetActive(false);
                playerUI.SetState(PlayerState.idle);
                playerUI.SetText("");
                //sys
                fallAreas.SetActive(false);
                battleAreas.SetActive(false);
                PlayerLog.ResetLog();
            break;
            case GameState.introduction:
                //player
                //ui
                titleCanvas.SetActive(false);
                //sys
            break;
            case GameState.fall:
                //player
                player.SetPlayerMode(PlayerState.fall);
                player.SetPlayerPosition(fallStartPosition.position);
                playerCamera.SetMode(PlayerState.fall);
                //ui
                playerUI.SetState(PlayerState.fall);
                //sys
                fallAreas.SetActive(true);
                battleAreas.SetActive(false);
            break;
            case GameState.annihilation:
                //player
                player.SetPlayerMode(PlayerState.battle);
                player.SetPlayerPosition(battleStartPosition.position);
                playerCamera.SetMode(PlayerState.battle);
                //ui
                playerUI.SetState(PlayerState.battle);
                //sys
                fallAreas.SetActive(false);
                battleAreas.SetActive(true);
            break;
            case GameState.boss:
                //player
                playerCamera.SetMode(PlayerState.battle);
                //ui
                playerUI.SetState(PlayerState.battle);
                //sys
            break;
            case GameState.ending:
                //player
                playerCamera.SetIdleTransform(endingCameraPos);
                playerCamera.SetMode(PlayerState.idle);
                //ui
                playerUI.SetState(PlayerState.idle);
                //sys
            break;
            case GameState.result:
                //player
                playerCamera.SetIdleTransform(endingCameraPos);
                playerCamera.SetMode(PlayerState.idle);
                //ui
                playerUI.SetState(PlayerState.idle);
                resultCanvas.SetActive(true);
                //sys
            break;
        }
    }

    IEnumerator GameFlow()
    {
        yield return new WaitForSeconds(0.1f);

        //ーーーー title ーーーー
        SetState(GameState.title, true);
        while(true)
        {
            if(plInput.GetContinueDown())
            {
                break;
            }
            yield return null;
        }


        //ーーーー introduction ーーーー
        SetState(GameState.introduction);

        playerUI.SetText("輸送機大破!\n迎撃システム作動まで残り10秒!");
        yield return new WaitForSeconds(2);
        playerUI.SetText("拾えるものは何でも拾え!\n着地と同時に戦闘開始だ!");
        yield return new WaitForSeconds(3);

        //ーーーー fall ーーーー
        SetState(GameState.fall);

        items_spawner.Spawn();

        playerUI.SetText("空中のゴミは早い者勝ちだ!\n左右に動いて体当たりしろ!");
        yield return new WaitForSeconds(3);
        playerUI.SetText("");

        //yield return new WaitForSeconds(12);
        yield return new WaitForSeconds(11.8f);

        //ーーーー annihilation ーーーー
        SetState(GameState.annihilation);

        items_spawner.DeleteFreeItems();
        for(int i = 0; i < enemy_spawner1.Length; ++i)
        {
            enemy_spawner1[i].Spawn();
        }

        playerUI.SetText("着地成功!!\nそのクソ武装で敵をブチ抜け!!");
        yield return new WaitForSeconds(2);
        playerUI.SetText("");

        yield return new WaitForSeconds(8);

        //ーーーー boss ーーーー
        SetState(GameState.boss);

        for(int i = 0; i < enemy_spawner2.Length; ++i)
        {
            enemy_spawner2[i].Spawn();
        }

        playerUI.SetText("全武装、リミッター解除!\n撃ちまくれ!");
        yield return new WaitForSeconds(2);
        playerUI.SetText("");

        yield return new WaitForSeconds(8);

        int allEnemyCount = 0;
        for(int i = 0; i < enemy_spawner1.Length; ++i)
        {
            allEnemyCount += enemy_spawner1[i].GetSpawnedNum();
        }
        for(int i = 0; i < enemy_spawner2.Length; ++i)
        {
            allEnemyCount += enemy_spawner2[i].GetSpawnedNum();
        }

        int playerItemCount = player.GetItemCount();
        int killcount = allEnemyCount - EnemyManager.Instance.GetNowCommonEnemyCount();
        string rank = "作戦失敗。……まあ、あんなアセンじゃ当然か。";
        if(killcount > (allEnemyCount*0.7f)) { rank = "歴史に刻め。この10秒間の奇跡を。"; }

        resultText.text = string.Format("回収武器数: {0}\n撃破敵数: {1}\n\n{2}",playerItemCount,killcount,rank);

        //ーーーー ending ーーーー
        SetState(GameState.ending);

        playerUI.SetText("...");
        yield return new WaitForSeconds(0.1f);
        playerUI.SetText("");

        //yield return new WaitForSeconds(8);

        //ーーーー result ーーーー
        SetState(GameState.result);

        while(true)
        {
            if(plInput.GetContinueDown())
            {
                SceneLoader.Instance.LoadScene(0);
                break;
            }
            yield return null;
        }

        yield break;
    }
}
