using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SK.Manager;

namespace SK.Utility
{
    public class StickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [SerializeField] private Image inner;
        [SerializeField] private Image outter;

        private Vector3 _defaultPos;
        private Vector3 _dir;
        private float _radius;

        private void Start()
        {
            _defaultPos = inner.transform.position;
            _radius = outter.rectTransform.rect.width * 0.5f;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            inner.transform.position = eventData.position;

            _dir = ((Vector3)eventData.position - _defaultPos).normalized;
        }
        public void OnDrag(PointerEventData eventData)
        {
            float distance = Vector2.Distance(eventData.position, _defaultPos);

            _dir = ((Vector3)eventData.position - _defaultPos).normalized;

            if (distance > _radius)
                inner.transform.position = _defaultPos + _dir * _radius;
            else
                inner.transform.position = _defaultPos + _dir * distance;

            InputManager.Instance.Movement = _dir;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            inner.transform.position = _defaultPos;
            _dir = Vector3.zero;

            InputManager.Instance.Movement = _dir;
        }
    }
}