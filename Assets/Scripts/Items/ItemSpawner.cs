using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] itemPrefabs;
    [SerializeField] Transform SpawnArea;

    [SerializeField] int maxItem = 128;
    Item[] fallingItems;

    Vector3 spawnAreaMin;
    Vector3 spawnAreaMax;

    public void Spawn()
    {
        fallingItems = new Item[maxItem];

        spawnAreaMin = SpawnArea.position - SpawnArea.localScale*0.5f;
        spawnAreaMax = SpawnArea.position + SpawnArea.localScale*0.5f;

        for(int i = 0; i < maxItem; ++i)
        {
            int rand = Random.Range(0,itemPrefabs.Length);
            GameObject obj = Instantiate(itemPrefabs[rand], transform);
            obj.SetActive(true);
            Transform tra = obj.transform;
            tra.position = new Vector3(
                Random.Range(spawnAreaMin.x,spawnAreaMax.x),
                Random.Range(spawnAreaMin.y,spawnAreaMax.y),
                Random.Range(spawnAreaMin.z,spawnAreaMax.z)
            );
            Item item = obj.GetComponent<Item>();

            fallingItems[i] = item;
        }
    }

    public void DeleteFreeItems()
    {
        for(int i = 0; i < fallingItems.Length; ++i)
        {
            if(fallingItems[i].GetFalling())
            {
                fallingItems[i].gameObject.SetActive(false);
            }
        }
    }
}
