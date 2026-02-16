using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [SerializeField] int maxCommonEnemyCount = 1024;
    [SerializeField] int maxBossEnemyCount = 8;

    EnemyList commonEnemys;
    EnemyList bossEnemys;

    readonly static int maybeV2PMaxCount = 3;
    int maybeV2PCount;

    float dtime;
    float nearesttmp;

    void Awake()
    {
        Instance = this;

        commonEnemys = new EnemyList(maxCommonEnemyCount);
        bossEnemys = new EnemyList(maxBossEnemyCount);
    }

    void Update()
    {
        dtime = Time.deltaTime;

        for (int i = commonEnemys.Index; i >= 0; i--)
        {
            if (commonEnemys[i].IsDead())
            {
                Enemy e = commonEnemys.Remove(i);
                e.E_Destroy();
                continue;
            }

            commonEnemys[i].E_A_Update(dtime);

            /*if(commonEnemys[i].IsDead())
            {
                N2M4_I_Enemy e = commonEnemys.Remove(i);
                e.Destroy();
                continue;
            }*/

            /*if (i % maybeV2PMaxCount == maybeV2PCount)
            { 

            }*/
        }
        /*for(int i = bossEnemys.Index; i >= 0; i--)
        {

        }*/
    }

    public Enemy InstantiateEnemy(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if(commonEnemys.GetRemainingSpaces() <= 0)
        {
            return null;
        }

        GameObject obj = Instantiate(prefab,position,rotation,transform);
        Enemy e = obj.GetComponent<Enemy>();
        e.E_A_Start();

        commonEnemys.Add(e);
        
        return e;
    }

    public Enemy GetNearestAngleEnemy(Vector3 pos, Vector3 vec, float maxdistance, LayerMask levellayer, bool excludeBackward = true)
    {
        Enemy nearestenemy = null;
        nearesttmp = excludeBackward ? 0 : -2;//-1: 360  0: 180  1: 0
        for(int i = commonEnemys.Index; i >= 0; i--)
        {
            if (commonEnemys[i].IsDead()) { continue; }
            //if (!commonEnemys[i].IsVisible()) { continue; }

            Vector3 enemypos = commonEnemys[i].GetPosition();
            Vector3 enemydiff = enemypos - pos; 
            float enemydist = enemydiff.magnitude;
            if (enemydist > maxdistance) { continue; }

            Vector3 enemyvec = enemydiff / enemydist;
            float dot = Vector3.Dot(vec, enemyvec);
            if (dot < nearesttmp) { continue; }
            


            bool rayres = Physics.Linecast(pos, enemypos, levellayer);//
            if (rayres) { continue; }
            
            nearesttmp = dot;
            nearestenemy = commonEnemys[i];
        }

        return nearestenemy;
    }

    public int GetNowCommonEnemyCount()
    {
        return commonEnemys.Count;
    }
}
