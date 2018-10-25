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

    public void Update()
    {
        if (hand != null)
        {
            this.transform.position = hand.transform.position;
            this.transform.Translate(new Vector3(xOffset, yOffset, zOffset));
            this.transform.eulerAngles = hand.transform.eulerAngles;
            this.transform.Rotate(new Vector3(tilt, 0, 0));
        }
    }
}
