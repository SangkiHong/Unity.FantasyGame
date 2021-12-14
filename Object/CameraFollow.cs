using UnityEngine;

namespace Sangki.Scripts.Object
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField]
        private float smoothFactor = 0.5f;

        [SerializeField]
        private GameObject targetObject;

        private Transform thisTransform;
        private Vector3 offset;
        private Quaternion offsetRotate;

        private void Start()
        {
            thisTransform = this.transform;
            offset = transform.position - targetObject.transform.position;
            offsetRotate = thisTransform.rotation;
        }

        private void LateUpdate()
        {
            thisTransform.SetPositionAndRotation
                (Vector3.Lerp(thisTransform.position, targetObject.transform.position + offset, smoothFactor), 
                Quaternion.Lerp(thisTransform.rotation, offsetRotate, smoothFactor));
        }
    }
}
