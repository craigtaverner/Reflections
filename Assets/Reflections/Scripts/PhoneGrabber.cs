using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneGrabber : MonoBehaviour {

    public Transform phoneTransform;
    public NVRHand hand;
    public Camera avatarCamera;
    public Camera phoneCamera;
    internal PhoneController heldPhone;

    void Start()
    {
        this.hand = this.GetComponent<NVRHand>();
        if (this.avatarCamera == null || this.phoneCamera == null)
        {
            foreach (Camera cam in Component.FindObjectsOfType<Camera>())
            {
                if (avatarCamera == null && cam.name.ToLower().Contains("avatar"))
                {
                    avatarCamera = cam;
                }
                else if (phoneCamera == null && cam.name.ToLower().Contains("phone"))
                {
                    phoneCamera = cam;
                }
            }
        }
    }

    public void FixedUpdate()
    {
        if (hand.IsInteracting)
        {
            PhoneController phone = hand.CurrentlyInteracting.GetComponent<PhoneController>();
            if (phone != null)
            {
                if (phone.hand != null)
                {
                    PhoneGrabber other = phone.hand.GetComponent<PhoneGrabber>();
                    if (phone == other.heldPhone)
                    {
                        Debug.Log("Grabbing already held phone");
                        other.heldPhone = null;
                    }
                }
                phone.hand = hand;
                this.heldPhone = phone;
                hand.DeregisterInteractable(phone.GetComponent<NVRInteractable>());
            }
        }
    }
}
