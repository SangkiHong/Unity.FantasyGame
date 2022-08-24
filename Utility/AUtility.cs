using System;
using System.Linq;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;

using UnityEngine;

using Random = UnityEngine.Random;

namespace AUtility
{
    static public class Utility
    {
        // 깊은 복사
        public static T DeepCopy<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, obj);
                stream.Position = 0;

                return (T)formatter.Deserialize(stream);
            }
        }

        static public Vector3 RandomPointInBounds(Bounds bounds)
        {
            return new Vector3(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y),
                Random.Range(bounds.min.z, bounds.max.z)
            );
        }

        static public float WrapAngle(float angle)
        {
            angle %= 360;
            if (angle > 180)
                return angle - 360;

            return angle;
        }

        static public float UnwrapAngle(float angle)
        {
            if (angle >= 0)
                return angle;

            angle = -angle % 360;

            return 360 - angle;
        }

        static public bool CheckCollision(BoxCollider boxCollider, Collider other)
        {
            // TODO FIX: boxCollider.bounds.Intersects 사용?
            Vector3 center = boxCollider.bounds.center;

            Vector3 lossyScale = boxCollider.transform.lossyScale;
            Vector3 boxSize = boxCollider.size;
            Vector3 size = new Vector3(lossyScale.x * boxSize.x, lossyScale.y * boxSize.y, lossyScale.z * boxSize.z) / 2;

            Quaternion rotation = boxCollider.transform.rotation;

            Collider[] colliders = Physics.OverlapBox(center, size, rotation);
            List<Collider> colliderList = colliders.ToList();

            return colliderList.Contains(other);
        }

    }

    // Position과 Kinematic이 한 프레임에 동시에 바뀌는 경우 체크 못함
    public class KinematicChecker
    {
        public Rigidbody targetRigidbody;
        public bool lastFrameIsKinematic;

        public bool isChanged
        {
            get;
            private set;
        }

        public KinematicChecker(Rigidbody targetRigidbody)
        {
            this.targetRigidbody = targetRigidbody;
            lastFrameIsKinematic = targetRigidbody.isKinematic;
        }

        public void UpdateChecker()
        {
            isChanged = lastFrameIsKinematic != targetRigidbody.isKinematic;
            lastFrameIsKinematic = targetRigidbody.isKinematic;
        }
    }

    
}

