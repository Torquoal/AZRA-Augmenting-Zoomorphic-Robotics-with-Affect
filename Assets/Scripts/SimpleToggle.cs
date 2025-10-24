using UnityEngine;

public class SimpleToggle : MonoBehaviour
{
    [SerializeField] private float debounceTime = 0.5f;
    [SerializeField] private MenuFollowSystem menuFollowSystem;
    
    private float lastClickTime = 0f;
    
    public void ToggleObjectActive()
    {
        // Check debouncing to prevent double-clicks
        if (Time.time - lastClickTime < debounceTime)
        {
            return;
        }
        
        lastClickTime = Time.time;
        
        // Toggle this object
        gameObject.SetActive(!gameObject.activeInHierarchy);
    }
    
    public void ToggleObjectActiveWithMenuPositioning()
    {
        // Check debouncing to prevent double-clicks
        if (Time.time - lastClickTime < debounceTime)
        {
            return;
        }
        
        lastClickTime = Time.time;
        
        // Toggle this object
        bool wasActive = gameObject.activeInHierarchy;
        gameObject.SetActive(!wasActive);
        
        // If the object is now active, call TeleportToUser from MenuFollowSystem
        if (!wasActive && menuFollowSystem != null)
        {
            menuFollowSystem.TeleportToUser();
        }
    }
    
    [ContextMenu("Toggle Active State")]
    public void TestToggleObjectActive()
    {
        ToggleObjectActive();
    }
    
    [ContextMenu("Toggle with Menu Positioning")]
    public void TestToggleWithMenuPositioning()
    {
        ToggleObjectActiveWithMenuPositioning();
    }
}
