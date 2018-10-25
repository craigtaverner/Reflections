using NewtonVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class NodeEvents : MonoBehaviour
    {
        public Camera head;
        public NVRHand hand;
        public NVRHand otherHand;
        public Text text;
        public Transform elementToTrack;
        public string closeParameter = "pointingToElement";
        public string farParameter = "notPointingToElement";
        private Animator animToTrigger;
        private bool triggerWasPressed;
        private bool twoHandInteraction;
        public float scale = 0.3f;

        public void Start()
        {
            if (this.head == null)
            {
                this.head = this.GetComponent<Camera>();
                if (this.head == null)
                {
                    Debug.LogError("Could not find a Camera object from this: " + this);
                }
            }
            if (this.hand == null)
            {
                this.hand = this.GetComponent<NVRHand>();
                if (this.hand == null)
                {
                    Debug.LogError("Could not find an NVRHand object from this: " + this);
                }
            }
            if (this.elementToTrack != null)
            {
                this.animToTrigger = elementToTrack.GetComponent<Animator>();
                if (this.animToTrigger == null)
                {
                    Debug.LogError("Could not find an Animator object from element: " + this.elementToTrack);
                }
            }
            this.triggerWasPressed = false;
        }

        public void FixedUpdate()
        {
            if (hand != null)
            {
                if (hand.UseButtonDown)
                {
                    triggerWasPressed = true;
                }
                if (hand.UseButtonUp)
                {
                    if (triggerWasPressed)
                    {
                        TriggerPressed();
                    }
                    triggerWasPressed = false;
                }
                if (hand.IsInteracting)
                {
                    this.twoHandInteraction = false;
                    if (otherHand != null)
                    {
                        if (otherHand.IsInteracting)
                        {
                            this.twoHandInteraction = true;
                            if (otherHand.CurrentlyInteracting == hand.CurrentlyInteracting)
                            {
                                // NewtonVR already filters this out, so we would need more changes to get to this point
                                Debug.Log("Two hands interacting with same object: " + hand.CurrentlyInteracting);
                            }
                            else
                            {
                                NodeObject first = hand.CurrentlyInteracting.GetComponent<NodeObject>();
                                NodeObject second = otherHand.CurrentlyInteracting.GetComponent<NodeObject>();
                                if (first == null)
                                {
                                    Debug.Log("Hand is interacting with something that is not a NodeObject: " + hand.CurrentlyInteracting);
                                }
                                else if (second == null)
                                {
                                    Debug.Log("Other hand is interacting with something that is not a NodeObject: " + hand.CurrentlyInteracting);
                                }
                                else
                                {
                                    EdgeObject edge = first.FindEdge(second);
                                    if (edge == null)
                                    {
                                        Debug.Log("No edge exists - making one");
                                        edge = MakeNode.MakeConnection(first, second, 0.05f, text);
                                        first.AddEdge(edge);
                                        second.AddEdge(edge);
                                        edge.text = text;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("No hand defined for " + this);
            }
            //Debug.Log("FixedUpdate: " + this);
        }

        private void TriggerPressed()
        {
            float dx = hand.transform.position.x - head.transform.position.x;
            float dz = hand.transform.position.z - head.transform.position.z;
            float x = head.transform.position.x + 2 * dx;
            float z = head.transform.position.z + 2 * dz;
            float height = 1;
            if (elementToTrack != null)
            {
                height = Math.Max(height, elementToTrack.position.y);
                if (this.animToTrigger != null)
                {
                    double angleToHand = EdgeObject.Azimuth(head.transform, hand.transform);
                    double angleToElement = EdgeObject.Azimuth(head.transform, elementToTrack.transform);
                    Debug.Log("AngleToHand: " + angleToHand);
                    Debug.Log("AngleToElement: " + angleToElement);
                    Debug.Log("Difference between hand and element: " + Math.Abs(angleToElement - angleToHand));
                    if (Math.Abs(angleToElement - angleToHand) < 0.1)
                    {
                        this.animToTrigger.SetTrigger(this.closeParameter);
                        this.animToTrigger.ResetTrigger(this.farParameter);
                    }
                    else
                    {
                        this.animToTrigger.ResetTrigger(this.closeParameter);
                        this.animToTrigger.SetTrigger(this.farParameter);
                    }
                }
                else
                {
                    Debug.LogError("Could not find animation component on " + elementToTrack);
                }
            }
            height = Math.Max(height, Math.Max(head.transform.position.y, hand.transform.position.y));
            MakeNode.MakeOne(x, height, z, scale);
        }

        public void DoButtonDown()
        {
            Debug.Log("Do button down: " + this);
        }
        public void DoButtonUp()
        {
            Debug.Log("Do button up: " + this);
        }
        public void DoHovering()
        {
            Debug.Log("Do hovering: " + this);
        }
        public void DoStartInteraction()
        {
            Debug.Log("Do start interaction: " + this);
        }
        public void DoEndInteraction()
        {
            Debug.Log("Do end interaction: " + this);
        }
    }
}
