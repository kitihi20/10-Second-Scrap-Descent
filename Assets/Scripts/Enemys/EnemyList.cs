using UnityEngine;

public class EnemyList
{
    public int maxObjectAmount { get; private set; }//最大格納数
    public int Index { get; private set; }//現在使用している最上箇所
    public int Count { get { return Index + 1;} }//格納数

    Enemy[] objectArray;

    public Enemy this[int index]
    {
        get 
        {
            return objectArray[index];
        }
    }

    public EnemyList(int max)
    {
        maxObjectAmount = max;
        objectArray = new Enemy[maxObjectAmount];
        Index = -1;
    }

    public int GetRemainingSpaces()
    {
        return maxObjectAmount - Count;
    }

    public bool Add(Enemy obj)
    {
        if (Index >= maxObjectAmount)
        {
            return false;
        }

        Index++;
        objectArray[Index] = obj;
        return true;
    }

    public Enemy Remove(int index)
    {
        if (index < 0 || index >= maxObjectAmount) { return null; }
        Enemy tmpObj = objectArray[index];
        objectArray[index] = objectArray[Index];
        objectArray[Index] = null;
        Index--;
        return tmpObj;
    }

    public void Clear()
    {
        for (int i = Index; i >= 0; i--)
        {
            objectArray[i] = null;
        }
        Index = -1;
    }
}
