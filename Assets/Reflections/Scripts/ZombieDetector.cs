using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

namespace Reflections
{
    public class ZombieDetector : MonoBehaviour
    {
        public Camera head;
        public float alertDistance = 5;
        public float attackDistance = 2;
        public float attackDuration = 5;
        public float killDistance = 1;
        public float killDuration = 1;
        public float volumeFactor = 0.5f;
        private ZombieMirror[] zombieMirrors;
        private ZombieMirror attackingZombie;
        private long startAttackTime;

        void Start()
        {
            if (this.head == null)
            {
                this.head = this.GetComponent<Camera>();
            }
            if (killDistance < 0.1)
            {
                killDistance = 0.1f;
            }
            if (attackDistance <= killDistance)
            {
                attackDistance = killDistance * 2;
            }
            if (alertDistance <= attackDistance)
            {
                alertDistance = attackDistance * 2;
            }
            this.zombieMirrors = FindObjectsOfType<ZombieMirror>();
            if (this.head != null)
            {
                foreach (ZombieMirror zombieMirror in zombieMirrors)
                {
                    zombieMirror.player = head;
                }
            }
        }

        private ZombieMirror getZombieMirror(GameObject zombie)
        {
            if (zombie == null) return null;
            return zombie.GetComponent<ZombieMirror>();
        }

        void Update()
        {
            foreach (ZombieMirror zombieMirror in zombieMirrors)
            {
                DetectZombie(zombieMirror);
            }
        }

        private void DetectZombie(ZombieMirror zombie)
        {
            Vector3 avatarPosition = this.transform.position;
            Vector3 zombiePosition = zombie.getBasePosition();
            float dx = avatarPosition.x - zombiePosition.x;
            float dz = avatarPosition.z - zombiePosition.z;
            float distance = Mathf.Sqrt(dx * dx + dz * dz);
            //Debug.Log("Distance to zombie1: " + distance);
            if (distance < killDistance)
            {
                zombie.visibility = 1;
                KillPlayer(zombie);
            }
            else if (distance < alertDistance)
            {
                float closenessFactor = 1.0f - (distance - killDistance) / (alertDistance - killDistance);
                Debug.Log("Detected zombie " + zombie.name + " closeness factor " + closenessFactor + " based on distance " + distance);
                zombie.visibility = closenessFactor;
                StartSound(zombie, closenessFactor);
                if (distance < attackDistance)
                {
                    StartAttack(zombie);
                }
                else
                {
                    StopAttack(zombie);
                }
            }
            else
            {
                StopAttack(zombie);
                zombie.visibility = 0;
            }
        }

        private void StopAttack(ZombieMirror zombie)
        {
            if (attackingZombie == zombie)
            {
                attackingZombie = null;
                startAttackTime = 0;
                FadeViewTo(Color.clear, 1);
            }
        }

        private void StartAttack(ZombieMirror zombie)
        {
            if (attackingZombie != zombie)
            {
                Debug.Log("Starting attack for " + zombie.name);
                attackingZombie = zombie;
                startAttackTime = DateTime.Now.Ticks;
                StartAttackAnimation(zombie);
                FadeViewTo(Color.red, attackDuration * 2);
            }
            else
            {
                long nowTicks = DateTime.Now.Ticks;
                if (nowTicks - startAttackTime > this.attackDuration * TimeSpan.TicksPerSecond)
                {
                    Debug.Log("Attack has gone on long enough, " + zombie.name + " will kill player");
                    KillPlayer(zombie);
                }
                else if (nowTicks - startAttackTime > TimeSpan.TicksPerSecond)
                {
                    Debug.Log("Re-triggering attach animation on " + zombie.name);
                    StartAttackAnimation(zombie);
                }
            }
        }

        private void StartAttackAnimation(ZombieMirror zombie)
        {
            Animator zombieAnimator = zombie.GetComponent<Animator>();
            if (zombieAnimator != null)
            {
                zombieAnimator.SetTrigger("attack");
            }
            else
            {
                Debug.Log("Zombie has no animator: " + zombie);
            }
        }

        private void KillPlayer(ZombieMirror zombie)
        {
            Debug.Log("Killing Player");
            this.head.transform.Rotate(new Vector3(10, 10, 10));
            float delay = killDuration;
            StartAttackAnimation(zombie);
            FadeViewTo(Color.red, delay);
            Invoke("ResetScene", delay);
        }

        static public void FadeViewTo(Color newColor, float duration)
        {
            var compositor = OpenVR.Compositor;
            if (compositor != null)
                compositor.FadeToColor(duration, newColor.r, newColor.g, newColor.b, newColor.a, false);
        }

        public void ResetScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            Invoke("ClearColor", 2);
            FadeViewTo(Color.clear, 1);
        }

        public void ClearColor()
        {
            FadeViewTo(Color.clear, 1);
        }

        private void StartSound(ZombieMirror zombie, float volume)
        {
            AudioSource zombieAudio = zombie.GetComponent<AudioSource>();
            if (!zombieAudio.isPlaying)
            {
                Debug.Log("Playing Zombie Audio");
                zombieAudio.Play();
            }
            zombieAudio.volume = volume * volumeFactor;
        }
    }
}
