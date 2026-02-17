using UnityEngine;

public class Enemy_FloatShooter : Enemy
{

    [SerializeField] float moveSpeed = 1;
    [SerializeField] float bounceTime = 1;
    [SerializeField] float bounceSpeed = 10;
    Transform playerCenterTra;

    float bounceTime_now;
    Vector3 bounceVec;

    protected override void E_Start()
    {
        playerCenterTra = PlayerController.instance.GetCenterTra();
    }
    protected override void E_Update(float dtime)
    {
        Vector3 movevec = Vector3.zero;
        
        transform.LookAt(playerCenterTra);

        if((GetPosition() - playerCenterTra.position).sqrMagnitude > 0.5f)
        {
            movevec += transform.forward * moveSpeed * dtime;
        }

        if(bounceTime_now > 0)
        {
            movevec += bounceVec * (bounceTime_now/bounceTime*bounceSpeed) * dtime;
            bounceTime_now -= dtime;
        }

        transform.position += movevec;
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

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("bounce!");
        bounceTime_now = bounceTime;
        bounceVec = (GetPosition()-collision.GetContact(0).point).normalized;
    }
}
