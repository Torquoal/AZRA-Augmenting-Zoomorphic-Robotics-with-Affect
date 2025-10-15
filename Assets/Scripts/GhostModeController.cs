using UnityEngine;
using UnityEngine.InputSystem;

public class GhostModeController : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Transform arQooboRoot; // AR robot root to move/scale
	[SerializeField] private Renderer[] bodyRenderers; // Mesh renderers to adjust transparency
	[SerializeField] private Transform followTarget; // Typically Camera.main transform; auto-filled if null

	[Header("Toggle")] 
	[SerializeField] private bool startInGhostMode = false;
	[SerializeField] private bool allowKeyboardToggle = true;
	[SerializeField] private Key toggleKeyFallback = Key.G; // Fallback key

	[Header("Ghost Settings")] 
	[SerializeField] private float ghostScaleFactor = 0.5f; // shrink to 50%
	[SerializeField] private float riseHeight = 0.3f; // meters upward on enter
	[SerializeField] private float transitionDuration = 1.5f; // seconds for enter/exit

	[Header("Follow Settings")] 
	[SerializeField] private float preferredDistance = 0.5f; // meters from target
	[SerializeField] private float maxDriftSpeed = 1.0f; // m/s while following
	[SerializeField] private float turnSpeedDegPerSec = 240f; // yaw to face target
	[SerializeField] private float followSmoothing = 0.15f; // positional smoothing factor
	[SerializeField] private float heightOffset = 0.0f; // optional offset relative to target height

	private bool isGhost;
	private bool isTransitioning;
	private Vector3 originalPosition;
	private Quaternion originalRotation;
	private Vector3 originalScale;

	// Public property to check ghost state
	public bool IsGhost => isGhost;

	void Awake()
	{
		if (arQooboRoot == null) arQooboRoot = transform;
	}

	void Start()
	{
		CacheOriginalPose();
		if (followTarget == null && Camera.main != null) followTarget = Camera.main.transform;
		if (startInGhostMode) EnterGhostModeImmediate();
	}

	void Update()
	{
		if (allowKeyboardToggle)
		{
			bool hashApprox = Keyboard.current != null && Keyboard.current.shiftKey.isPressed && Keyboard.current[Key.Digit3].wasPressedThisFrame; // '#' on US layout
			bool fallbackKey = Keyboard.current != null && Keyboard.current[toggleKeyFallback].wasPressedThisFrame;
			if (hashApprox || fallbackKey)
			{
				ToggleGhostMode();
			}
		}

		if (isGhost && !isTransitioning)
		{
			FollowTargetUpdate();
		}
	}

	public void ToggleGhostMode()
	{
		if (isTransitioning) return;
		if (!isGhost) StartCoroutine(EnterGhostMode());
		else StartCoroutine(ExitGhostMode());
	}

	private void CacheOriginalPose()
	{
		originalPosition = arQooboRoot.position;
		originalRotation = arQooboRoot.rotation;
		originalScale = arQooboRoot.localScale;
	}

	private System.Collections.IEnumerator EnterGhostMode()
	{
		isTransitioning = true;
		CacheOriginalPose();
		Vector3 startPos = arQooboRoot.position;
		Quaternion startRot = arQooboRoot.rotation;
		Vector3 startScale = arQooboRoot.localScale;
		Vector3 endPos = startPos + Vector3.up * Mathf.Max(0f, riseHeight);
		Vector3 endScale = originalScale * Mathf.Clamp(ghostScaleFactor, 0.1f, 1.0f);

		// Hardcoded alpha values: 0 (transparent) to 0.5 (semi-transparent)
		float startAlpha = 0f;
		float endAlpha = 0.5f;

		float t = 0f;
		while (t < transitionDuration)
		{
			t += Time.deltaTime;
			float k = Mathf.SmoothStep(0f, 1f, t / transitionDuration);
			arQooboRoot.position = Vector3.Lerp(startPos, endPos, k);
			arQooboRoot.localScale = Vector3.Lerp(startScale, endScale, k);
			SetBodyAlpha(Mathf.Lerp(startAlpha, endAlpha, k));
			yield return null;
		}

		arQooboRoot.position = endPos;
		arQooboRoot.localScale = endScale;
		SetBodyAlpha(endAlpha);
		isGhost = true;
		isTransitioning = false;
	}

	private void EnterGhostModeImmediate()
	{
		CacheOriginalPose();
		arQooboRoot.position = originalPosition + Vector3.up * Mathf.Max(0f, riseHeight);
		arQooboRoot.localScale = originalScale * Mathf.Clamp(ghostScaleFactor, 0.1f, 1.0f);
		SetBodyAlpha(0.5f); // Hardcoded semi-transparent
		isGhost = true;
		isTransitioning = false;
	}

	private System.Collections.IEnumerator ExitGhostMode()
	{
		isTransitioning = true;
		Vector3 startPos = arQooboRoot.position;
		Vector3 startScale = arQooboRoot.localScale;

		Vector3 endPos = originalPosition;
		Vector3 endScale = originalScale;

		// Hardcoded alpha values: 0.5 (semi-transparent) to 0 (fully transparent)
		float startAlpha = 0.5f;
		float endAlpha = 0f;

		float t = 0f;
		while (t < transitionDuration)
		{
			t += Time.deltaTime;
			float k = Mathf.SmoothStep(0f, 1f, t / transitionDuration);
			arQooboRoot.position = Vector3.Lerp(startPos, endPos, k);
			arQooboRoot.localScale = Vector3.Lerp(startScale, endScale, k);
			SetBodyAlpha(Mathf.Lerp(startAlpha, endAlpha, k));
			yield return null;
		}

		arQooboRoot.position = endPos;
		arQooboRoot.localScale = endScale;
		SetBodyAlpha(endAlpha);
		isGhost = false;
		isTransitioning = false;
	}

	private void FollowTargetUpdate()
	{
		if (followTarget == null) return;
		Vector3 targetPos = followTarget.position + Vector3.up * heightOffset;
		Vector3 toTarget = targetPos - arQooboRoot.position;
		Vector3 toTargetHorizontal = new Vector3(toTarget.x, 0f, toTarget.z);
		float dist = toTargetHorizontal.magnitude;

		// Maintain preferred distance in the horizontal plane
		Vector3 desiredOffset = Vector3.zero;
		if (dist > Mathf.Epsilon)
		{
			float delta = dist - preferredDistance;
			desiredOffset = toTargetHorizontal.normalized * delta;
		}

		Vector3 desiredPos = arQooboRoot.position + desiredOffset;
		// Smooth drift
		Vector3 newPos = Vector3.Lerp(arQooboRoot.position, desiredPos, 1f - Mathf.Exp(-Time.deltaTime / Mathf.Max(0.001f, followSmoothing)));
		// Clamp max speed
		Vector3 deltaMove = newPos - arQooboRoot.position;
		float maxStep = maxDriftSpeed * Time.deltaTime;
		if (deltaMove.magnitude > maxStep) newPos = arQooboRoot.position + deltaMove.normalized * maxStep;
		arQooboRoot.position = newPos;

		// Face the user
		Vector3 lookDir = (arQooboRoot.position - followTarget.position); // Flipped: robot looks toward user
		lookDir.y = 0f;
		if (lookDir.sqrMagnitude > 0.0001f)
		{
			Quaternion targetYaw = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
			arQooboRoot.rotation = Quaternion.RotateTowards(arQooboRoot.rotation, targetYaw, turnSpeedDegPerSec * Time.deltaTime);
		}
	}

	private void SetBodyAlpha(float alpha)
	{
		if (bodyRenderers == null) return;
		for (int i = 0; i < bodyRenderers.Length; i++)
		{
			var r = bodyRenderers[i];
			if (r == null || r.sharedMaterial == null) continue;
			
			// Simple approach: just set the color alpha
			Color c = r.material.color;
			c.a = alpha;
			r.material.color = c;
			Debug.Log($"Set renderer {i} alpha to: {alpha}");
		}
	}

	private float GetCurrentAlpha()
	{
		if (bodyRenderers == null) return 0f; // Default to transparent
		for (int i = 0; i < bodyRenderers.Length; i++)
		{
			var r = bodyRenderers[i];
			if (r != null && r.sharedMaterial != null) return r.material.color.a;
		}
		return 0f; // Default to transparent
	}
}


