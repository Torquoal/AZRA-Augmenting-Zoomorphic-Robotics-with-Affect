using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class HoverButton : MonoBehaviour
{
    [Header("Button Settings")]
    [SerializeField] private GameObject buttonObject;
    [SerializeField] private Transform buttonSpawn;
    [SerializeField] private UnityEvent onButtonPressed;

    [Header("Hover Timing")]
    [SerializeField] private float hoverTime = 5f;
    [SerializeField] private float buttonCooldown = 0.5f;

    [Header("Hand Path Matching")]
    [SerializeField] private string leftHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/LeftHand";
    [SerializeField] private string rightHandPath = "Camera Rig/[BuildingBlock] Interaction/[BuildingBlock] Hand Interactions/RightHand";

    private bool isHovering = false;
    private float hoverStartTime = 0f;
    private bool buttonSpawned = false;
    private float lastButtonSpawnTime = -999f;

    private void OnTriggerEnter(Collider other)
    {
        if (IsHand(other))
        {
            Debug.Log("HoverButton: Hand entered area.");
            isHovering = true;
            hoverStartTime = Time.time;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsHand(other))
        {
            Debug.Log("HoverButton: Hand exited area.");
            isHovering = false;
            hoverStartTime = 0f;
        }
    }

    private void Update()
    {
        if (isHovering && !buttonSpawned)
        {
            if (Time.time - hoverStartTime >= hoverTime && Time.time - lastButtonSpawnTime >= buttonCooldown)
            {
                Debug.Log("HoverButton: Hover time reached. Spawning button.");
                SpawnButton();
            }
        }
    }

    private void SpawnButton()
    {
        if (buttonObject && buttonSpawn)
        {
            Instantiate(buttonObject, buttonSpawn.position, buttonSpawn.rotation);
            buttonSpawned = true;
            lastButtonSpawnTime = Time.time;
            onButtonPressed?.Invoke();
        }
        else
        {
            Debug.LogWarning("HoverButton: ButtonObject or ButtonSpawn not assigned!");
        }
    }

    private bool IsHand(Collider col)
    {
        string fullPath = GetGameObjectPath(col.gameObject);
        return fullPath.Contains(leftHandPath) || fullPath.Contains(rightHandPath);
    }

    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }

    // Optional: call this if you ever want to reset the button spawn state externally
    public void ResetButton()
    {
        buttonSpawned = false;
        lastButtonSpawnTime = -999f;
    }
}
