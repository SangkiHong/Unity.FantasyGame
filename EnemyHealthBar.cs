using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthBar : PoolObject
{
    private Transform thisTransform, targetTransform;
    private float offsetY;
    private bool isAssign;

    private void Awake()
    {
        thisTransform = this.transform;
    }

    private void FixedUpdate()
    {
        if (isAssign)
        {
            thisTransform.position = targetTransform.position + Vector3.up * offsetY;
        }
    }

    public void Assign(Transform target) 
    {
        isAssign = true;
        targetTransform = target;

        offsetY = targetTransform.GetComponent<Collider>().bounds.max.y - targetTransform.position.y;
    }

    public void Unssaign()
    {
        isAssign = false;
        targetTransform = null;

        Done(true);
    }
}
