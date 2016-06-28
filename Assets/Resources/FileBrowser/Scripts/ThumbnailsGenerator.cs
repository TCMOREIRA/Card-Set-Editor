using UnityEngine;
using System.Collections;

public class Thumbnails : MonoBehaviour
{


	// Use this for initialization
	void Start ()
    {
	
	}

    void OnBecameVisible()
    {
        enabled = true;
    }

    void OnBecameInvisible()
    {
        enabled = false;
    }
	
	// Update is called once per frame
	void Update ()
    {
	    
	}
}
