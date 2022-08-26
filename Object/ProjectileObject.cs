using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using SK.Player;

namespace SK.Object
{
	public class ProjectileObject : PoolObject
	{
		[SerializeField] private float speed = 30f;
		[SerializeField] private float lifetime = 4.5f;
		[SerializeField] private bool isRotate;

		[ShowIf("isRotate")]
		[SerializeField] private float rotateSpeed = 1.5f;
		[SerializeField] private bool isTrajectory;

		[ShowIf("isTrajectory")]
		[SerializeField] private float arcAmount = 1;

		[Header("Damage")]
		[SerializeField] private int damageAmount = 1;

		private Transform thisTransform;
		private Rigidbody thisRigidbody;
		private Collider thisCollider;

		public override void OnAwake()
		{
			if (!thisTransform) thisTransform = transform;
			if (!thisRigidbody) TryGetComponent(out thisRigidbody);
			if (!thisCollider) TryGetComponent(out thisCollider);

			thisCollider.enabled = true;

			// Clear old values
			thisRigidbody.velocity = thisRigidbody.angularVelocity = Vector3.zero;

			if (isRotate) 
				thisTransform.DOLocalRotate(thisTransform.rotation.eulerAngles + 180 * rotateSpeed * Vector3.down, 0.8f, RotateMode.Fast)
						.SetLoops(-1, LoopType.Incremental)
						.SetRelative();

			if (!isTrajectory) thisRigidbody.velocity = transform.forward * speed;

			Done(lifetime);
		}

		private void OnTriggerEnter(Collider other)
		{
			if (other.CompareTag(Strings.Tag_Player) || other.CompareTag(Strings.Tag_Ground))
			{
				thisCollider.enabled = false;
				//if (other.CompareTag(Strings.Tag_Player)) 
				//	PlayerController.Instance.OnDamageTrigger(thisTransform.gameObject, damageAmount);

				Done();
			}
		}
	}
}
