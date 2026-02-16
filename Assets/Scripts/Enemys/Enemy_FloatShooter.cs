using UnityEngine;

public class Enemy_FloatShooter : Enemy
{

    [SerializeField] float moveSpeed = 1;
    Transform playerCenterTra;

    protected override void E_Start()
    {
        playerCenterTra = PlayerController.instance.GetCenterTra();
    }
    protected override void E_Update(float dtime)
    {
        transform.Translate(Vector3.forward * moveSpeed * dtime);
        transform.LookAt(playerCenterTra);
    }
    protected override void E_Damage(int d)
    {
        
    }
    protected override void E_Death()
    {
        
    }
    public override void E_Destroy()
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
