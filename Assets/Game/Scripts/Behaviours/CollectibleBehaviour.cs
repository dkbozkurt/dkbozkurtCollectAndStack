// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class CollectibleBehaviour : MonoBehaviour
    {
        [SerializeField] private ObjectName _objectName;
        [SerializeField] private bool _IsIndividuallyCollectable = false;
        private Collider _collider;
        private float _followSpeed;

        private Quaternion _initialRotation;

        public ObjectName ObjectName => _objectName;

        private Coroutine _followingCoroutine = null;
        
        private void Awake()
        {
            _collider = GetComponent<Collider>();
            ColliderSetter(_IsIndividuallyCollectable);
            _initialRotation = transform.localRotation;
        }
        
        public void ColliderSetter(bool status)
        {
            _collider.enabled = status;
        }

        public Quaternion GetInitialRotation()
        {
            return _initialRotation;
        }
        
        public void UpdateCubePosition(Transform followedCube,float followSpeed, bool isFollowStart)
        {
            _followSpeed = followSpeed;
            transform.SetParent(null);
            _followingCoroutine = StartCoroutine(StartFollowingToLastCubePosition(followedCube, isFollowStart));
        }

        public void StopCubePositionLerping()
        {
            if(_followingCoroutine == null) return;
                StopCoroutine(_followingCoroutine);
        }
        
        private IEnumerator StartFollowingToLastCubePosition(Transform followedCube, bool isFollowStart)
        {
            while (isFollowStart)
            {
                yield return new WaitForEndOfFrame();
                transform.position = new Vector3(Mathf.Lerp(transform.position.x, followedCube.position.x, _followSpeed * Time.deltaTime),
                    transform.position.y,
                    Mathf.Lerp(transform.position.z, followedCube.position.z, _followSpeed * Time.deltaTime));
            }
        }
    }
}
