using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PoolObject : MonoBehaviour {
	public string poolKey;

	private readonly string done = "Done";

	public virtual void OnAwake() { }

	protected void Done(float time) {
		DOVirtual.DelayedCall(time, () => { if (gameObject.activeInHierarchy) Done(); });
	}

	protected void Done(bool isUI = false) {
		if (!isUI) PoolManager.instance.ReturnObjectToQueue(gameObject, this);
		else UIPoolManager.instance.ReturnObjectToQueue(gameObject, this);
	}
}
