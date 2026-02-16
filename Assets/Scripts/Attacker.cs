using UnityEngine;

public class Attacker : MonoBehaviour
{
    [SerializeField] int damage = 1;

    public int GetDamage(){ return damage; }
}
