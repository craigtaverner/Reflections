using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhoneController : MonoBehaviour {

    public NVRHand hand;
    public float xOffset = 0.05f;
    public float yOffset = 0.02f;
    public float zOffset = 0;
    public float tilt = 30;
    internal Transform parentTransform;
    internal Vector3 basePosition;
    internal Vector3 baseAngles;
    public bool injectIntoHand = true;

    public void Start()
    {
        this.basePosition = this.transform.position;
        this.baseAngles = this.transform.eulerAngles;
        this.parentTransform = this.transform.parent;
    }

    public void Update()
    {
        if (hand != null)
        {
            if (injectIntoHand)
            {
                this.transform.localPosition = new Vector3(xOffset, yOffset, zOffset);
                this.transform.localEulerAngles = new Vector3(tilt, 0, 0);
            }
            else
            {
                this.transform.position = hand.transform.position;
                this.transform.Translate(new Vector3(xOffset, yOffset, zOffset));
                this.transform.eulerAngles = hand.transform.eulerAngles;
                this.transform.Rotate(new Vector3(tilt, 0, 0));
            }
        }
    }

    internal void SetParent(Transform attachTo)
    {
        if (injectIntoHand)
        {
            this.transform.SetParent(attachTo, true);
        }
    }

    internal void SetHand(NVRHand attachTo)
    {
        this.hand = attachTo;
        if (attachTo == null)
        {
            SetParent(this.parentTransform);
            this.transform.position = this.basePosition;
            this.transform.eulerAngles = this.baseAngles;
        }
        else
        {
            this.hand.DeregisterInteractable(this.GetComponent<NVRInteractable>());
            SetParent(this.hand.transform);
        }
    }
}
