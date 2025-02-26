using UnityEngine;
using UnityEngine.InputSystem;

public class GrabberController : MonoBehaviour
{
    public float verticalSpeed = 2f;
    public float rotationSpeed = 90f;

    public LayerMask toolsLayer;


    private GameObject selectedObject = null;

    private Vector3 selectionOffset;

    private float verticalOffset;

    private InputAction selectAction;

    private void Awake()
    {
        selectAction = new InputAction("Select", binding: "<Mouse>/leftButton");
        selectAction.performed += ctx => OnSelect();
    }

    private void OnEnable() { selectAction.Enable(); }
    private void OnDisable() { selectAction.Disable(); }

    private void OnSelect()
    {
        if (selectedObject != null)
            return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, toolsLayer))
        {
            selectedObject = hit.collider.gameObject;

            verticalOffset = selectedObject.transform.position.y;

            Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, verticalOffset, 0));
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

    private void Update()
    {
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

        Plane horizontalPlane = new Plane(Vector3.up, new Vector3(0, verticalOffset, 0));
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (horizontalPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 targetPos = hitPoint + selectionOffset;

            targetPos.y = verticalOffset;
            selectedObject.transform.position = targetPos;
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
    }
}
