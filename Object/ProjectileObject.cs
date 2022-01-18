using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;

public class ProjectileObject : PoolObject
{
	[SerializeField]
	private float speed = 30f;
	[SerializeField]
	private float lifetime = 4.5f;
	[SerializeField]
	private bool isRotate;
	[ShowIf("isRotate")]
	[SerializeField]
	private float rotateSpeed = 1.5f;
	[SerializeField]
	private bool isTrajectory;
	[ShowIf("isTrajectory")]
	[SerializeField]
	private float arcAmount = 1;

	Transform thisTransform;
	Rigidbody myRigidbody;

	Vector3 m_StartPosition, m_TargetPos;

	readonly string m_Tag_Player = "Player";
	readonly string m_Tag_Ground = "Ground";

    public override void OnAwake()
	{
		if (myRigidbody == null) myRigidbody = GetComponent<Rigidbody>();
		if (thisTransform == null) thisTransform = this.transform;

		// Clear old values
		myRigidbody.velocity = Vector3.zero;
		myRigidbody.angularVelocity = Vector3.zero;

		if (isRotate) thisTransform.DOLocalRotate(thisTransform.rotation.eulerAngles + Vector3.down * 180 * rotateSpeed, 0.8f, RotateMode.Fast).SetLoops(-1, LoopType.Incremental).SetRelative();

		if (!isTrajectory)
		{
			myRigidbody.velocity = transform.forward * speed;
		}
		else
		{
			m_StartPosition = thisTransform.position;
			m_TargetPos = thisTransform.forward * 50f; 
            
        }
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
