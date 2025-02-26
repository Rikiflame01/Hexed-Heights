using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Target to Orbit Around")]
    public Transform target;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    private float orbitRadius;

    private Vector3 offset;


    private InputAction spaceAction;
    private InputAction mouseDeltaAction;


    private const float minVerticalAngle = -19.5f;
    private const float maxVerticalAngle = 89f;

    private void Awake()
    {

        spaceAction = new InputAction("OrbitToggle", binding: "<Keyboard>/space");

        mouseDeltaAction = new InputAction("MouseDelta", binding: "<Mouse>/delta");
    }

    private void Start()
    {
        if (target == null)
        {
            Debug.LogError("No target assigned for PlayerController.");
            return;
        }

        offset = transform.position - target.position;
        orbitRadius = offset.magnitude;
    }

    private void OnEnable()
    {
        spaceAction.Enable();
        mouseDeltaAction.Enable();
    }

    private void OnDisable()
    {
        spaceAction.Disable();
        mouseDeltaAction.Disable();
    }

    private void Update()
    {

        if (spaceAction.ReadValue<float>() > 0.1f)
        {
            Vector2 mouseDelta = mouseDeltaAction.ReadValue<Vector2>();

            Quaternion horizontalRotation = Quaternion.AngleAxis(mouseDelta.x * rotationSpeed * Time.deltaTime, Vector3.up);

            Vector3 rightAxis = Vector3.Cross(Vector3.up, offset).normalized;

            Quaternion verticalRotation = Quaternion.AngleAxis(mouseDelta.y * rotationSpeed * Time.deltaTime, rightAxis);

            Quaternion combinedRotation = horizontalRotation * verticalRotation;

            offset = combinedRotation * offset;


            offset = offset.normalized * orbitRadius;

            float horizontalDistance = new Vector3(offset.x, 0, offset.z).magnitude;

            float currentAngle = Mathf.Atan2(offset.y, horizontalDistance) * Mathf.Rad2Deg;

            float clampedAngle = Mathf.Clamp(currentAngle, minVerticalAngle, maxVerticalAngle);
            float clampedAngleRad = clampedAngle * Mathf.Deg2Rad;

            Vector3 horizontalDir = horizontalDistance > 0.001f ? new Vector3(offset.x, 0, offset.z).normalized : transform.forward;

            offset = horizontalDir * (orbitRadius * Mathf.Cos(clampedAngleRad)) + Vector3.up * (orbitRadius * Mathf.Sin(clampedAngleRad));

            transform.position = target.position + offset;

            transform.LookAt(target);
        }
    }
}
