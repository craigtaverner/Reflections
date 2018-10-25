using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FloatingSpheres
{
    public class NodeObject : MonoBehaviour
    {
        private HashSet<EdgeObject> incoming;
        private HashSet<EdgeObject> outgoing;

        void Start()
        {
            incoming = new HashSet<EdgeObject>();
            outgoing = new HashSet<EdgeObject>();
        }

        void Update()
        {

        }

        internal EdgeObject FindEdge(NodeObject other)
        {
            EdgeObject edge = FindEdge(other, this, incoming);
            if (edge == null)
            {
                edge = FindEdge(this, other, outgoing);
            }
            return edge;
        }

        private EdgeObject FindEdge(NodeObject first, NodeObject second, HashSet<EdgeObject> edges)
        {
            HashSet<EdgeObject>.Enumerator edge = edges.GetEnumerator();
            while (edge.MoveNext())
            {
                EdgeObject current = edge.Current;
                if (current.endNode == second && current.startNode == first)
                {
                    return current;
                }
            }
            return null;
        }

        internal void AddEdge(EdgeObject edge)
        {
            if (edge.startNode == this)
            {
                if (FindEdge(edge.startNode, edge.endNode, outgoing) == null)
                {
                    outgoing.Add(edge);
                }
            }
            else if (edge.endNode == this)
            {
                if (FindEdge(edge.endNode, edge.startNode, incoming) == null)
                {
                    incoming.Add(edge);
                }
            }
            else
            {
                Debug.Log("Invalid edge: " + edge);
            }
        }
    }
}
