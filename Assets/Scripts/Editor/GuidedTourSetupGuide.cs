using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.UI;

public class GuidedTourSetupGuide : EditorWindow
{
    [MenuItem("Tools/Guided Tour Setup")]
    public static void ShowWindow()
    {
        GetWindow<GuidedTourSetupGuide>("Guided Tour Setup");
    }
    
    void OnGUI()
    {
        GUILayout.Label("Guided Tour Setup Guide", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("1. Create Tour Panel UI:", EditorStyles.boldLabel);
        GUILayout.Label("   - Create a Canvas (Screen Space - Camera)");
        GUILayout.Label("   - Add a Panel as child");
        GUILayout.Label("   - Add TextMeshPro components for title, description, progress");
        GUILayout.Label("   - Add a Button for 'Next'");
        GUILayout.Space(5);
        
        GUILayout.Label("2. Position Tour Panel:", EditorStyles.boldLabel);
        GUILayout.Label("   - Place panel behind and above Qoobo");
        GUILayout.Label("   - Set appropriate size and position");
        GUILayout.Label("   - Make sure it's visible but not intrusive");
        GUILayout.Space(5);
        
        GUILayout.Label("3. Assign References:", EditorStyles.boldLabel);
        GUILayout.Label("   - GuidedTourController → Tour Panel");
        GUILayout.Label("   - GuidedTourController → Title Text");
        GUILayout.Label("   - GuidedTourController → Description Text");
        GUILayout.Label("   - GuidedTourController → Next Button");
        GUILayout.Label("   - GuidedTourController → StudyLogger");
        GUILayout.Label("   - WelcomeSequenceController → GuidedTourController");
        GUILayout.Space(5);
        
        GUILayout.Label("4. Test Tour:", EditorStyles.boldLabel);
        GUILayout.Label("   - Use context menu 'Start Tour' on GuidedTourController");
        GUILayout.Label("   - Verify panel appears and text updates");
        GUILayout.Label("   - Test 'Next' button functionality");
        GUILayout.Space(5);
        
        GUILayout.Space(10);
        GUILayout.Label("Manual Setup (Recommended):", EditorStyles.boldLabel);
        GUILayout.Label("Create the tour panel manually in the scene:");
        GUILayout.Label("1. Create Canvas (Screen Space - Camera)");
        GUILayout.Label("2. Add Panel with title and description text");
        GUILayout.Label("3. Add Next button");
        GUILayout.Label("4. Position behind and above Qoobo");
        GUILayout.Label("5. Assign components to GuidedTourController");
    }
}
