using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour {
	public GameObject bullet;

	// Use this for initialization
	void Start () {
		PoolManager.instance.CreatePool ("bullet", bullet, 5);
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown(KeyCode.Space)) {
			PoolManager.instance.GetObject("bullet", transform.position, transform.rotation);
		}
	}
}
