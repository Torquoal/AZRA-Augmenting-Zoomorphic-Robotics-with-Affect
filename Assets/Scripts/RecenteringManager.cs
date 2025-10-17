using System.Collections;
using UnityEngine;

/*
 SUMMARY: Recentering only seems to be useful while testing in the editor. Whereas when it's built and all just recalibrating it seems to work fine. Need to test it more tho.
 */
// Useful forum discussion: https://forum.unity.com/threads/help-with-oculus-quest-reorientation-on-levelload.759626/#post-5066975
public class RecenteringManager : MonoBehaviour
{
    private Transform _OVRCameraRig;
    private Transform _centreEyeAnchor;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        //XRDevice.SetTrackingSpaceType(TrackingSpaceType.RoomScale);
    }

    void Start()
    {

    }

    void Update()
    {
        
    }

    //Helper function to find the correct instances of OVRCameraRig and CentreEyeAnchor
    private bool FindOVRCameraRig()
    {
        OVRCameraRig ovr = FindObjectOfType<OVRCameraRig>();

        if (ovr)
        {
            _OVRCameraRig = ovr.transform;
            _centreEyeAnchor = ovr.centerEyeAnchor;
            return true;
        }
        else
        {
            Debug.LogError("No OVRCameraRig object found! Cannot Recalibrate");
            return false;
        }
    }

    //Calls ResetCamera based on the current scene which was just loaded
    public void RecenterCamera()
    {
        if(FindOVRCameraRig()) StartCoroutine(ResetCamera(new Vector3(0, 1.25f, 0), 0));
    }

    //Resets the OVRCameraRig's position and Y-axis rotation to help align the player's starting position and view to the target parameters
    IEnumerator ResetCamera(Vector3 targetPosition, float targetYRotation)
    {
        yield return new WaitForEndOfFrame();

        float currentRotY = _centreEyeAnchor.eulerAngles.y;
        float difference = targetYRotation - currentRotY;
        _OVRCameraRig.Rotate(0, difference, 0);

        Vector3 newPos = new Vector3(targetPosition.x - _centreEyeAnchor.position.x, targetPosition.y - _centreEyeAnchor.position.y, targetPosition.z - _centreEyeAnchor.position.z);
        _OVRCameraRig.transform.position += newPos;
    }

}