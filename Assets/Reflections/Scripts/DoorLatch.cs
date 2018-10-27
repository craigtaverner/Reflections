using NewtonVR;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reflections
{
    public class DoorLatch : MonoBehaviour
    {
        public GameObject door;
        public NVRInteractable keyCard;
        public float distanceThreshold = 0.1f;
        public float secondsToLock = 1;
        public bool locked;
        public float maxOpen = 135;
        public float openTo = 15;
        private HingeJoint doorHinge;
        private long lastChangeTime = 0;
        private Boolean active;
        private Vector3 basePosition;

        void Start()
        {
            if (this.door == null)
            {
                this.door = this.transform.parent.gameObject;
            }
            this.basePosition = this.transform.position;
            this.doorHinge = this.door.GetComponent<HingeJoint>();
            this.active = false;
            LockDoor(locked);
        }

        void Update()
        {
            if (distance(this.transform.position, keyCard.transform.position) < distanceThreshold)
            {
                if (!active)
                {
                    lastChangeTime = DateTime.Now.Ticks;
                    SetActive(true);
                }
                else
                {

                    long nowTicks = DateTime.Now.Ticks;
                    if (nowTicks - lastChangeTime > secondsToLock * TimeSpan.TicksPerSecond)
                    {
                        LockDoor(!locked);
                        SetActive(false);
                    }
                }
            }
            else
            {
                SetActive(false);
            }
        }

        private void SetActive(bool makeActive)
        {
            this.transform.transform.position = new Vector4(basePosition.x, basePosition.y, basePosition.z - (makeActive ? 0.005f : 0));
            active = makeActive;
        }

        private void LockDoor(bool shouldLock)
        {
            JointLimits limits = doorHinge.limits;
            if (shouldLock)
            {
                if (door.transform.eulerAngles.y < openTo + 0.1)
                {
                    Debug.Log("Locking door: " + door.name);
                    limits.max = 0;
                    doorHinge.limits = limits;
                    door.transform.eulerAngles = new Vector3(0, 0, 0);
                    this.locked = true;
                }
                else
                {
                    Debug.Log("Cannot lock open door!");
                }
            }
            else
            {
                Debug.Log("Unlocking door: " + door.name);
                limits.max = maxOpen;
                doorHinge.limits = limits;
                door.transform.eulerAngles = new Vector3(0, openTo, 0);
                this.locked = false;
            }
        }

        private float distance(Vector3 a, Vector3 b)
        {
            float dx = b.x - a.x;
            float dy = b.y - a.y;
            float dz = b.z - a.z;
            return Mathf.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
