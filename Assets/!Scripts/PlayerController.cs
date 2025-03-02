using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public enum MovementMode { Movement1, Movement2 }
    [Header("Movement Mode")]
    public MovementMode currentMovementMode = MovementMode.Movement1;

    [Header("Movement2 Settings")]
    public float movement2Sensitivity = 1f;

    [Header("Target to Orbit Around")]
    public Transform target;

    public Transform CameraResetPoint;

    [Header("Rotation Settings")]
    public float rotationSpeed = 5f;

    private float orbitRadius;
    private Vector3 offset;

    private InputAction spaceAction;
    private InputAction mouseDeltaAction;
    private InputAction escAction;

    private bool isPaused = false;
    private bool isOrbiting = false;

    private const float minVerticalAngle = -19.5f;
    private const float maxVerticalAngle = 89f;

    private void Awake()
    {
        spaceAction = new InputAction("OrbitToggle", binding: "<Keyboard>/space");
        mouseDeltaAction = new InputAction("MouseDelta", binding: "<Mouse>/delta");
        escAction = new InputAction("PauseToggle", binding: "<Keyboard>/escape");
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
        ActionManager.OnStickReset += ResetCameraTransform;
        spaceAction.Enable();
        mouseDeltaAction.Enable();
        escAction.Enable();
    }

    private void OnDisable()
    {
        ActionManager.OnStickReset -= ResetCameraTransform;
        spaceAction.Disable();
        mouseDeltaAction.Disable();
        escAction.Disable();
    }

    private void ResetCameraTransform()
    {
        transform.position = CameraResetPoint.position;
        transform.rotation = CameraResetPoint.rotation;

        offset = transform.position - target.position;
        orbitRadius = offset.magnitude;
    }

    private void Update()
    {
        if (escAction.WasPressedThisFrame())
        {
            TogglePause();
        }

        switch (currentMovementMode)
        {
            case MovementMode.Movement1:
                if (spaceAction.ReadValue<float>() > 0.1f && !isPaused)
                {
                    PerformOrbitRotation(rotationSpeed);
                }
                break;

            case MovementMode.Movement2:
                if (spaceAction.WasPressedThisFrame())
                {
                    isOrbiting = !isOrbiting;
                }
                if (isOrbiting && !isPaused)
                {
                    PerformOrbitRotation(rotationSpeed * movement2Sensitivity);
                }
                break;
        }
    }

    private void PerformOrbitRotation(float effectiveRotationSpeed)
    {
        Vector2 mouseDelta = mouseDeltaAction.ReadValue<Vector2>();

        Quaternion horizontalRotation = Quaternion.AngleAxis(mouseDelta.x * effectiveRotationSpeed * Time.deltaTime, Vector3.up);
        Vector3 rightAxis = Vector3.Cross(Vector3.up, offset).normalized;
        Quaternion verticalRotation = Quaternion.AngleAxis(mouseDelta.y * effectiveRotationSpeed * Time.deltaTime, rightAxis);

        Quaternion combinedRotation = horizontalRotation * verticalRotation;
        offset = combinedRotation * offset;
        offset = offset.normalized * orbitRadius;

        float horizontalDistance = new Vector3(offset.x, 0, offset.z).magnitude;
        float currentAngle = Mathf.Atan2(offset.y, horizontalDistance) * Mathf.Rad2Deg;
        float clampedAngle = Mathf.Clamp(currentAngle, minVerticalAngle, maxVerticalAngle);
        float clampedAngleRad = clampedAngle * Mathf.Deg2Rad;

        Vector3 horizontalDir = horizontalDistance > 0.001f
            ? new Vector3(offset.x, 0, offset.z).normalized
            : transform.forward;

        offset = horizontalDir * (orbitRadius * Mathf.Cos(clampedAngleRad)) +
                 Vector3.up * (orbitRadius * Mathf.Sin(clampedAngleRad));

        transform.position = target.position + offset;
        transform.LookAt(target);
    }

    private void TogglePause()
    {
        ActionManager.InvokePaused();
    }
}
