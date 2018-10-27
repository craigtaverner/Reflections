using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightFlickering : MonoBehaviour {

    public float flickerFrequency = 0.2f;

    private long flickerWaitTicks = 0;
    private long lastFlickerTicks = 0;
    private float originalIntensity;
    public Light mirrorLight;
    private Light lightSource;
    private long keepOffCount=0;
    public bool debug = false;

    void Start()
    {
        lastFlickerTicks = DateTime.Now.Ticks;
        this.lightSource = this.GetComponent<Light>();
        this.originalIntensity = this.lightSource.intensity;
    }

    void Update()
    {
        if (keepOffCount > 0)
        {
            keepOffCount--;
        }
        else
        {
            long nowTicks = DateTime.Now.Ticks;
            if (nowTicks - lastFlickerTicks > flickerWaitTicks)
            {
                this.lastFlickerTicks = nowTicks;
                double flickerWaitSeconds = UnityEngine.Random.Range(0, 100) * 0.01f / this.flickerFrequency;
                this.flickerWaitTicks = (long)(TimeSpan.TicksPerSecond * flickerWaitSeconds);
                SetIntensity(0.01f);
                this.keepOffCount = (long)UnityEngine.Random.Range(0, 5);
                if (debug) Debug.Log("Turning off light for " + keepOffCount + " frames, and waiting " + flickerWaitSeconds + "s before doing this again (" + flickerWaitTicks + " ticks)");
            }
            else
            {
                SetIntensity(originalIntensity);
            }
        }
    }

    private void SetIntensity(float intensity)
    {
        this.lightSource.intensity = intensity;
        if (this.mirrorLight != null)
        {
            this.mirrorLight.intensity = intensity;
        }
    }
}
