using System.Collections;
using System.Collections.Generic;
using NewtonVR;
using UnityEngine;
using Valve.VR;

namespace Reflections
{
    public class BoundsChecker : MonoBehaviour
    {
        public NVRPlayer player;
        public Camera head;

        public float minX = 0;
        public float maxX = 0;
        public float minZ = 0;
        public float maxZ = 0;

        private bool valid = true;
        private bool outsideX = false;
        private bool outsideZ = false;
        private bool faded = false;

        void Start()
        {
            if (this.player == null)
            {
                this.player = this.GetComponent<NVRPlayer>();
            }
            if (this.head == null)
            {
                this.head = player.Head.GetComponent<Camera>();
            }
            CheckValid();
        }

        private void CheckValid()
        {
            if (minX > maxX)
            {
                float min = maxX;
                maxX = minX;
                minX = min;
            }
            if (minZ > maxZ)
            {
                float min = maxZ;
                maxZ = minZ;
                minZ = min;
            }
            float deltaX = maxX - minX;
            float deltaZ = maxZ - minZ;
            if (deltaX > 0.01 && deltaZ > 0.01)
            {
                valid = true;
            }
            else
            {
                Debug.Log(string.Format("Invalid range for bounds checker: x=(%f,%f), z=(%f,%f)", minX, maxX, minZ, maxZ));
            }
        }

        private bool IsPositionOutsideBounds(Vector3 position)
        {
            outsideX = position.x > maxX || position.x < minX;
            outsideZ = position.z > maxZ || position.z < minZ;
            return outsideX || outsideZ;
        }

        void Update()
        {
            if (valid)
            {
                if (IsPositionOutsideBounds(this.head.transform.position))
                {
                    if (outsideX)
                    {
                        if (!faded)
                        {
                            this.faded = true;
                            Debug.Log(string.Format("Head %s is outside valid range: x=(%f,%f), z=(%f,%f)", head.transform.position, minX, maxX, minZ, maxZ));
                            FadeViewTo(Color.black, 0.1f);
                        }
                    }
                    if (outsideZ)
                    {
                        Vector3 pos = this.head.transform.position;
                        float newZ = pos.z;
                        if (newZ > maxZ)
                        {
                            newZ = newZ - maxZ + minZ;
                        }
                        if (newZ < minZ)
                        {
                            newZ = newZ + maxZ - minZ;
                        }
                        this.head.transform.position = new Vector3(pos.x, pos.y, newZ);
                    }
                }
                else if (faded)
                {
                    FadeViewTo(Color.clear, 0.2f);
                }
            }
        }

        static public void FadeViewTo(Color newColor, float duration)
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
                compositor.FadeToColor(duration, newColor.r, newColor.g, newColor.b, newColor.a, false);
        }
    }
}
