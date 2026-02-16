using System;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [SerializeField] float cameraRotationSpeed;

    [SerializeField] Vector3 basePosition = new Vector3(0, 1f, -6f);
    [SerializeField] Vector3 fallPosition = new Vector3(0, 8f, -1f);
    [SerializeField] Transform startTarget;
    [SerializeField] Transform cameraTra;
    [SerializeField] LayerMask fieldLayer = 0b0001;

    [Header("Debug")]

    float dtime;
    float xRot;
    float yRot;

    int singleshakemode;
    Vector2 nowSingleShakeVec;
    Vector2 targetSingleShakeVec;
    
    Vector3 beforeCameraPos;
    Vector3 cameraPos;
    Vector3 localpos;

    RaycastHit hit;
    PlayerInput playerInput;
    [SerializeField] Transform target;
    Transform idleTra;
    [SerializeField] CameraMode nowMode;
    public enum CameraMode
    {
        idle,
        fall,
        battle,
    }

    void Start()
    {
        xRot = 0;
        yRot = 0;
        playerInput = PlayerInput.instance;
        SetTarget(startTarget);
    }

    void LateUpdate()
    {
        dtime = Time.deltaTime;

        switch(nowMode)
        {
            case CameraMode.fall:
                Update_fall();
            break;
            case CameraMode.battle:
                Update_battle();
            break;
        }
    }

    void Update_fall()
    {
        Update_CameraPosition();
    }

    void Update_battle()
    {
        Update_Normal();
        Update_CameraPosition();
    }

    void Update_Normal()
    {
        Vector2 lookvec = playerInput.GetLookVector() * cameraRotationSpeed;//delta
        float half_xaal = 170 * 0.5f;

        xRot -= lookvec.y;
        if (xRot > half_xaal) { xRot = half_xaal; }
        if (xRot < -half_xaal) { xRot = -half_xaal; }

        yRot += lookvec.x;
        yRot = yRot % 360;

        SetRotation(xRot, yRot);
    }

    void Update_CameraPosition()
    {
        if (singleshakemode == 1)
        {
            nowSingleShakeVec = Vector2.Lerp(nowSingleShakeVec, targetSingleShakeVec, dtime * 20f);
            if ((nowSingleShakeVec - targetSingleShakeVec).sqrMagnitude < 0.1f)
            {
                singleshakemode = 0;
            }
        }
        else
        {
            nowSingleShakeVec = Vector2.Lerp(nowSingleShakeVec, Vector2.zero, dtime * 6f);
            //nowSingleShakeVec = Vector2.MoveTowards(nowSingleShakeVec, Vector2.zero, dtime * 8f);
        }
        Vector3 shakepos = cameraTra.right * nowSingleShakeVec.x + cameraTra.up * nowSingleShakeVec.y;

        Vector3 p = target.position + (Quaternion.Euler(xRot,yRot,0) * (localpos));
        cameraPos = Vector3.Lerp(beforeCameraPos, p, dtime*10);
        
        bool res = Physics.Linecast(target.position, cameraPos, out hit, fieldLayer, QueryTriggerInteraction.Ignore);
        //bool res = false;
        if (res)
        {
            cameraTra.position = hit.point + shakepos;
        }
        else
        {
            cameraTra.position = cameraPos + shakepos;
        }

        beforeCameraPos = cameraPos;
    }

    void SetRotation(float x, float y)
    {
        cameraTra.rotation = Quaternion.Euler(x,y,0);
    }

    public void SetMode(CameraMode mode)
    {
        if(nowMode == mode){ return; }
        nowMode = mode;

        switch(nowMode)
        {
            case CameraMode.idle:
                cameraTra.position = idleTra.position;
                cameraTra.rotation = idleTra.rotation;
            break;
            case CameraMode.fall:
                localpos = fallPosition;
                SetRotation(90,0);
            break;
            case CameraMode.battle:
                localpos = basePosition;
            break;
        }
    }

    public void SetTarget(Transform tra)
    {
        target = tra;
    }

    public void SetIdleTransform(Transform t)
    {
        idleTra = t;
    }

    public Vector3 getCameraPos(){ return cameraTra.position; }
    public Vector3 getCameraForward(){ return cameraTra.forward; }

    public float GetXRot() { return xRot; }
    public float GetYRot() { return yRot; }
}
