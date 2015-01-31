using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour 
{

    public static int tInt = 0;
    void Awake()
    {
        tInt = 5;
        Debug.Log("Awake");
    }
    
    void OnEnabled() 
    {
        tInt = 50;
        Debug.Log("Enabled");
    }
    
    void OnLevelWasLoaded()
    {
        tInt = 500;
        Debug.Log("OnLevelWasLoaded");
    }
	
    void Start () 
    {
        Debug.Log("Start");
        Debug.Log("tInt = " + tInt);
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}
}
