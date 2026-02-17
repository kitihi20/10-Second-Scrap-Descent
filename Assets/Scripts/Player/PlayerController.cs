using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{   
    public static PlayerController instance;


    [Header("Params")]
    [SerializeField] float airSpeed = 10;
    [SerializeField] float groundSpeed = 10;
    [SerializeField] float fallGravity = -9;
    [SerializeField] float gravity = -9;
    [SerializeField] LayerMask groundLayer = 0b0001;

    [Header("Components")]
    [SerializeField] PlayerCameraController cam;
    [SerializeField] Transform modelTra;
    [SerializeField] Transform centerTra;
    [SerializeField] Transform groundPosition;
    [SerializeField] Rigidbody rb;
    [SerializeField] Material limitWallMaterial;
    
    int playerPosition_hash;
    float dtime;
    
    PlayerState nowState;
    PlayerInput plInput;
    List<Item> gotItems;

    Vector3 moveVector;

    [SerializeField] bool onGround;
    Collider[] gHitColliders;

    readonly Vector3 groundHitBox = new Vector3(0.75f, 0.2f, 0.75f);//地面判定の検出範囲の大きさ
    readonly float damping_GroundGravity = 4;//地面にいるときの重力の減少量
    readonly float groundGraviy = -1;//地面にいるときの重力



    void Awake()
    {
        instance = this;
        gotItems = new List<Item>(128);
        playerPosition_hash = Shader.PropertyToID("_PlayerPosition");
    }

    void Start()
    {
        plInput = PlayerInput.instance;
    }

    void Update()
    {
        dtime = Time.deltaTime;

        if(transform.position.y < -10)
        {
            SetPlayerPosition(new Vector3(0,1,0));
        }

        limitWallMaterial.SetVector(playerPosition_hash, GetCenterTra().position);

        switch(nowState)
        {
            case PlayerState.idle:
                Update_Idle();
            break;
            case PlayerState.fall:
                Update_Fall();
            break;
            case PlayerState.battle:
                GetGround();
                Update_Battle();
            break;
        }
    }

    void GetGround()
    {
        int hitnum = Physics.OverlapBoxNonAlloc(groundPosition.position, groundHitBox * 0.5f, gHitColliders, groundPosition.rotation, groundLayer, QueryTriggerInteraction.Ignore);
        onGround = hitnum > 0;
    }

    void Update_Idle()
    {
        
    }

    void Update_Fall()
    {
        Vector2 inputvec = plInput.GetMoveVector();
        Vector3 movevec = Quaternion.Euler(0,cam.GetYRot(),0) * new Vector3(inputvec.x, 0, inputvec.y) * groundSpeed;

        moveVector.x = Mathf.Lerp(moveVector.x, movevec.x, dtime * 10);
        moveVector.z = Mathf.Lerp(moveVector.z, movevec.z, dtime * 10);

        moveVector.y += Time.deltaTime * fallGravity;

        //modelTra.rotation = Quaternion.Lerp(modelTra.rotation, Quaternion.Euler(90,0,0), dtime*4);
        modelTra.rotation = Quaternion.Lerp(modelTra.rotation, Quaternion.Euler(0,0,0), dtime*4);
        centerTra.rotation = modelTra.rotation;
    }

    void Update_Battle()
    {
        Vector2 inputvec = plInput.GetMoveVector();
        Vector3 movevec = Quaternion.Euler(0,cam.GetYRot(),0) * new Vector3(inputvec.x, 0, inputvec.y) * groundSpeed;

        moveVector.x = movevec.x;
        moveVector.z = movevec.z;

        moveVector.y += Time.deltaTime * gravity;

        if (onGround)
        {
            if (moveVector.y < groundGraviy)
            {
                moveVector.y = groundGraviy;
            }
        }

        Vector3 targetPos = cam.getCameraPos() + cam.getCameraForward() * 100;
        RaycastHit hit;
        bool ishit = Physics.Raycast(cam.getCameraPos(), cam.getCameraForward(),out hit , 1000, groundLayer);
        if(ishit){ targetPos = hit.point; }

        for(int i = 0; i < gotItems.Count; ++i)
        {
            gotItems[i].transform.LookAt(targetPos);
        }

        modelTra.rotation = Quaternion.Lerp(modelTra.rotation, Quaternion.Euler(0,cam.GetYRot(),0), dtime*4);
        centerTra.rotation = modelTra.rotation;
    }


    void FixedUpdate()
    {
        ApplyMove();
    }

    void ApplyMove()
    {
        rb.linearVelocity = moveVector;
    }

    void SetActivateWeapons(bool v)
    {
        for(int i = 0; i < gotItems.Count; ++i)
        {
            gotItems[i].SetFire(v);
        }
    }

    void AddItem(Item i)
    {
        //if(gotItems.Contains(i)){ return; }//負荷...
        if(!i.isFalling){ return; }
        gotItems.Add(i);
        i.Catch(centerTra);
        PlayerLog.AddLog(string.Format("CATCH >>> {0}", i.GetItemName()));
    }



    public void SetPlayerMode(PlayerState s)
    {
        if(nowState == s) { return; }
        nowState = s;

        SetActivateWeapons(nowState == PlayerState.battle);

    }

    public void SetPlayerPosition(Vector3 p)
    {
        rb.position = p;
    }

    public Transform GetCenterTra()
    {
        return centerTra;
    }

    public int GetItemCount()
    {
        return gotItems.Count;
    }


    void OnTriggerEnter(Collider other)
    {
        Item i = other.GetComponent<Item>();
        if(i)
        {
            AddItem(i);
            return;
        }
    }
    
}
