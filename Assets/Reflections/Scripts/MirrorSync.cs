using NewtonVR;
using System;
using UnityEngine;

// For IK to work to animate the hands following the controllers we need an animator component
[RequireComponent(typeof(Animator))]

public class MirrorSync : MonoBehaviour {
    protected Animator animator;
    [Tooltip("The camera representing the players head position.")]
    public Camera playerHead;
    [Tooltip("The NVRHand object representing the controller in the players left hand.")]
    public NVRHand playerLeftHand;
    [Tooltip("The NVRHand object representing the controller in the players right hand.")]
    public NVRHand playerRightHand;
    [Tooltip("The x-values at which the mirror(s) are located. This can be empty in which case one mirror at x=0 is assumed. At most two mirrors are allowed.")]
    public float[] xMirrors;
    public float xMin = 0;
    public float xMax = 0;
    public float zMin = 0;
    public float zMax = 0;
    public Transform avatarWaist;
    public Transform avatarLowerNeck;
    public Transform avatarUpperNeck;
    public Transform avatarHead;
    public Transform avatarLeftHand;
    public Transform avatarRightHand;
    public Transform avatarLeftEye;
    public Transform avatarRightEye;
    public Transform lowerJaw;
    public float mouthOpenAngle = 0;
    private float eyeDirection = 0.0f;
    public float tiltShiftFactor = 0.002f;
    public float heightOffset = 0;
    public float headTurnFactor = 0.75f;
    private float averagePlayerHeight = 1.70f;

    public bool ikActive = false;
    public Transform rightHandObj = null;
    public Transform leftHandObj = null;
    public Transform lookObj = null;
    public Transform avatarCamera = null;
    public bool debug = false;

    private Vector3 hiddenPosition;
    private bool validMirror;
    private int whichMirror;
    private float baseRotation;

    void Start()
    {
        animator = GetComponent<Animator>();
        hiddenPosition = new Vector3((xMax + xMin) / 2, - 5, (zMax + zMin) / 2);
        baseRotation = this.transform.eulerAngles.y;
        if (xMirrors == null || xMirrors.Length == 0)
        {
            xMirrors = new float[] { 0 };
        }
        if (xMirrors.Length > 2)
        {
            throw new ArgumentException(string.Format("The MirrorSync script only works with at most 2 mirrors, but this is configured with ({0})", xMirrors.Length));
        }
    }

    void Update () {

        validMirror = UpdateHeadAndNeck(playerHead.transform.position, playerHead.transform.eulerAngles);
        if (ikActive && validMirror)
        {
            UpdateLookAtObjectForIK(playerHead.transform, lookObj);
            if (playerLeftHand != null && leftHandObj != null)
                UpdateHandObjectForIK(playerLeftHand, leftHandObj);
            if (playerRightHand != null && rightHandObj != null)
                UpdateHandObjectForIK(playerRightHand, rightHandObj);
        }
        if (avatarCamera != null)
        {
            UpdateAvatarCamera(playerLeftHand, avatarCamera);
        }
    }

    private void LateUpdate()
    {
        if (ikActive)
        {
            UpdateMouth();
            UpdateTilt(playerHead.transform.eulerAngles);
            UpdateEyes();
        }
    }

    private void UpdateLookAtObjectForIK(Transform head, Transform lookAt)
    {
        Vector3 pos = head.position;
        Vector3 angles = head.eulerAngles;
        //DebugLog("Head direction: " + angles);
        float ex = 360 - angles.x;
        if (ex > 180) ex -= 360;
        float ey = angles.y - 90;
        if (ey > 180) ey -= 360;
        if (eyeDirection > 0)
        {
            ex = -ex;
        }
        //DebugLog("Looking up by angle: " + ex);
        //DebugLog("Looking right by angle: " + ey);
        float y = (xMirrors[whichMirror] - pos.x) * Mathf.Tan(Mathf.PI * ex / 180);
        float z = (xMirrors[whichMirror] - pos.x) * Mathf.Tan(Mathf.PI * ey / 180);
        lookAt.position = new Vector3(xMirrors[whichMirror], pos.y + y, pos.z - z);
        //DebugLog("Placed lookat object: " + lookAt.position);
    }

