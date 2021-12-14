using UnityEngine;

public class Bullet : PoolObject {
	
	[SerializeField]
	private float lifetime;
	
	Rigidbody myRigidbody;

	public override void OnAwake() {
		if (myRigidbody == null) {
			myRigidbody = GetComponent<Rigidbody> ();
		}

		// Clear old values
		myRigidbody.velocity = Vector3.zero;
		myRigidbody.angularVelocity = Vector3.zero;


		// Set new values
		myRigidbody.velocity = transform.forward * 60;

		Done (lifetime);
	}
}
