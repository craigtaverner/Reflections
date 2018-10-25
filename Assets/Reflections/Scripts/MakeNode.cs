using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FloatingSpheres
{
    public class MakeNode : MonoBehaviour
    {
        public Camera head;
        public void MakeOne()
        {
            float x = head.transform.position.x + Random.Range(-0.5f, 0.5f);
            float y = head.transform.position.y + Random.Range(-0.5f, 0.5f);
            float z = head.transform.position.z + Random.Range(-0.5f, 0.5f);
            MakeOne(x, y, z, 0.4f);
        }

        public void BringModel()
        {
            foreach (NodeObject node in GameObject.FindObjectsOfType<NodeObject>())
            {
                Vector3 pos = node.gameObject.transform.position;
                node.gameObject.transform.position = new Vector3(pos.x / 2, pos.y / 10, pos.z / 2);
            }
        }

        public static NodeObject MakeOne(float x, float y, float z, float scale)
        {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = new Vector3(x, y, z);
            sphere.transform.localScale = Vector3.one * (float)System.Math.Max(0.1, scale);
            MeshRenderer renderer = sphere.GetComponent<MeshRenderer>();
            renderer.material.color = Random.ColorHSV();
            Rigidbody body = sphere.AddComponent<Rigidbody>();
            body.mass = 0.05F;
            body.angularDrag = 0.05f;
            body.useGravity = false;  // turn off gravity
            body.drag = 0.8f;
            SphereCollider collider = sphere.GetComponent<SphereCollider>();
            collider.material.bounciness = 1.0F;
            sphere.AddComponent<NVRCollisionSoundObject>();
            NVRInteractableItem interactable = sphere.AddComponent<NVRInteractableItem>();
            interactable.CanAttach = true;
            interactable.DisableKinematicOnAttach = true;
            interactable.EnableKinematicOnDetach = false;
            interactable.EnableGravityOnDetach = false;
            NodeObject node = sphere.AddComponent<NodeObject>();
            return node;
        }

        public void MakeMany()
        {
            for (int i = 0; i < 20; i++)
            {
                MakeOne();
            }
        }

        public static EdgeObject MakeConnection(NodeObject first, NodeObject second, float scale, Text text)
        {
            Debug.Log("Making connection from " + first + " to " + second);
            GameObject connection = GameObject.CreatePrimitive(PrimitiveType.Cube);
            DestroyColliders(connection);
            MeshRenderer renderer = connection.GetComponent<MeshRenderer>();
            renderer.material.color = Color.gray;
            connection.transform.localScale = Vector3.one * scale;  // make very small at first
            EdgeObject edge = connection.AddComponent<EdgeObject>();
            edge.startNode = first;
            edge.endNode = second;
            edge.scale = scale;
            edge.text = text;
            edge.UpdateConnection();
            return edge;
        }

        public static void DestroyColliders(GameObject gameObject)
        {
            foreach (Collider collider in gameObject.GetComponents<Collider>())
            {
                DestroyImmediate(collider);
            }
        }
    }
}
