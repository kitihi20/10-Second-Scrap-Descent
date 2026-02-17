
using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] string itemName = "Scrap_01";
    [SerializeField] ParticleSystem attackParticle;

    public bool isFalling {get; private set;}
    public bool isFire {get; private set;}
    float dtime;

    public void Start()
    {
        isFalling = true;
        isFire = false;
        transform.rotation = Quaternion.Euler(0, Random.Range(0,360),0);
    }

    public void Update()
    {
        dtime = Time.deltaTime;

        if(isFalling){ Update_Fall(); }
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
        //transform.position = new Vector3(transform.position.x, parent.position.y, transform.position.z);
        if(transform.localPosition.y < 0)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, Random.Range(0f,1f), transform.localPosition.z);
        }
        isFalling = false;
    }

    public string GetItemName() { return itemName; }
}
