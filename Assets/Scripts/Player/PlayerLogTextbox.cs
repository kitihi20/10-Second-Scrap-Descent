using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class PlayerLogTextbox : MonoBehaviour
{
    [SerializeField] TMP_Text text;

    void Start()
    {
        PlayerLog.logUpdateFunc += UpdateText;
        text.text = "";
    }

    void OnDestroy()
    {
        PlayerLog.logUpdateFunc -= UpdateText;
    }

    void UpdateText(string s, int newCharCount)
    {
        text.text = s;
    }
}
