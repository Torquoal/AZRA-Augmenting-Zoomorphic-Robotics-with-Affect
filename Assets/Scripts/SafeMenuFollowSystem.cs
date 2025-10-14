using UnityEngine;

public class SafeMenuFollowSystem : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private Transform targetTransform; // The menu canvas or container
    [SerializeField] private float followDistance = 0.5f; // Distance from user
    [SerializeField] private float followSpeed = 1.0f; // How fast the menu follows (slower)
    [SerializeField] private float rotationSpeed = 0.5f; // How fast the menu rotates (slower)
    
    [Header("Positioning")]
    [SerializeField] private Vector3 leftOffset = new Vector3(-0.3f, 0f, 0.2f); // Offset to left side
    [SerializeField] private bool smoothFollow = true; // Smooth movement vs instant
    [SerializeField] private float maxDistance = 1.5f; // Max distance before teleporting (smaller)
    
    [Header("Safety")]
    [SerializeField] private bool enableFollow = true; // Master toggle
    [SerializeField] private float minFollowDistance = 0.1f; // Minimum distance to start following
    [SerializeField] private bool showDebugLogs = true;
    
    private Transform userTransform; // User's headset position
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isInitialized = false;
    
    void Start()
    {
        InitializeFollowSystem();
    }
    
    void InitializeFollowSystem()
    {
        // Find the user's headset (usually the main camera)
        userTransform = Camera.main.transform;
        
        if (userTransform == null)
        {
            Debug.LogError("SafeMenuFollowSystem: No main camera found! Make sure your camera is tagged as MainCamera.");
            enabled = false;
            return;
        }
        
        if (targetTransform == null)
        {
            targetTransform = transform;
            Debug.LogWarning("SafeMenuFollowSystem: No target transform assigned, using this GameObject's transform.");
        }
        
        // Set initial position safely
        UpdateTargetPosition();
        targetTransform.position = targetPosition;
        targetTransform.rotation = targetRotation;
        
        isInitialized = true;
        
        if (showDebugLogs)
        {
            Debug.Log("SafeMenuFollowSystem: Initialized safely");
        }
    }
    
    void Update()
    {
        if (!isInitialized || !enableFollow || userTransform == null) return;
        
        // Check if user has moved enough to warrant following
        float distanceToUser = Vector3.Distance(targetTransform.position, userTransform.position);
        
        if (distanceToUser < minFollowDistance)
        {
            // User is too close, don't follow
            return;
        }
        
        // Update target position and rotation
        UpdateTargetPosition();
        
        // Move the menu
        if (smoothFollow)
        {
            // Smooth movement with safety checks
            Vector3 newPosition = Vector3.Lerp(targetTransform.position, targetPosition, followSpeed * Time.deltaTime);
            Quaternion newRotation = Quaternion.Lerp(targetTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            // Safety check: don't move too far at once
            float moveDistance = Vector3.Distance(targetTransform.position, newPosition);
            if (moveDistance < 2.0f) // Max 2 units per frame
            {
                targetTransform.position = newPosition;
                targetTransform.rotation = newRotation;
            }
        }
        else
        {
            // Instant movement with safety check
            float moveDistance = Vector3.Distance(targetTransform.position, targetPosition);
            if (moveDistance < 5.0f) // Max 5 units teleport
            {
                targetTransform.position = targetPosition;
                targetTransform.rotation = targetRotation;
            }
        }
        
        // Check if menu is too far away and teleport if needed
        if (distanceToUser > maxDistance)
        {
            if (showDebugLogs)
            {
                Debug.Log($"SafeMenuFollowSystem: Menu too far away ({distanceToUser:F2}m), teleporting to user");
            }
            targetTransform.position = targetPosition;
            targetTransform.rotation = targetRotation;
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
                                   (userUp * (leftOffset.y + followDistance)) + 
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
    
    [ContextMenu("Disable Follow")]
    public void DisableFollow()
    {
        enableFollow = false;
        Debug.Log("SafeMenuFollowSystem: Follow disabled");
    }
    
    [ContextMenu("Enable Follow")]
    public void EnableFollow()
    {
        enableFollow = true;
        Debug.Log("SafeMenuFollowSystem: Follow enabled");
    }
    
    [ContextMenu("Reset to Safe Position")]
    public void ResetToSafePosition()
    {
        if (userTransform == null) return;
        
        // Reset to a safe position in front of the user
        Vector3 safePosition = userTransform.position + userTransform.forward * 0.5f + userTransform.up * 0.1f;
        targetTransform.position = safePosition;
        targetTransform.rotation = Quaternion.LookRotation(userTransform.forward);
        
        Debug.Log("SafeMenuFollowSystem: Reset to safe position");
    }
}
