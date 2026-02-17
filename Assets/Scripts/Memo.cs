
//Scene上に置いておくメモ用のスクリプト

using UnityEngine;

public class Memo : MonoBehaviour
{
    [TextArea(3,5)]
    [SerializeField] string memoTexts;
}
