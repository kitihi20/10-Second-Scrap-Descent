using UnityEngine;

public class Enemy_KAKASHI : Enemy
{
    protected override void E_Start()
    {
        
    }
    protected override void E_Update(float dtime)
    {
        
    }
    protected override void E_Damage(int d)
    {
        
    }
    protected override void E_Death()
    {
        
    }
    protected override void E_Destroy()
    {
        Destroy(gameObject);
    }


    public override Vector3 GetPosition()
    {
        return transform.position;
    }
    public override Transform GetTransform()
    {
        return transform;
    }
}
