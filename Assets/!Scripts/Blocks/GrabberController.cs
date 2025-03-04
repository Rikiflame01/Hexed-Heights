using UnityEngine;
using UnityEngine.InputSystem;

public class GrabberController : MonoBehaviour
{
    public enum MovementMode { Movement1, Movement2 }
    [Header("Movement Mode")]
    public MovementMode currentMovementMode = MovementMode.Movement1;

    public float verticalSpeed = 2f;
    public float rotationSpeed = 90f;
    public float scrollSensitivity = 5f;

    public LayerMask toolsLayer;

    private GameObject selectedObject = null;
    private Vector3 selectionOffset;

    private float baseY;
    private float verticalOffset;

    private InputAction selectAction;
    private InputAction dropAction;
    private InputAction scrollAction;

    public Pause pause;

    private bool isRotatingStick = false;
    private Vector2 lockedCursorPosition;

    private void Awake()
    {
        selectAction = new InputAction("Select", binding: "<Mouse>/leftButton");
        selectAction.performed += ctx => OnSelect();

        dropAction = new InputAction("Drop", binding: "<Mouse>/rightButton");
        dropAction.performed += ctx => OnDrop();

        scrollAction = new InputAction("Scroll", binding: "<Mouse>/scroll");
    }

    private void OnEnable()
    {
        selectAction.Enable();
        dropAction.Enable();
        scrollAction.Enable();

        ActionManager.OnStickReset += ResetGrabber;
    }

    private void OnDisable()
    {
        selectAction.Disable();
        dropAction.Disable();
        scrollAction.Disable();

        ActionManager.OnStickReset -= ResetGrabber;
    }

    private void OnSelect()
    {
        if (selectedObject != null || GameStateManager.CurrentState == GameStateManager.GameState.MainMenu || pause.IsPaused || 
        TurnManager.Instance.rewindRoutineRunning == true)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, toolsLayer))
        {
            selectedObject = hit.collider.gameObject;
            baseY = selectedObject.transform.position.y;
            verticalOffset = baseY;

            Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, baseY, 0));
            if (horizontalPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                selectionOffset = selectedObject.transform.position - hitPoint;
            }
        }
        else
        {
            Debug.Log("No object in Tools layer was hit.");
        }
    }

    private void OnDrop()
    {
        if (selectedObject != null)
        {
            Debug.Log("Stick dropped.");
            selectedObject = null;
            isRotatingStick = false;

            if (Cursor.lockState != CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void ResetGrabber()
    {
        if (selectedObject != null)
        {
            OnDrop();
            verticalOffset = baseY;
        }
    }

    private void Update()
    {
        if (pause.IsPaused)
            return;

        switch (currentMovementMode)
        {
            case MovementMode.Movement1:
                if (selectedObject == null)
                    return;

                if (Keyboard.current.wKey.isPressed)
                {
                    verticalOffset += verticalSpeed * Time.deltaTime;
                }
                if (Keyboard.current.sKey.isPressed)
                {
                    verticalOffset -= verticalSpeed * Time.deltaTime;
                }

                {
                    Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, baseY, 0));
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    Ray ray = Camera.main.ScreenPointToRay(mousePos);
                    if (horizontalPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        Vector3 targetPos = hitPoint + selectionOffset;
                        targetPos.y = verticalOffset;
                        selectedObject.transform.position = targetPos;
                    }
                }

                float rotationInput = 0f;
                if (Keyboard.current.aKey.isPressed)
                    rotationInput -= rotationSpeed * Time.deltaTime;
                if (Keyboard.current.dKey.isPressed)
                    rotationInput += rotationSpeed * Time.deltaTime;
                if (rotationInput != 0f)
                {
                    selectedObject.transform.Rotate(Vector3.up, rotationInput, Space.World);
                }
                break;

            case MovementMode.Movement2:
                if (selectedObject == null)
                    return;

                Vector2 scrollValue = scrollAction.ReadValue<Vector2>();
                verticalOffset += scrollValue.y * scrollSensitivity * Time.deltaTime;

                if (Mouse.current.leftButton.isPressed)
                {
                    if (!isRotatingStick)
                    {
                        isRotatingStick = true;
                        lockedCursorPosition = Mouse.current.position.ReadValue();
                    }
                    Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                    float rotationInput2 = mouseDelta.x * rotationSpeed * Time.deltaTime;
                    selectedObject.transform.Rotate(Vector3.up, rotationInput2, Space.World);

                    Mouse.current.WarpCursorPosition(lockedCursorPosition);
                }
                else
                {
                    isRotatingStick = false;

                    Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, baseY, 0));
                    Vector2 mousePos = Mouse.current.position.ReadValue();
                    Ray ray = Camera.main.ScreenPointToRay(mousePos);
                    if (horizontalPlane.Raycast(ray, out float enter))
                    {
                        Vector3 hitPoint = ray.GetPoint(enter);
                        Vector3 targetPos = hitPoint + selectionOffset;
                        targetPos.y = verticalOffset;
                        selectedObject.transform.position = targetPos;
                    }
                }
                break;
        }
    }
}
