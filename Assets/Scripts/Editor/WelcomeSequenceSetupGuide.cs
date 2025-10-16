using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class WelcomeSequenceSetupGuide : EditorWindow
{
    [MenuItem("Tools/Welcome Sequence Setup Guide")]
    public static void ShowWindow()
    {
        GetWindow<WelcomeSequenceSetupGuide>("Welcome Sequence Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Welcome Sequence Setup Guide", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Step 1: Create UI Canvas", EditorStyles.boldLabel);
        GUILayout.Label("1. Right-click in Hierarchy → UI → Canvas");
        GUILayout.Label("2. Set Canvas Scaler to 'Scale With Screen Size'");
        GUILayout.Label("3. Set Reference Resolution to 1920x1080");
        GUILayout.Space(5);
        
        GUILayout.Label("Step 2: Add TextMeshPro Elements", EditorStyles.boldLabel);
        GUILayout.Label("1. Right-click Canvas → UI → Text - TextMeshPro (create 2 texts)");
        GUILayout.Label("2. Name them: 'InstructionText' and 'CountdownText'");
        GUILayout.Label("3. Position InstructionText in center");
        GUILayout.Label("4. Position CountdownText below instruction");
        GUILayout.Space(5);
        
        GUILayout.Label("Step 3: Configure WelcomeSequenceController", EditorStyles.boldLabel);
        GUILayout.Label("1. Create empty GameObject named 'WelcomeSequence'");
        GUILayout.Label("2. Add WelcomeSequenceController script");
        GUILayout.Label("3. Assign Canvas to Welcome Canvas field");
        GUILayout.Label("4. Assign Text components to their respective fields");
        GUILayout.Space(5);
        
        GUILayout.Label("Step 4: TextMeshPro Settings", EditorStyles.boldLabel);
        GUILayout.Label("InstructionText:");
        GUILayout.Label("- Font Size: 48");
        GUILayout.Label("- Color: White");
        GUILayout.Label("- Alignment: Center");
        GUILayout.Space(5);
        
        GUILayout.Label("CountdownText:");
        GUILayout.Label("- Font Size: 72");
        GUILayout.Label("- Color: Yellow");
        GUILayout.Label("- Alignment: Center");
        GUILayout.Space(10);
        
        if (GUILayout.Button("Create Default Canvas Setup"))
        {
            CreateDefaultSetup();
        }
    }
    
    void CreateDefaultSetup()
    {
        // Create Canvas
        GameObject canvasGO = new GameObject("WelcomeCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera; // Use Screen Space Camera for VR
        canvasGO.AddComponent<CanvasScaler>();
        canvasGO.AddComponent<GraphicRaycaster>();
        
        // Find the main camera (VR camera)
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindObjectOfType<Camera>();
        }
        
        if (mainCamera != null)
        {
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = 1f; // 1 meter in front of camera
        }
        
        // Configure Canvas Scaler for VR
        CanvasScaler scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Create Instruction TextMeshPro
        GameObject instructionGO = new GameObject("InstructionText");
        instructionGO.transform.SetParent(canvasGO.transform);
        TextMeshProUGUI instructionText = instructionGO.AddComponent<TextMeshProUGUI>();
        instructionText.text = "Look Straight Ahead";
        instructionText.fontSize = 48;
        instructionText.color = Color.white;
        instructionText.alignment = TextAlignmentOptions.Center;
        
        // Create Countdown TextMeshPro
        GameObject countdownGO = new GameObject("CountdownText");
        countdownGO.transform.SetParent(canvasGO.transform);
        TextMeshProUGUI countdownText = countdownGO.AddComponent<TextMeshProUGUI>();
        countdownText.text = "3";
        countdownText.fontSize = 72;
        countdownText.color = Color.yellow;
        countdownText.alignment = TextAlignmentOptions.Center;
        
        // Position texts
        instructionText.rectTransform.anchoredPosition = new Vector2(0, 50);
        countdownText.rectTransform.anchoredPosition = new Vector2(0, -50);
        
        Debug.Log("Welcome Sequence Canvas created! Assign it to your WelcomeSequenceController.");
    }
}
