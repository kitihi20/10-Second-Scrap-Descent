using UnityEngine;

public class EnemyAreaSpawner : MonoBehaviour
{
    [SerializeField] GameObject enemyPrefab;
    [SerializeField] bool autoSpawn = true;
    [SerializeField] int spawnNum = 4;

    int spawnedNum = 0;

    public bool activated { get; private set; } = false;

    Unity.Mathematics.Random rand;


    void Start()
    {
        rand = new Unity.Mathematics.Random((uint)Random.Range(0,100000));
        spawnedNum = 0;
        
        if (autoSpawn)
        {
            Spawn();
        }
    }

    public void Spawn()
    {
        Vector3 lopos = transform.position - transform.localScale * 0.5f;
        Vector3 hipos = transform.position + transform.localScale * 0.5f;
        for (int i = 0; i < spawnNum; ++i)
        {
            Vector3 pos;
            pos.x = Random.Range(lopos.x, hipos.x);
            pos.y = Random.Range(lopos.y, hipos.y);
            pos.z = Random.Range(lopos.z, hipos.z);
            EnemyManager.Instance.InstantiateEnemy(enemyPrefab, pos, Quaternion.Euler(0,Random.Range(-180,180),0));
            spawnedNum++;
        }
        activated = true;
    }

    public int GetSpawnedNum()
    {
        return spawnedNum;
    }


}