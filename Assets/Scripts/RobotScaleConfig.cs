using UnityEngine;

public class RobotScaleConfig : MonoBehaviour
{
	public enum RobotSizeProfile
	{
		PetitQoobo, // 1.0
		Qoobo,      // 1.4
		Qoobito     // 0.035
	}

	[Header("Target Root")]
	[SerializeField] private Transform robotRoot; // Assign ARQoobo root here

	[Header("Profile")] 
	[SerializeField] private RobotSizeProfile sizeProfile = RobotSizeProfile.PetitQoobo;
	[SerializeField] [Range(0.01f, 2.0f)] private float customScaleOverride = 1.0f;
	[SerializeField] private bool useCustomScale = false;

	[Header("Absolute Target Local Scales (override)")]
	[SerializeField] private bool useTargetLocalScale = true; // When true, set localScale directly from these values
	[SerializeField] private float petitTargetLocalScale = 0.094f;
	[SerializeField] private float qooboTargetLocalScale = 0.1316f; // 0.094 * 1.4
	[SerializeField] private float qoobitoTargetLocalScale = 0.035f;

	[Header("Profile Rotations (X Axis)")]
	[SerializeField] private float petitXRotationDegrees = 0f;
	[SerializeField] private float qooboXRotationDegrees = 5f;
	[SerializeField] private float qoobitoXRotationDegrees = 0f;

	[Header("Options")] 
	[SerializeField] private bool applyOnStart = true;
	[SerializeField] private bool logChanges = true;
	[SerializeField] private bool livePreviewInPlayMode = true; // Apply immediately on inspector changes during Play
	[SerializeField] private bool useAbsoluteScaling = true;    // Safer for repeated previews

	private const float PETIT_QOOBO_SCALE = 1.0f;
	private const float QOOBO_SCALE = 1.4f;
	private const float QOOBITO_SCALE = 0.37f;

	private Vector3 baseRootScale = Vector3.one; // Captured once to support absolute scaling

	void Start()
	{
		if (applyOnStart)
		{
			// Capture base scale before first apply
			if (robotRoot != null)
			{
				baseRootScale = robotRoot.localScale;
			}
			ApplyConfiguredScale();
		}
	}

	[ContextMenu("Apply Configured Scale")] 
	public void ApplyConfiguredScale()
	{
		if (useTargetLocalScale)
		{
			ApplyExactLocalScale(sizeProfile);
		}
		else
		{
			float targetScale = useCustomScale ? customScaleOverride : GetScaleForProfile(sizeProfile);
			if (useAbsoluteScaling)
			{
				ApplyAbsoluteScale(targetScale);
			}
			else
			{
				ApplyScale(targetScale);
			}
		}
		ApplyProfileRotation(sizeProfile);
	}

	public void ApplyScaleProfile(RobotSizeProfile profile)
	{
		if (useTargetLocalScale)
		{
			ApplyExactLocalScale(profile);
		}
		else
		{
			float targetScale = GetScaleForProfile(profile);
			if (useAbsoluteScaling)
			{
				ApplyAbsoluteScale(targetScale);
			}
			else
			{
				ApplyScale(targetScale);
			}
		}
		ApplyProfileRotation(profile);
	}

	private void ApplyExactLocalScale(RobotSizeProfile profile)
	{
		if (robotRoot == null)
		{
			Debug.LogWarning("RobotScaleConfig: robotRoot not assigned. Please assign ARQoobo root.");
			return;
		}
		float s;
		switch (profile)
		{
			case RobotSizeProfile.Qoobo:
				s = qooboTargetLocalScale;
				break;
			case RobotSizeProfile.Qoobito:
				s = qoobitoTargetLocalScale;
				break;
			case RobotSizeProfile.PetitQoobo:
			default:
				s = petitTargetLocalScale;
				break;
		}
		robotRoot.localScale = Vector3.one * s;
		if (logChanges)
		{
			Debug.Log($"RobotScaleConfig: Set exact localScale = {s:F5} for profile {profile}");
		}
	}

