using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [SerializeField] string enemyName = "enemyName";
    [SerializeField] int maxHealth = 10;


    bool deathFlag;
    [SerializeField] int nowHealth;


    protected abstract void E_Start();
    protected abstract void E_Update(float dtime);
    protected abstract void E_Damage(int d);
    protected abstract void E_Death();
    protected abstract void E_Destroy();

    public abstract Vector3 GetPosition();
    public abstract Transform GetTransform();

    //
    public void E_A_Start()
    {
        deathFlag = false;
        nowHealth = maxHealth;
        E_Start();
    }
    public void E_A_Update(float dtime)
    {
        E_Update(dtime);
    }
    public int E_A_Damage(int d)
    {
        if (IsDead()) { return 0; }
        nowHealth -= d;

        if (nowHealth <= 0)
        {
            deathFlag = true;
            E_Death();
        }
        E_Damage(d);

        return nowHealth;
    }
    public void E_A_Destroy()
    {
        PlayerLog.AddLog(string.Format("DESTROY >>> {0}", enemyName));
        E_Destroy();
    }

    //
    public bool IsDead()
    {
        return deathFlag;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    public int GetNowHealth()
    {
        return nowHealth;
    }
    public string GetName()
    {
        return enemyName;
    }

    //
    void OnParticleCollision(GameObject other)
    {
        Attacker attacker = other.GetComponent<Attacker>();
        if (attacker)
        {
            E_A_Damage(attacker.GetDamage());
        }
    }
}