    private void UpdateHandObjectForIK(NVRHand hand, Transform fakeHand)
    {
        Vector3 pos = hand.transform.position;
        Vector3 angles = hand.transform.eulerAngles;
        float ex = angles.x;
        if (ex > 180) ex -= 360f;
        float ey = 0 - angles.y;
        float ez = 360 - angles.z;
        if (ez > 180) ez -= 360f;
        fakeHand.position = new Vector3(xMirrors[whichMirror] * 2 - pos.x, pos.y, pos.z);
        fakeHand.eulerAngles = new Vector3(ex, ey, ez);
    }

    private void UpdateAvatarCamera(NVRHand hand, Transform handCam)
    {
        Vector3 pos = hand.transform.position;
        Vector3 angles = hand.transform.eulerAngles;
        float ex = angles.x;
        if (ex > 180) ex -= 360f;
        float ey = 0 - angles.y;
        float ez = 360 - angles.z;
        if (ez > 180) ez -= 360f;
        handCam.position = new Vector3(xMirrors[whichMirror] * 2 - pos.x, pos.y, pos.z);
        handCam.eulerAngles = new Vector3(ex, ey, ez);
    }

    private void UpdateEyes()
    {
        this.avatarLeftEye.transform.eulerAngles = new Vector3(0, eyeDirection, 0);
        this.avatarRightEye.transform.eulerAngles = new Vector3(0, eyeDirection, 0);
    }

    private bool UpdateHeadAndNeck(Vector3 pos, Vector3 angles)
    {
        float tiltShiftFactorMirror = tiltShiftFactor;
        float rotation = baseRotation;
        this.whichMirror = 0;
        if (angles.y < 180)
        {
            // The player is looking away from the avatar, we should move the avatar to the opposite side and rotate it by 180 to face the player
            DebugLog("Looking in secondary direction: angles.y=" + angles.y);
            this.whichMirror = xMirrors.Length - 1;
            this.eyeDirection = -90;
            tiltShiftFactorMirror = 0 - tiltShiftFactor;
            rotation = 0 - baseRotation;
        }
        else
        {
            // The player is looking at the avatar, we should keep the default avatar position and rotation
            DebugLog("Looking in primary direction: angles.y=" + angles.y);
            this.eyeDirection = 90;
        }
        float ex = angles.x;
        if (ex > 180) ex -= 360f;
        float ey = 180 + eyeDirection - angles.y;
        float ez = 360 - angles.z;
        if (ez > 180) ez -= 360f;
        float tiltShiftZ = ez * tiltShiftFactorMirror;
        float tiltShiftX = ex * tiltShiftFactorMirror;
        ey /= 4.0f;
        ez /= 4.0f;
        Vector3 newPosition = new Vector3(xMirrors[whichMirror] * 2 - pos.x + tiltShiftX, pos.y - averagePlayerHeight + heightOffset, pos.z + tiltShiftZ);
        if (IsPositionValid(newPosition))
        {
            this.transform.position = newPosition;
            Vector3 avatarAngles = this.transform.eulerAngles;
            DebugLog("Based on mirror[" + whichMirror + "] - setting avatar rotation to " + rotation);
            this.transform.eulerAngles = new Vector3(avatarAngles.x, rotation, avatarAngles.z);
            DebugLog("Headset at: " + pos);
            DebugLog("Avatar at: " + this.transform.position);
            DebugLog("Avatar angles: " + this.transform.eulerAngles);
            SetAvatarAngles(ex, ey, ez);
            return true;
        }
        else
        {
            DebugLog("Position is out of bounds - hiding avatar");
            this.transform.position = hiddenPosition;
            return false;
        }
    }

    private void DebugLog(string message)
    {
        if (debug) Debug.Log(message);
    }

