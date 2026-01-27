using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public MainInputs _mainInput;
    PlayerShellscript _playerShellScript;
    Camera _camera;
    RectTransform _homeButton;

    private void Awake()
    {
        _mainInput = new MainInputs();

        _camera = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("CamUI").GetComponent<Camera>();
        _homeButton = GameObject.FindGameObjectWithTag("HomeButton").GetComponent<RectTransform>();
        _playerShellScript = GameObject.FindGameObjectWithTag("PlayerShell").GetComponent<PlayerShellscript>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _mainInput.UI.Enable();

        _mainInput.Player.Enable();
        _mainInput.Player.Touch.started += ctx => StartTouchPrimary(ctx);
        _mainInput.Player.Touch.canceled += ctx => EndTouchPrimary(ctx);
    }

    void StartTouchPrimary(InputAction.CallbackContext ctx)
    {
        if(PartieManager.Instance._partieState == PartieState.PartieStarted)
            _playerShellScript._playerControl.StartFlapp();
        else if (PartieManager.Instance._partieState == PartieState.InGame 
        && !RectTransformUtility.RectangleContainsScreenPoint(_homeButton, _mainInput.Player.TouchPosition.ReadValue<Vector2>(), _camera))
             PartieManager.Instance._menuManager.LoadInPartieMenu();
    }

    void EndTouchPrimary(InputAction.CallbackContext ctx)
    {
        if(PartieManager.Instance._partieState == PartieState.PartieStarted)
            _playerShellScript._playerControl.StopBoostingFlapp();
    }

    private void OnDisable()
    {
        //Le maininput n'est pas détruit automatiqueent lorsque la scene est reload, on le desactive donc juste avant pour éviter les conflits avec celui que l'on va recréé dans l'awake
        _mainInput.Disable();
    }
}
