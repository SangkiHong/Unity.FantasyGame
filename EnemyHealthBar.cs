using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : PoolObject
{
    [SerializeField]
    private Slider slider;

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

    public void Assign(Transform target, int maxValue) 
    {
        isAssign = true;
        targetTransform = target;

        offsetY = targetTransform.GetComponent<Collider>().bounds.max.y - targetTransform.position.y;
        slider.maxValue = maxValue;
    }

    public void UpdateState(int currentPoint)
    {
        slider.value = currentPoint;
    }

    public void Unssaign()
    {
        isAssign = false;
        targetTransform = null;

        Done(true);
    }
}
