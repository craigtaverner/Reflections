using System.Collections;
using System.Collections.Generic;
using NewtonVR;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        public bool nextSceneOnZThreshold = false;
        public bool loopOnZThreshold = true;
        public string nextScene = null;

        void Start()
        {
            if (this.player == null)
            {
                this.player = this.GetComponent<NVRPlayer>();
            }
            if (this.head == null)
            {
                this.head = this.player.Head.GetComponent<Camera>();
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
                Vector3 pos = this.head.transform.position;
                if (IsPositionOutsideBounds(pos))
                {
                    if (outsideX)
                    {
                        Debug.Log(string.Format("Player %s is outside valid x-range: x=(%f,%f), z=(%f,%f)", pos.ToString(), minX, maxX, minZ, maxZ));
                        FadeView();
                    }
                    if (outsideZ)
                    {
                        Debug.Log(string.Format("Player %s is outside valid z-range: x=(%f,%f), z=(%f,%f)", pos.ToString(), minX, maxX, minZ, maxZ));
                        if (nextSceneOnZThreshold)
                        {
                            this.NextScene();
                        }
                        else if (loopOnZThreshold)
                        {
                            float newZ = pos.z;
                            if (newZ > maxZ)
                            {
                                newZ = newZ - maxZ + minZ;
                            }
                            if (newZ < minZ)
                            {
                                newZ = newZ + maxZ - minZ;
                            }
                            // Use the head (camera) to determine out of bounds position, but set the player position (which sets player, hands and head)
                            pos = this.player.transform.position;
                            this.player.transform.position = new Vector3(pos.x, pos.y, newZ);
                        }
                        else
                        {
                            FadeView();
                        }
                    }
                }
                else if (faded)
                {
                    this.faded = false;
                    FadeViewTo(Color.clear, 0.2f);
                }
            }
        }

        private void FadeView()
        {
            if (!faded)
            {
                this.faded = true;
                FadeViewTo(Color.black, 0.1f);
            }
        }

        private void SetNextScene(string sceneName)
        {
            this.nextScene = sceneName;
        }

        public void NextScene()
        {
            if (nextScene != null)
            {
                SceneManager.LoadScene(nextScene, LoadSceneMode.Single);
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
