using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Reflections
{
    public class ZombieMirror : MonoBehaviour
    {
        public Camera player;
        public float xMirror1 = 0.0f;
        public float xMirror2 = 0.0f;
        public float followDistance = 6;
        public float watchDistance = 8;
        public float visibility = 0.0f;
        public bool followPlayer = false;
        public float followSpeed = 0.5f;
        private float previousVisbility = 0.0f;
        private Vector3 basePosition;
        private int step = 0;
        private int baseStep = 1;
        private int maxStep = 128;
        private int whichIsCloser = 0;
        private float[] xOrigin;
        private long lastMoveTicks = 0;
        public bool debug;

        void Start()
        {
            this.basePosition = this.transform.position;
            this.xOrigin = new float[] { 2 * xMirror1, 2 * xMirror2 };
            UpdateWhichIsCloser();
            SetupSteps();
        }

        public Vector4 getBasePosition()
        {
            return basePosition;
        }

        private void SetupSteps()
        {
            int viz = Mathf.RoundToInt(visibility * maxStep);
            DebugLog("Zombie " + this.name + " with visibility " + visibility + " has viz=" + viz);
            this.baseStep = 2 * ((viz > 0) ? maxStep / viz : maxStep);
            DebugLog("Zombie " + this.name + " with visibility " + visibility + " has " + baseStep + " frames between real world frames");
            previousVisbility = visibility;
        }

        void Update()
        {
            if (Mathf.Abs(visibility - previousVisbility) > 0.01)
            {
                DebugLog("Zombie " + this.name + " with visibility " + visibility + " changed from " + previousVisbility);
                SetupSteps();
            }
            if (step >= maxStep)
            {
                step = 0;
            }
            this.transform.position = MirrorPosition(basePosition);
            if (player != null)
            {
                FacePlayer();
                FollowPlayer();
            }
            step++;
        }

        private void FacePlayer()
        {
            Vector3 playerPosition = MirrorPosition(player.transform.position);
            Vector3 pos = this.transform.position;
            //DebugLog("Player position: " + playerPosition);
            //DebugLog(this.name + " position: " + pos);
            float dx = playerPosition.x - pos.x;
            float dz = playerPosition.z - pos.z;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            if (distance > 0.1)
            {
                if (distance < watchDistance)
                {
                    float azimuth = Mathf.Asin(dx / distance);
                    float direction = 180.0f * azimuth / Mathf.PI;
                    if (dz < 0)
                    {
                        direction = 180 - direction;
                    }
                    DebugLog("Calculated mirrored[" + WhichMirror() + "] zombie '" + this.name + "' direction: " + direction);
                    Vector3 angles = this.transform.eulerAngles;
                    this.transform.eulerAngles = new Vector3(angles.x, direction, angles.z);
                }
            }
        }

        private void FollowPlayer()
        {
            Vector3 playerPosition = new Vector3(player.transform.position.x, basePosition.y, player.transform.position.z);
            float dx = playerPosition.x - basePosition.x;
            float dz = playerPosition.z - basePosition.z;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            if (distance > 0.1)
            {
                if (followPlayer && distance < followDistance)
                {
                    DebugLog("Zombie '" + this.name + "' at " + basePosition + " with player at " + playerPosition);
                    float azimuthToZombie = Vector3.Angle(Vector3.back, basePosition - playerPosition);
                    float azimuthLooking = 180 - player.transform.eulerAngles.y;
                    if (dx > 0) azimuthToZombie = 0 - azimuthToZombie;
                    if (azimuthToZombie > 180) azimuthToZombie -= 180;
                    if (azimuthLooking > 180) azimuthLooking -= 180;
                    DebugLog("Zombie is in direction=" + azimuthToZombie + " while player is looking in direction " + azimuthLooking);
                    float azimuthDelta = Mathf.Abs(azimuthToZombie - azimuthLooking);
                    if (azimuthDelta > 90)
                    {
                        long now = DateTime.Now.Ticks;
                        if (lastMoveTicks > 0)
                        {
                            float secondsSinceLastMove = ((float)(now - lastMoveTicks)) / TimeSpan.TicksPerSecond;
                            float moveBy = this.followSpeed * secondsSinceLastMove;
                            DebugLog("Following by " + moveBy + " towards: " + playerPosition);
                            Vector3 newPosition = Vector3.MoveTowards(this.basePosition, playerPosition, moveBy);
                            DebugLog("Move from " + this.basePosition + " to " + newPosition);
                            this.basePosition = newPosition;
                            DebugLog("Moved to " + this.basePosition);
                            UpdateWhichIsCloser();
                        }
                        this.lastMoveTicks = now;
                    }
                    else
                    {
                        DebugLog("Zombie will not move when player is looking at it: " + azimuthDelta);
                        this.lastMoveTicks = 0;
                    }
                }
            }
        }

        private void DebugLog(string message)
        {
            if (debug) Debug.Log(message);
        }

        private void UpdateWhichIsCloser()
        {
            float xDiff1 = Mathf.Abs(this.basePosition.x - xMirror1);
            float xDiff2 = Mathf.Abs(this.basePosition.x - xMirror2);
            this.whichIsCloser = (xDiff2 < xDiff1) ? 1 : 0;
        }

        private int WhichMirror()
        {
            return this.step % 2 == 0 ? whichIsCloser : 1 - whichIsCloser;
        }

        private Vector3 MirrorPosition(Vector3 pos)
        {
            int which = WhichMirror();
            if ((this.step + 1) % baseStep == 0 && which != whichIsCloser)
            {
                return pos;
            }
            else
            {
                return new Vector3(xOrigin[which] - pos.x, pos.y, pos.z);
            }
        }
    }
}
