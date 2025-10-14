using UnityEngine;

public class VRDebugPositioner : MonoBehaviour
{
    [Header("Positioning Settings")]
    [SerializeField] private float distanceFromUser = 1.5f;
    [SerializeField] private float heightOffset = 0.0f;
    [SerializeField] private float horizontalOffset = 0.3f; // Offset to the right
    [SerializeField] private bool followUser = true;
    [SerializeField] private bool lookAtUser = true;
    
    [Header("Smoothing")]
    [SerializeField] private float followSpeed = 2.0f;
    [SerializeField] private float rotationSpeed = 2.0f;
    [SerializeField] private bool smoothMovement = true;
    
    [Header("Auto-Reset")]
    [SerializeField] private bool resetOnStart = true;
    [SerializeField] private bool showDebugLogs = true;
    
    private Transform cameraTransform;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializePositioner();
    }
    
    void InitializePositioner()
    {
        // Find the main camera
        cameraTransform = Camera.main?.transform;
        
        if (cameraTransform == null)
        {
            Debug.LogError("VRDebugPositioner: No main camera found!");
            return;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Camera found at {cameraTransform.position}");
        }
        
        if (resetOnStart)
        {
            ResetPosition();
        }
        
        isInitialized = true;
    }
    
    void Update()
    {
        if (!isInitialized || !followUser) return;
        
        UpdatePosition();
    }
    
    void UpdatePosition()
    {
        if (cameraTransform == null) return;
        
        // Calculate target position
        Vector3 cameraForward = cameraTransform.forward;
        Vector3 cameraRight = cameraTransform.right;
        Vector3 cameraUp = cameraTransform.up;
        
        // Position in front and to the right of the user
        targetPosition = cameraTransform.position + 
                       (cameraForward * distanceFromUser) + 
                       (cameraRight * horizontalOffset) + 
                       (cameraUp * heightOffset);
        
        // Calculate target rotation
        if (lookAtUser)
        {
            Vector3 lookDirection = (cameraTransform.position - targetPosition).normalized;
            targetRotation = Quaternion.LookRotation(lookDirection);
        }
        else
        {
            targetRotation = cameraTransform.rotation;
        }
        
        // Apply movement
        if (smoothMovement)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = targetPosition;
            transform.rotation = targetRotation;
        }
    }
    
    public void ResetPosition()
    {
        if (cameraTransform == null) return;
        
        UpdatePosition();
        
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Position reset to {transform.position}");
        }
    }
    
    public void SetDistance(float distance)
    {
        distanceFromUser = distance;
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Distance set to {distance}m");
        }
    }
    
    public void SetHeightOffset(float height)
    {
        heightOffset = height;
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Height offset set to {height}m");
        }
    }
    
    public void SetHorizontalOffset(float horizontal)
    {
        horizontalOffset = horizontal;
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Horizontal offset set to {horizontal}m");
        }
    }
    
    public void ToggleFollow(bool enabled)
    {
        followUser = enabled;
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Follow user {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    public void ToggleLookAtUser(bool enabled)
    {
        lookAtUser = enabled;
        if (showDebugLogs)
        {
            Debug.Log($"VRDebugPositioner: Look at user {(enabled ? "enabled" : "disabled")}");
        }
    }
    
    // Context menu methods for easy testing
    [ContextMenu("Reset Position")]
    public void ResetPositionContext()
    {
        ResetPosition();
    }
    
    [ContextMenu("Show Current Position")]
    public void ShowCurrentPosition()
    {
        Debug.Log($"VRDebugPositioner: Current position: {transform.position}");
        Debug.Log($"VRDebugPositioner: Current rotation: {transform.rotation.eulerAngles}");
        if (cameraTransform != null)
        {
            Debug.Log($"VRDebugPositioner: Camera position: {cameraTransform.position}");
        }
    }
    
    [ContextMenu("Toggle Follow")]
    public void ToggleFollowContext()
    {
        ToggleFollow(!followUser);
    }
    
    [ContextMenu("Set Distance to 1m")]
    public void SetDistance1Meter()
    {
        SetDistance(1.0f);
    }
    
    [ContextMenu("Set Distance to 2m")]
    public void SetDistance2Meters()
    {
        SetDistance(2.0f);
    }
}
