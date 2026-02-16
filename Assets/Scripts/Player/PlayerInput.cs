using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    public static PlayerInput instance {get; private set;}


    [SerializeField] Vector2 move;
    [SerializeField] Vector2 look;
    [SerializeField] bool cont;
    [SerializeField] bool cont_down;

    PlayerInputActions input;
    InputAction[] actions;

    void Awake()
    {
        if(instance)
        {
            enabled = false;
            Destroy(gameObject);
            return;
        }
        instance = this;

        input = new PlayerInputActions();
        actions = new InputAction[4];
        actions[0] = input.Main.Move;
        actions[1] = input.Main.Look_Gamepad;
        actions[2] = input.Main.Look_Mouse;
        actions[3] = input.Main.Continue;
        input.Enable();
    }

    void OnDestroy()
    {
        if (input == null) { return; }
        input.Disable();
        input.Dispose();
    }

    void Update()
    {
        UpdateControll();
    }

    void UpdateControll()
    {
        move = actions[0].ReadValue<Vector2>();
        look = Vector2.zero;
        look += actions[1].ReadValue<Vector2>() * Time.unscaledDeltaTime * 30f;
        look += actions[2].ReadValue<Vector2>() / Screen.dpi;
        cont = actions[3].IsPressed();
        cont_down = actions[3].WasPressedThisFrame();
    }

    public Vector2 GetMoveVector() { return move; }
    public Vector2 GetLookVector() { return look; }
    public bool GetContinue() { return cont; }
    public bool GetContinueDown() { return cont_down; }

}
