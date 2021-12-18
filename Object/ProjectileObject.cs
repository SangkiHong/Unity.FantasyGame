using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileObject : PoolObject
{
	[SerializeField]
	private float speed = 30f;
	[SerializeField]
	private float lifetime = 4.5f;

	Rigidbody myRigidbody;

	readonly string m_Tag_Player = "Player";
	readonly string m_Tag_Ground = "Ground";

	public override void OnAwake()
	{
		if (myRigidbody == null)
		{
			myRigidbody = GetComponent<Rigidbody>();
		}

		// Clear old values
		myRigidbody.velocity = Vector3.zero;
		myRigidbody.angularVelocity = Vector3.zero;


		// Set new values
		myRigidbody.velocity = transform.forward * speed;

		Done(lifetime);
	}

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(m_Tag_Player) || other.CompareTag(m_Tag_Ground))
        {
			Done();
		}
    }
}
