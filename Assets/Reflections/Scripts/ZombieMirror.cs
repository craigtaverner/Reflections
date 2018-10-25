using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieMirror : MonoBehaviour {
    public Camera player;
    public float xMirror1 = 0.0f;
    public float xMirror2 = 0.0f;
    public float followDistance = 8;
    public float watchDistance = 5;
    public float visibility = 0.0f;
    public bool followPlayer = false;
    private float previousVisbility = 0.0f;
    private Vector3 basePosition;
    private int step = 0;
    private int baseStep = 1;
    private int maxStep = 128;
    private int whichIsCloser = 0;
    private float[] xOrigin;

    void Start()
    {
        this.basePosition = this.transform.position;
        this.xOrigin = new float[] { 2 * xMirror1, 2 * xMirror2 };
        float xDiff1 = Mathf.Abs(this.basePosition.x - xMirror1);
        float xDiff2 = Mathf.Abs(this.basePosition.x - xMirror2);
        this.whichIsCloser = (xDiff2 > xDiff1) ? 1 : 0;
        SetupSteps();
    }

    public Vector4 getBasePosition()
    {
        return basePosition;
    }

    private void SetupSteps()
    {
        int viz = Mathf.RoundToInt(visibility * maxStep);
        Debug.Log("Zombie " + this.name + " with visibility " + visibility + " has viz=" + viz);
        this.baseStep = 2 * ((viz > 0) ? maxStep / viz : maxStep);
        Debug.Log("Zombie " + this.name + " with visibility " + visibility + " has " + baseStep + " frames between real world frames");
        previousVisbility = visibility;
    }

    void Update()
    {
        if (Mathf.Abs(visibility - previousVisbility) > 0.01)
        {
            Debug.Log("Zombie " + this.name + " with visibility " + visibility + " changed from " + previousVisbility);
            SetupSteps();
        }
        if (step >= maxStep)
        {
            step = 0;
        }
        this.gameObject.transform.position = MirrorPosition(basePosition);
        if (player != null)
        {
            FacePlayer();
        }
        step++;
    }

    private void FacePlayer()
    {
        Vector3 playerPosition = MirrorPosition(player.transform.position);
        Vector3 pos = this.transform.position;
        //Debug.Log("Player position: " + playerPosition);
        //Debug.Log(this.name + " position: " + pos);
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
                //Debug.Log("Calculated mirrored[" + steps[step] + "] zombie '" + this.name + "' direction: " + direction);
                Vector3 angles = this.transform.eulerAngles;
                this.transform.eulerAngles = new Vector3(angles.x, direction, angles.z);
                if (followPlayer && distance < followDistance)
                {
                    float moveBy = 0.1f;
                    this.transform.position = Vector3.MoveTowards(this.transform.position, playerPosition, moveBy);
                }
            }
        }
    }

    private Vector3 MirrorPosition(Vector3 pos)
    {
        int which = this.step % 2 == 0 ? whichIsCloser : 1 - whichIsCloser;
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