	public void ApplyScale(float scaleMultiplier)
	{
		if (robotRoot == null)
		{
			Debug.LogWarning("RobotScaleConfig: robotRoot not assigned. Please assign ARQoobo root.");
			return;
		}

		robotRoot.localScale *= scaleMultiplier;
		if (logChanges)
		{
			Debug.Log($"RobotScaleConfig: Multiplied ARQoobo scale by {scaleMultiplier:F2}. New scale: {robotRoot.localScale}");
		}
	}

	public void ApplyAbsoluteScale(float absoluteMultiplier)
	{
		if (robotRoot == null)
		{
			Debug.LogWarning("RobotScaleConfig: robotRoot not assigned. Please assign ARQoobo root.");
			return;
		}
		// Capture base if not captured yet or if starting fresh in Play Mode
		if (baseRootScale == Vector3.zero)
		{
			baseRootScale = Vector3.one;
		}
		// If entering play without applyOnStart, capture base once
		if (Application.isPlaying && baseRootScale == Vector3.one && robotRoot != null)
		{
			baseRootScale = robotRoot.localScale; // Use current as base
		}
		robotRoot.localScale = baseRootScale * absoluteMultiplier;
		if (logChanges)
		{
			Debug.Log($"RobotScaleConfig: Set absolute ARQoobo scale to base({baseRootScale}) * {absoluteMultiplier:F2} = {robotRoot.localScale}");
		}
	}

	[ContextMenu("Reset Root Scale To 1,1,1")]
	public void ResetRootScale()
	{
		if (robotRoot == null)
		{
			Debug.LogWarning("RobotScaleConfig: robotRoot not assigned. Please assign ARQoobo root.");
			return;
		}
		robotRoot.localScale = Vector3.one;
		baseRootScale = robotRoot.localScale;
		if (logChanges)
		{
			Debug.Log("RobotScaleConfig: Reset ARQoobo scale to 1,1,1");
		}
	}

	[ContextMenu("Quick: Apply Petit (abs x1.0)")]
	private void QuickApplyPetit()
	{
		ApplyAbsoluteScale(PETIT_QOOBO_SCALE);
		ApplyProfileRotation(RobotSizeProfile.PetitQoobo);
	}

	[ContextMenu("Quick: Apply Qoobo (abs x1.4)")]
	private void QuickApplyQoobo()
	{
		ApplyAbsoluteScale(QOOBO_SCALE);
		ApplyProfileRotation(RobotSizeProfile.Qoobo);
	}

	[ContextMenu("Quick: Apply Qoobito (abs x0.035)")]
	private void QuickApplyQoobito()
	{
		ApplyAbsoluteScale(QOOBITO_SCALE);
		ApplyProfileRotation(RobotSizeProfile.Qoobito);
	}

	void OnValidate()
	{
		// During Play, if live preview is on and fields change in inspector, re-apply
		if (Application.isPlaying && livePreviewInPlayMode)
		{
			ApplyConfiguredScale();
		}
	}

	private void ApplyProfileRotation(RobotSizeProfile profile)
	{
		if (robotRoot == null) return;
		float targetX;
		switch (profile)
		{
			case RobotSizeProfile.Qoobo:
				targetX = qooboXRotationDegrees;
				break;
			case RobotSizeProfile.Qoobito:
				targetX = qoobitoXRotationDegrees;
				break;
			case RobotSizeProfile.PetitQoobo:
			default:
				targetX = petitXRotationDegrees;
				break;
		}
		Vector3 euler = robotRoot.localEulerAngles;
		euler.x = targetX;
		robotRoot.localRotation = Quaternion.Euler(euler);
		if (logChanges)
		{
			Debug.Log($"RobotScaleConfig: Applied X rotation {targetX:F1}° for profile {profile}");
		}
	}

	private float GetScaleForProfile(RobotSizeProfile profile)
	{
		switch (profile)
		{
			case RobotSizeProfile.Qoobo:
				return QOOBO_SCALE;
			case RobotSizeProfile.Qoobito:
				return QOOBITO_SCALE;
			case RobotSizeProfile.PetitQoobo:
			default:
				return PETIT_QOOBO_SCALE;
		}
	}
}


