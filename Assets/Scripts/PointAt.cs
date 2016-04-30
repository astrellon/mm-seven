using UnityEngine;
using System.Collections;

public class PointAt : MonoBehaviour {

    public GameObject Target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        var toTarget = (transform.position - Target.transform.position).normalized;
        transform.forward = toTarget;
	}
}
