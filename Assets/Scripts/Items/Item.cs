
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] string itemName = "Scrap_01";
    [SerializeField] ParticleSystem attackParticle;

    bool isFall;
    bool isFire;
    float dtime;

    public void Start()
    {
        isFall = true;
        isFire = false;
        transform.rotation = Quaternion.Euler(0, Random.Range(0,360),0);
    }

    public void Update()
    {
        dtime = Time.deltaTime;

        if(isFall){ Update_Fall(); }
    }

    void Update_Fall()
    {
        transform.Translate(-Vector3.up * 9 * dtime);
        transform.Rotate(new Vector3(0,180 * dtime,0));
    }

    public void SetFire(bool v)
    {
        isFire = v;
        if(isFire)
        {
            attackParticle.Play();
        }else
        {
            attackParticle.Stop();
        }
    }
    public void Catch(Transform parent)
    {
        transform.SetParent(parent);
        transform.position = new Vector3(transform.position.x, parent.position.y, transform.position.z);
        isFall = false;
    }

    public bool GetFalling()
    {
        return isFall;
    }
}