    private void SetAvatarAngles(float ex, float ey, float ez)
    {
        //this.gameObject.transform.eulerAngles = new Vector3(ex, ey, ez);
        this.avatarWaist.transform.localEulerAngles = new Vector3(0, ey, 0);
        this.avatarLowerNeck.transform.localEulerAngles = new Vector3(0, ey, 0);
        this.avatarUpperNeck.transform.localEulerAngles = new Vector3(0, ey, 0);
        this.avatarHead.transform.localEulerAngles = new Vector3(0, ey * headTurnFactor, 0);
        this.avatarHead.transform.Rotate(new Vector3(ex * headTurnFactor, 0.0f, ez * headTurnFactor));
        this.avatarUpperNeck.transform.Rotate(new Vector3(ex / 8.0f, 0.0f, ez));
        this.avatarLowerNeck.transform.Rotate(new Vector3(ex / 8.0f, 0.0f, ez));
        this.avatarWaist.transform.Rotate(new Vector3(ex / 8.0f, 0.0f, ez));
        this.avatarLeftEye.transform.eulerAngles = new Vector3(0, eyeDirection, 0);
        this.avatarRightEye.transform.eulerAngles = new Vector3(0, eyeDirection, 0);
    }

    private void UpdateMouth()
    {
        if (lowerJaw != null)
        {
            Vector3 angles = lowerJaw.localEulerAngles;
            lowerJaw.localEulerAngles = new Vector3(mouthOpenAngle, angles.y, angles.z);
        }
    }

    private void UpdateTilt(Vector3 angles)
    {
        float ex = angles.x;
        if (ex > 180) ex -= 360f;
        float ey = 180 + eyeDirection - angles.y;
        float ez = 360 - angles.z;
        if (ez > 180) ez -= 360f;
        float tiltShiftZ = ez * tiltShiftFactor;
        float tiltShiftX = ex * tiltShiftFactor;
        ey /= 4.0f;
        ez /= 4.0f;
        SetAvatarAngles(ex, ey, ez);
    }

    private bool IsPositionValid(Vector3 pos)
    {
        bool debug = false;// xMirrors[whichMirror] == 0;
        if(debug) DebugLog("Checking avatar in mirror at " + xMirrors[whichMirror] + " and position " + pos);
        if (xMax - xMin < 0.001 || zMax - zMin < 0.001)
        {
            if (debug) DebugLog("Avatar position valid due to no limitations");
            return true;
        }
        if (pos.x < xMin || pos.x > xMax)
        {
            if (debug) DebugLog("Avatar position invalid due to being outside x range: " + xMin + ".." + xMax);
            return false;
        }
        if (pos.z < zMin || pos.z > zMax)
        {
            if (debug) DebugLog("Avatar position invalid due to being outside z range: " + zMin + ".." + zMax);
            return false;
        }
        if (debug) DebugLog("Avatar position valid: " + pos);
        return true;
    }

    //a callback for calculating IK
    void OnAnimatorIK()
    {
        if (animator)
        {
            //if the IK is active, set the position and rotation directly to the goal. 
            if (ikActive && IsPositionValid(this.transform.position))
            {

                // Set the look target position, if one has been assigned
                if (lookObj != null)
                {
                    animator.SetLookAtWeight(1);
                    animator.SetLookAtPosition(lookObj.position);
                }

                // Set the right hand target position and rotation, if one has been assigned
                if (rightHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.LeftHand, rightHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.LeftHand, rightHandObj.rotation);
                }

                // Set the left hand target position and rotation, if one has been assigned
                if (leftHandObj != null)
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1);
                    animator.SetIKPosition(AvatarIKGoal.RightHand, leftHandObj.position);
                    animator.SetIKRotation(AvatarIKGoal.RightHand, leftHandObj.rotation);
                }
            }

            //if the IK is not active, set the position and rotation of the hand and head back to the original position
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                animator.SetLookAtWeight(0);
            }
        }
    }

}
