using UnityEngine;
using System.Collections;
using System.Collections.Generic;
 using Vectrosity;
 
 public class Graph : MonoBehaviour 
 {
     public Material lineMaterial;

     //the buffer contains 100 points
     private CircularBuffer<Vector3> buffer = new CircularBuffer<Vector3>(1000);
 
     private VectorLine line;
     private Vector3 point;
 
     private float x = -5f;
     private float increment = .1f;
 
 
     void Start() 
     {
         //initial points
         for (int i = 0; i < buffer.Count; i++) 
         {
             x += increment;
 
             point = new Vector3(x, Mathf.Sin(x));
             buffer.Add(point);
         }

         line = new VectorLine("MyLine", buffer.ToArray(), lineMaterial, 2.0f, LineType.Continuous);
         line.drawTransform = GameObject.Find("LineTransform").transform;
         line.Draw3DAuto();
     }

     public float val = 1.2f;

     void FixedUpdate() 
     {
         x += increment;
 
         //add the points to the buffer (old points get dequeued)
         point = new Vector3(x, Mathf.Sin(x) * Random.Range(1, val));
         buffer.Add(point);
 
         //move the line object
         Vector3 pos = line.drawTransform.position;
         pos.x -= increment;
         line.drawTransform.position = pos;
 
         //update the current points
         //line.points3 = buffer;


         Vector3[] arr = buffer.ToArray();

         List<Vector3> arraytoList = new List<Vector3>(buffer.Count);
         
         for (int i = 0; i < buffer.Count; i++)
         {
             arraytoList.Add(arr[i]);
         }

         line.points3 = arraytoList;
     }
 }