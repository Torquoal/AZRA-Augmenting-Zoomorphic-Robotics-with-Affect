using UnityEngine;

public class PlayerHeightReset : MonoBehaviour
{
    [Header("Height Settings")]
    [SerializeField] private float targetHeight = 1.7f; // Standard human height in meters
    [SerializeField] private bool resetOnStart = true;
    [SerializeField] private bool autoResetOnPlay = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Reset Options")]
    [SerializeField] private bool resetPosition = true;
    [SerializeField] private bool resetRotation = true;
    [SerializeField] private Vector3 resetPositionOffset = Vector3.zero;
    
    [Header("References")]
    [SerializeField] private Transform playerTransform; // Usually the camera or XR Origin
    [SerializeField] private Transform floorTransform; // Reference to the floor/ground
    
    
    private Transform cameraTransform;
    
    void Start()
    {
        // Find the main camera
        cameraTransform = Camera.main.transform;
        
        // If no player transform assigned, use the camera's parent (usually XR Origin)
        if (playerTransform == null && cameraTransform != null)
        {
            playerTransform = cameraTransform.parent;
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"PlayerHeightReset: Camera at {cameraTransform.position}, Player at {playerTransform.position}");
        }
        
        if (resetOnStart || autoResetOnPlay)
        {
            // Wait a frame for XR to initialize, then reset
            StartCoroutine(ResetHeightAfterDelay());
        }
    }
    
    private System.Collections.IEnumerator ResetHeightAfterDelay()
    {
        // Wait for XR to initialize
        yield return new WaitForSeconds(0.5f);
        
        if (showDebugLogs)
        {
            Debug.Log("PlayerHeightReset: Auto-resetting player height...");
        }
        
        ResetPlayerHeight();
    }
    
    
    [ContextMenu("Reset Player Height")]
    public void ResetPlayerHeight()
    {
        if (cameraTransform == null)
        {
            Debug.LogError("PlayerHeightReset: No camera found!");
            return;
        }
        
        // Get current camera position
        Vector3 currentCameraPos = cameraTransform.position;
        
        if (showDebugLogs)
        {
            Debug.Log($"PlayerHeightReset: Current camera position: {currentCameraPos}");
        }
        
        // Calculate the height difference
        float heightDifference = currentCameraPos.y - targetHeight;
        
        if (resetPosition)
        {
            // Move the player (XR Origin) down by the height difference
            if (playerTransform != null)
            {
                Vector3 newPlayerPos = playerTransform.position;
                newPlayerPos.y -= heightDifference;
                newPlayerPos += resetPositionOffset;
                playerTransform.position = newPlayerPos;
                
                if (showDebugLogs)
                {
                    Debug.Log($"PlayerHeightReset: Moved player to {newPlayerPos}");
                }
            }
        }
        
        if (resetRotation)
        {
            // Reset rotation to face forward
            if (playerTransform != null)
            {
                playerTransform.rotation = Quaternion.identity;
                
                if (showDebugLogs)
                {
                    Debug.Log("PlayerHeightReset: Reset player rotation");
                }
            }
        }
        
        if (showDebugLogs)
        {
            Debug.Log($"PlayerHeightReset: Camera now at {cameraTransform.position.y:F2}m height");
        }
    }
    
    [ContextMenu("Set Height to 1.7m")]
    public void SetHeightToStandard()
    {
        targetHeight = 1.7f;
        ResetPlayerHeight();
    }
    
    [ContextMenu("Set Height to 1.5m")]
    public void SetHeightToShort()
    {
        targetHeight = 1.5f;
        ResetPlayerHeight();
    }
    
    [ContextMenu("Set Height to 1.8m")]
    public void SetHeightToTall()
    {
        targetHeight = 1.8f;
        ResetPlayerHeight();
    }
    
    [ContextMenu("Teleport to Origin")]
    public void TeleportToOrigin()
    {
        if (playerTransform != null)
        {
            playerTransform.position = Vector3.zero + resetPositionOffset;
            playerTransform.rotation = Quaternion.identity;
            
            if (showDebugLogs)
            {
                Debug.Log("PlayerHeightReset: Teleported player to origin");
            }
        }
    }
    
    [ContextMenu("Show Current Position")]
    public void ShowCurrentPosition()
    {
        if (cameraTransform != null)
        {
            Debug.Log($"PlayerHeightReset: Camera position: {cameraTransform.position}");
            Debug.Log($"PlayerHeightReset: Camera height: {cameraTransform.position.y:F2}m");
        }
        
        if (playerTransform != null)
        {
            Debug.Log($"PlayerHeightReset: Player position: {playerTransform.position}");
        }
    }
    
    // Public method for external scripts
    public void SetTargetHeight(float height)
    {
        targetHeight = height;
        ResetPlayerHeight();
    }
    
    // Public method to get current height
    public float GetCurrentHeight()
    {
        if (cameraTransform != null)
        {
            return cameraTransform.position.y;
        }
        return 0f;
    }
}
