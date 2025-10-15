using UnityEngine;

public class MenuFollowSystem : MonoBehaviour
{
    [Header("Follow Settings")]
    [SerializeField] private float followDistance = 1.5f; // meters from user
    [SerializeField] private float followHeight = 0.0f; // height offset relative to user
    [SerializeField] private float followSmoothing = 0.05f; // how smoothly the menu follows (slower)
    [SerializeField] private float maxFollowSpeed = 1.0f; // maximum follow speed (slower)
    [SerializeField] private bool faceUser = true; // whether menu should face the user
    [SerializeField] private float rotationSmoothing = 0.05f; // how smoothly the menu rotates (slower)
    [SerializeField] private float leftOffset = 0.5f; // how far to the left of the user
    
    [Header("Positioning")]
    [SerializeField] private Vector3 preferredOffset = new Vector3(0, 0, 0); // preferred position relative to user
    [SerializeField] private bool usePreferredOffset = false; // use offset instead of distance
    
    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;
    
    private Transform userTransform;
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private bool isFollowing = false;
    
    void Start()
    {
        // Find the main camera (user's head)
        Camera mainCamera = Camera.main;
        if (mainCamera != null)
        {
            userTransform = mainCamera.transform;
            if (showDebugLogs) Debug.Log("MenuFollowSystem: Found user camera");
        }
        else
        {
            Debug.LogError("MenuFollowSystem: No main camera found!");
        }
        
        // Start following immediately
        isFollowing = true;
    }
    
    void Update()
    {
        if (isFollowing && userTransform != null)
        {
            UpdateMenuPosition();
        }
    }
    
    void UpdateMenuPosition()
    {
        // Calculate target position
        if (usePreferredOffset)
        {
            // Use offset relative to user
            targetPosition = userTransform.position + userTransform.TransformDirection(preferredOffset);
        }
        else
        {
            // Use distance-based positioning to the left of user
            Vector3 userForward = userTransform.forward;
            Vector3 userRight = userTransform.right;
            userForward.y = 0; // Keep menu at user's height level
            userRight.y = 0;
            userForward.Normalize();
            userRight.Normalize();
            
            // Position to the left and slightly in front of user
            targetPosition = userTransform.position + userRight * -leftOffset + userForward * followDistance;
            targetPosition.y += followHeight;
        }
        
        // Calculate target rotation (face user if enabled)
        if (faceUser)
        {
            Vector3 directionToUser = (userTransform.position - transform.position).normalized;
            directionToUser.y = 0; // Keep menu upright
            if (directionToUser != Vector3.zero)
            {
                // Reverse the direction so menu faces user (not away from user)
                targetRotation = Quaternion.LookRotation(-directionToUser);
            }
        }
        else
        {
            targetRotation = transform.rotation; // Keep current rotation
        }
        
        // Smoothly move to target position
        Vector3 currentPosition = transform.position;
        Vector3 newPosition = Vector3.Lerp(currentPosition, targetPosition, followSmoothing);
        
        // Limit movement speed
        Vector3 movement = newPosition - currentPosition;
        if (movement.magnitude > maxFollowSpeed * Time.deltaTime)
        {
            movement = movement.normalized * maxFollowSpeed * Time.deltaTime;
            newPosition = currentPosition + movement;
        }
        
        transform.position = newPosition;
        
        // Smoothly rotate to target rotation
        if (faceUser)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothing);
        }
        
        if (showDebugLogs && Time.time % 3f < 0.1f) // Log every 3 seconds
        {
            Debug.Log($"MenuFollowSystem: Menu at {transform.position}, User at {userTransform.position}, Distance: {Vector3.Distance(transform.position, userTransform.position):F2}m");
        }
    }
    
    // Public methods to control following
    public void StartFollowing()
    {
        isFollowing = true;
        if (showDebugLogs) Debug.Log("MenuFollowSystem: Started following user");
    }
    
    public void StopFollowing()
    {
        isFollowing = false;
        if (showDebugLogs) Debug.Log("MenuFollowSystem: Stopped following user");
    }
    
    public void ToggleFollowing()
    {
        isFollowing = !isFollowing;
        if (showDebugLogs) Debug.Log($"MenuFollowSystem: Following {(isFollowing ? "started" : "stopped")}");
    }
    
    // Teleport menu to user's side
    public void TeleportToUser()
    {
        if (userTransform != null)
        {
            if (usePreferredOffset)
            {
                transform.position = userTransform.position + userTransform.TransformDirection(preferredOffset);
            }
            else
            {
                Vector3 userForward = userTransform.forward;
                Vector3 userRight = userTransform.right;
                userForward.y = 0;
                userRight.y = 0;
                userForward.Normalize();
                userRight.Normalize();
                
                Vector3 newPosition = userTransform.position + userRight * -leftOffset + userForward * followDistance;
                newPosition.y += followHeight;
                transform.position = newPosition;
            }
            
            if (faceUser)
            {
                Vector3 directionToUser = (userTransform.position - transform.position).normalized;
                directionToUser.y = 0;
                if (directionToUser != Vector3.zero)
                {
                    // Reverse the direction so menu faces user (not away from user)
                    transform.rotation = Quaternion.LookRotation(-directionToUser);
                }
            }
            
            if (showDebugLogs) Debug.Log("MenuFollowSystem: Teleported to user");
        }
    }
}
