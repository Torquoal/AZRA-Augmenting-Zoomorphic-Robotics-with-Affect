using UnityEngine;

public class MenuFollowSystem : MonoBehaviour
{
    [Header("Position Settings")]
    [SerializeField] private float distanceFromUser = 1.5f; // meters from user
    [SerializeField] private float heightOffset = 0.0f; // height offset relative to user
    [SerializeField] private float horizontalOffset = 0.5f; // how far to the left/right of the user (positive = right, negative = left)
    [SerializeField] private bool faceUser = true; // whether menu should face the user
    [SerializeField] private float rotationSmoothing = 0.1f; // how smoothly the menu rotates to face user
    
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
        
        // Position menu once in front of user, then stay fixed in world space
        PositionMenuInFrontOfUser();
        
        // Start rotation following immediately
        isFollowing = true;
    }
    
    void Update()
    {
        if (isFollowing && userTransform != null)
        {
            UpdateMenuPosition();
        }
    }
    
    void PositionMenuInFrontOfUser()
    {
        if (userTransform == null) return;
        
        // Calculate initial position in front of user
        Vector3 userForward = userTransform.forward;
        Vector3 userRight = userTransform.right;
        userForward.y = 0; // Keep menu at user's height level
        userRight.y = 0;
        userForward.Normalize();
        userRight.Normalize();
        
        Vector3 initialPosition;
        if (usePreferredOffset)
        {
            // Use offset relative to user
            initialPosition = userTransform.position + userTransform.TransformDirection(preferredOffset);
        }
        else
        {
            // Position in front of user with horizontal and height offset
            initialPosition = userTransform.position + 
                            userForward * distanceFromUser + 
                            userRight * horizontalOffset + 
                            Vector3.up * heightOffset;
        }
        
        // Set position once - menu stays fixed in world space
        transform.position = initialPosition;
        
        // Set initial rotation to face user
        if (faceUser)
        {
            Vector3 directionToUser = (userTransform.position - transform.position).normalized;
            directionToUser.y = 0;
            if (directionToUser != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(-directionToUser);
            }
        }
        
        if (showDebugLogs) Debug.Log($"MenuFollowSystem: Positioned menu at {transform.position}");
    }
    
    void UpdateMenuPosition()
    {
        // Only rotate to face user (like Thought Bubble) - position stays fixed in world space
        if (faceUser)
        {
            Vector3 directionToUser = (userTransform.position - transform.position).normalized;
            directionToUser.y = 0; // Keep menu upright
            if (directionToUser != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(-directionToUser);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSmoothing);
            }
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
                
                Vector3 newPosition = userTransform.position + 
                                    userForward * distanceFromUser + 
                                    userRight * horizontalOffset + 
                                    Vector3.up * heightOffset;
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
