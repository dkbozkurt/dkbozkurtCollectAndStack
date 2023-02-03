// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using System.Collections;
using System.Collections.Generic;
using Game.Scripts.Enums;
using UnityEngine;

namespace Game.Scripts.Behaviours.CubeStackAndFollow
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

        public ObjectName ObjectName => _objectName;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            ColliderSetter(_IsIndividuallyCollectable);
        }
        
        public void ColliderSetter(bool status)
        {
            _collider.enabled = status;
        }
        
        public void UpdateCubePosition(Transform followedCube,float followSpeed, bool isFollowStart)
        {
            _followSpeed = followSpeed;
            StartCoroutine(StartFollowingToLastCubePosition(followedCube, isFollowStart));
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
