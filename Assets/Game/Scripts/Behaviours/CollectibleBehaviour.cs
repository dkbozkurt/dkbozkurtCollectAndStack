// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using DG.Tweening;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    [RequireComponent(typeof(Collider))]
    public class CollectibleBehaviour : MonoBehaviour
    {
        [SerializeField] private ObjectName _objectName;
        [SerializeField] private bool _isDirectlyInteractable = false;

        public ObjectName ObjectName => _objectName;

        protected Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = _isDirectlyInteractable;
        }

        public void ToggleCollider(bool status,float delay =0f)
        {
            DOVirtual.DelayedCall(delay, () => _collider.enabled = status);
        }
    }
}
