using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{

    [Header("Fall")]
    [SerializeField] GameObject fallParentObj;
    [SerializeField] float maxHeight = 1000;
    [SerializeField] RectTransform fall_height_now;
    [SerializeField] RectTransform fall_height_high;
    [SerializeField] RectTransform fall_height_low;
    [SerializeField] TMP_Text fall_height_text;

    [Header("Battle")]
    [SerializeField] GameObject battleParentObj;

    [Header("Textbox")]
    [SerializeField] PlayerTextbox playerTextbox;

    [Header("Log")]
    [SerializeField] PlayerLogTextbox playerLogText;

    [Header("param")]
    [SerializeField] PlayerState nowState;


    Transform playerCenterTra;


    void Start()
    {
        playerCenterTra = PlayerController.instance.GetCenterTra();
    }

    void Update()
    {
        switch(nowState)
        {
            case PlayerState.fall:
                Update_Fall();
            break;
            case PlayerState.battle:
                Update_Battle();
            break;
        }
    }

    void Update_Fall()
    {
        float playerHeight = playerCenterTra.position.y;
        float fallRatio = playerHeight / maxHeight;
        fall_height_now.position = Vector3.Lerp(fall_height_low.position, fall_height_high.position, fallRatio);

        fall_height_text.text = string.Format("{0:0000}m",playerHeight);
    }

    void Update_Battle()
    {
        
    }


    public void SetState(PlayerState s)
    {
        nowState = s;

        switch(nowState)
        {
            case PlayerState.idle:
                fallParentObj.SetActive(false);
                battleParentObj.SetActive(false);
                playerLogText.gameObject.SetActive(false);
            break;
            case PlayerState.fall:
                fallParentObj.SetActive(true);
                battleParentObj.SetActive(false);
                playerLogText.gameObject.SetActive(true);
            break;
            case PlayerState.battle:
                fallParentObj.SetActive(false);
                battleParentObj.SetActive(true);
                playerLogText.gameObject.SetActive(true);
            break;
        }
    }

    public void SetText(string s)
    {
        playerTextbox.SetText(s);
    }
}
