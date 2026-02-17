using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTextbox : MonoBehaviour
{
    [SerializeField] TMP_Text textbox;

    public void SetText(string st)
    {
        if(string.IsNullOrWhiteSpace(st))
        {
            gameObject.SetActive(false);
            textbox.text = "";
        }else
        {
            gameObject.SetActive(true);
            textbox.text = st;   
        }
    }
}
