using UnityEngine;
using System.Collections;

public class ContextHelper : MonoBehaviour {

	void Start () 
	{
		if (Application.isEditor) 
		{
			this.gameObject.SetActive(false);
		}
	
	}
}
