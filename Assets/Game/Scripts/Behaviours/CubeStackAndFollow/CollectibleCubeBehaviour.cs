using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Behaviours.CubeStackAndFollow
{
    public class CollectibleCubeBehaviour : MonoBehaviour
    {
        [SerializeField] private float _followSpeed;

        public void UpdateCubePosition(Transform followedCube, bool isFollowStart)
        {
            StartCoroutine(StartFollowingToLastCubePosition(followedCube, isFollowStart));
        }

        IEnumerator StartFollowingToLastCubePosition(Transform followedCube, bool isFollowStart)
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
