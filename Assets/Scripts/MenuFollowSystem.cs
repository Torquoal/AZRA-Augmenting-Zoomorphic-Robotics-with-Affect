using UnityEngine;

public class MenuFollowSystem : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform targetTransform; // The menu canvas or container
    [SerializeField] private float followDistance = 0.5f; // Distance from user
    [SerializeField] private float followHeight = 0.1f; // Height offset from user
    [SerializeField] private float followSpeed = 2.0f; // How fast the menu follows
    [SerializeField] private float rotationSpeed = 1.0f; // How fast the menu rotates to face user
    
    [Header("Positioning")]
    [SerializeField] private Vector3 leftOffset = new Vector3(-0.3f, 0f, 0.2f); // Offset to left side
    [SerializeField] private bool smoothFollow = true; // Smooth movement vs instant
    [SerializeField] private float maxDistance = 2.0f; // Max distance before teleporting
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    [SerializeField] private float debugInterval = 2.0f;
    
    private Transform userTransform; // User's headset position
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private float lastDebugTime = 0f;
    
    void Start()
    {
        // Find the user's headset (usually the main camera)
        userTransform = Camera.main.transform;
        
        if (userTransform == null)
        {
            Debug.LogError("MenuFollowSystem: No main camera found! Make sure your camera is tagged as MainCamera.");
            enabled = false;
            return;
        }
        
        if (targetTransform == null)
        {
            targetTransform = transform;
            Debug.LogWarning("MenuFollowSystem: No target transform assigned, using this GameObject's transform.");
        }
        
        // Set initial position
        UpdateTargetPosition();
        targetTransform.position = targetPosition;
        targetTransform.rotation = targetRotation;
        
        if (showDebugLogs)
        {
            Debug.Log("MenuFollowSystem: Started following user");
        }
    }
    
    void Update()
    {
        if (userTransform == null) return;
        
        // Update target position and rotation
        UpdateTargetPosition();
        
        // Move the menu
        if (smoothFollow)
        {
            // Smooth movement
            targetTransform.position = Vector3.Lerp(targetTransform.position, targetPosition, followSpeed * Time.deltaTime);
            targetTransform.rotation = Quaternion.Lerp(targetTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            // Instant movement
            targetTransform.position = targetPosition;
            targetTransform.rotation = targetRotation;
        }
        
        // Check if menu is too far away and teleport if needed
        float distanceToUser = Vector3.Distance(targetTransform.position, userTransform.position);
        if (distanceToUser > maxDistance)
        {
            if (showDebugLogs)
            {
                Debug.Log($"MenuFollowSystem: Menu too far away ({distanceToUser:F2}m), teleporting to user");
            }
            targetTransform.position = targetPosition;
            targetTransform.rotation = targetRotation;
        }
        
        // Debug logging
        if (showDebugLogs && Time.time - lastDebugTime >= debugInterval)
        {
            Debug.Log($"MenuFollowSystem: Distance to user: {distanceToUser:F2}m, Target: {targetPosition}");
            lastDebugTime = Time.time;
        }
    }
    
    void UpdateTargetPosition()
    {
        if (userTransform == null) return;
        
        // Calculate position to the left of the user
        Vector3 userForward = userTransform.forward;
        Vector3 userRight = userTransform.right;
        Vector3 userUp = userTransform.up;
        
        // Position to the left side of the user
        Vector3 leftSidePosition = userTransform.position + 
                                   (userRight * leftOffset.x) + 
                                   (userUp * (leftOffset.y + followHeight)) + 
                                   (userForward * leftOffset.z);
        
        targetPosition = leftSidePosition;
        
        // Make the menu face the user
        Vector3 directionToUser = (userTransform.position - targetPosition).normalized;
        directionToUser.y = 0; // Keep menu level
        if (directionToUser != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(directionToUser);
        }
    }
    
    [ContextMenu("Teleport Menu to User")]
    public void TeleportMenuToUser()
    {
        if (userTransform == null) return;
        
        UpdateTargetPosition();
        targetTransform.position = targetPosition;
        targetTransform.rotation = targetRotation;
        
        if (showDebugLogs)
        {
            Debug.Log("MenuFollowSystem: Teleported menu to user");
        }
    }
    
    [ContextMenu("Toggle Smooth Follow")]
    public void ToggleSmoothFollow()
    {
        smoothFollow = !smoothFollow;
        Debug.Log($"MenuFollowSystem: Smooth follow {(smoothFollow ? "enabled" : "disabled")}");
    }
    
    [ContextMenu("Reset Menu Position")]
    public void ResetMenuPosition()
    {
        if (userTransform == null) return;
        
        // Reset to a default position in front of the user
        Vector3 defaultPosition = userTransform.position + userTransform.forward * 0.5f + userTransform.up * 0.1f;
        targetTransform.position = defaultPosition;
        targetTransform.rotation = Quaternion.LookRotation(userTransform.forward);
        
        if (showDebugLogs)
        {
            Debug.Log("MenuFollowSystem: Reset menu to default position");
        }
    }
    
    // Public methods for external control
    public void SetFollowDistance(float distance)
    {
        followDistance = distance;
        if (showDebugLogs)
        {
            Debug.Log($"MenuFollowSystem: Follow distance set to {distance}");
        }
    }
    
    public void SetFollowSpeed(float speed)
    {
        followSpeed = speed;
        if (showDebugLogs)
        {
            Debug.Log($"MenuFollowSystem: Follow speed set to {speed}");
        }
    }
    
    public void SetLeftOffset(Vector3 offset)
    {
        leftOffset = offset;
        if (showDebugLogs)
        {
            Debug.Log($"MenuFollowSystem: Left offset set to {offset}");
        }
    }
}
