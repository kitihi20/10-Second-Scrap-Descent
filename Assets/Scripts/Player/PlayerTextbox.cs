using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerTextbox : MonoBehaviour
{
    [SerializeField] TMP_Text textbox;

    public void SetActive(bool v)
    {
        gameObject.SetActive(v);
    }

    public void SetText(string st)
    {
        textbox.text = st;
    }
}
