using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameController : MonoBehaviour
{
    [Header("player")]
    [SerializeField] PlayerTextbox player_textbox;
    [SerializeField] PlayerController player;
    [SerializeField] PlayerCameraController playerCamera;

    [Header("items")]
    [SerializeField] ItemSpawner items_spawner;

    [Header("enemys")]
    [SerializeField] EnemyAreaSpawner enemy_spawner1;
    [SerializeField] EnemyAreaSpawner enemy_spawner2;

    [Header("level")]
    [SerializeField] Transform titleCameraPos;
    [SerializeField] GameObject titleCanvas;
    [SerializeField] GameObject gameCanvas;
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

    public enum GameState
    {
        title,
        introduction,
        fall,
        annihilation,
        boss,
        ending,
        result
    }
    
    void Start()
    {
        plInput = PlayerInput.instance;
        StartCoroutine(GameFlow());
    }

    void Update()
    {
        dtime = Time.deltaTime;
    }

    void SetState(GameState s)
    {
        if(nowState == s){ return; }
        nowState = s;

        switch(nowState)
        {
            case GameState.title:
                titleCanvas.SetActive(true);
                gameCanvas.SetActive(false);
                resultCanvas.SetActive(false);
                player_textbox.SetActive(false);
                player.SetPlayerMode(PlayerController.PlayerState.idle);
                player.SetPlayerPosition(Vector3.zero);
                playerCamera.SetIdleTransform(titleCameraPos);
                playerCamera.SetTarget(player.GetCenterTra());
                playerCamera.SetMode(PlayerCameraController.CameraMode.idle);
                fallAreas.SetActive(false);
                battleAreas.SetActive(false);
            break;
            case GameState.introduction:
            titleCanvas.SetActive(false);
                
            break;
            case GameState.fall:
                gameCanvas.SetActive(true);
                player.SetPlayerMode(PlayerController.PlayerState.fall);
                player.SetPlayerPosition(fallStartPosition.position);
                fallAreas.SetActive(true);
                battleAreas.SetActive(false);
                items_spawner.Spawn();
            break;
            case GameState.annihilation:
                player.SetPlayerMode(PlayerController.PlayerState.battle);
                player.SetPlayerPosition(battleStartPosition.position);
                fallAreas.SetActive(false);
                battleAreas.SetActive(true);
            break;
            case GameState.ending:
                gameCanvas.SetActive(false);
                playerCamera.SetIdleTransform(endingCameraPos);
                playerCamera.SetMode(PlayerCameraController.CameraMode.idle);
            break;
            case GameState.result:
                resultCanvas.SetActive(true);
            break;
        }
    }

    IEnumerator GameFlow()
    {
        yield return new WaitForSeconds(0.1f);

        //title
        SetState(GameState.title);
        while(true)
        {
            if(plInput.GetContinueDown())
            {
                break;
            }
            yield return null;
        }


        //introduction
        SetState(GameState.introduction);

        player_textbox.SetActive(true);
        player_textbox.SetText("輸送機大破!\n迎撃システム作動まで残り10秒!");
        yield return new WaitForSeconds(2);
        player_textbox.SetText("拾えるものは何でも拾え!\n着地と同時に戦闘開始だ!");
        yield return new WaitForSeconds(3);

        //fall
        SetState(GameState.fall);
        playerCamera.SetMode(PlayerCameraController.CameraMode.fall);

        player_textbox.SetActive(true);
        player_textbox.SetText("空中のゴミは早い者勝ちだ!\n左右に動いて体当たりしろ!");
        yield return new WaitForSeconds(2);
        player_textbox.SetActive(false);

        yield return new WaitForSeconds(8);

        //annihilation
        SetState(GameState.annihilation);
        playerCamera.SetMode(PlayerCameraController.CameraMode.battle);

        items_spawner.DeleteFreeItems();
        enemy_spawner1.Spawn();

        player_textbox.SetActive(true);
        player_textbox.SetText("着地成功!!\nそのクソ武装で敵をブチ抜け!!");
        yield return new WaitForSeconds(2);
        player_textbox.SetActive(false);

        yield return new WaitForSeconds(8);

        //boss
        SetState(GameState.boss);
        playerCamera.SetMode(PlayerCameraController.CameraMode.battle);

        enemy_spawner2.Spawn();

        player_textbox.SetActive(true);
        player_textbox.SetText("全武装、リミッター解除!\n撃ちまくれ!");
        yield return new WaitForSeconds(2);
        player_textbox.SetActive(false);

        yield return new WaitForSeconds(8);

        int playerItemCount = player.GetItemCount();
        int killcount = (64 + 64) - EnemyManager.Instance.GetNowCommonEnemyCount();
        string rank = "作戦失敗。……まあ、あんなアセンじゃ当然か。";
        if(killcount > 80) { rank = "歴史に刻め。この10秒間の奇跡を。"; }

        resultText.text = string.Format("回収武器数: {0}\n撃破敵数: {1}\n\n{2}",playerItemCount,killcount,rank);

        //ending
        SetState(GameState.ending);
        playerCamera.SetIdleTransform(titleCameraPos);
        playerCamera.SetMode(PlayerCameraController.CameraMode.idle);

        player_textbox.SetActive(true);
        player_textbox.SetText("...");
        yield return new WaitForSeconds(0.1f);
        player_textbox.SetActive(false);

        //yield return new WaitForSeconds(8);

        //result
        SetState(GameState.result);
        playerCamera.SetIdleTransform(titleCameraPos);
        playerCamera.SetMode(PlayerCameraController.CameraMode.idle);

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
