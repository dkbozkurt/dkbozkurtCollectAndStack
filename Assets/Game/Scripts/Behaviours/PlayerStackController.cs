// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace Game.Scripts.Behaviours.CubeStackAndFollow
{
    [Serializable]
    public enum CollectibleFollowType
    {
        Static,
        Dynamic
    }
    /// <summary>
    /// 
    /// </summary>
    public class PlayerStackController : MonoBehaviour
    {
        [Header("Stack Properties")]
        [SerializeField] private Transform _stackOriginPoint;
        [SerializeField] private Vector3 _stackPeaceFollowOffSetValue = new Vector3(0f,0.5f,0f);
        [SerializeField] private int _maxStackCount = 20;
        [SerializeField] private float _collectibleJumpDuration = 0.7f;
        [SerializeField] private float _collectibleInitialCollectDelay = 0f;
        
        [Header("Stack Follow Type")]
        [SerializeField] private CollectibleFollowType _collectibleFollowType;
        [SerializeField] private float _collectibleFollowSpeed = 30f;
        
        public int StackedObjectCount => _stackedCollectibleList.Count;
        
        public bool HasStack => StackedObjectCount > 0;

        public bool IsStackFull => StackedObjectCount >= _maxStackCount;

        private Vector3 _lastCollectiblePosition;

        private List<GameObject> _stackedCollectibleList = new List<GameObject>();
        
        private int CubeIndexToFollow => StackedObjectCount-2;

        private Coroutine _waitForCollectCoroutine;
        
        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out CollectibleBehaviour collectibleCubeBehaviour))
            {
                _waitForCollectCoroutine = StartCoroutine(WaitForCollectCoroutine(_collectibleInitialCollectDelay,
                    ()=>CollectCollectible(collectibleCubeBehaviour)));
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.TryGetComponent(out CollectibleBehaviour collectibleCubeBehaviour))
            {
                StopCoroutine(_waitForCollectCoroutine);
            }
        }

        private void CollectCollectible(CollectibleBehaviour collectible)
        {
            if(IsStackFull) return;

            collectible.ColliderSetter(false);
            _stackedCollectibleList.Add(collectible.gameObject);
            SetCollectiblesAsChild(collectible.transform, true);
            
            _lastCollectiblePosition = new Vector3(_stackOriginPoint.localPosition.x + (StackedObjectCount -1) * _stackPeaceFollowOffSetValue.x,
                _stackOriginPoint.localPosition.y + (StackedObjectCount -1) * _stackPeaceFollowOffSetValue.y,
                _stackOriginPoint.localPosition.z + (StackedObjectCount -1 ) * _stackPeaceFollowOffSetValue.z);

            Transform followTransform =
                CubeIndexToFollow < 0 ? _stackOriginPoint : _stackedCollectibleList[CubeIndexToFollow].transform;
            Action action = _collectibleFollowType == CollectibleFollowType.Static
                ? null
                : () => collectible.UpdateCubePosition(followTransform, _collectibleFollowSpeed,true); 
            CollectibleMove(collectible.transform,_lastCollectiblePosition,
                action);
        }

        private void CollectibleMove(Transform collectible,Vector3 lastCollectiblePosition,Action action = null)
        {
            collectible.DOLocalJump(lastCollectiblePosition, 2f,1, _collectibleJumpDuration).SetEase(Ease.Linear).OnComplete(() =>
            {
                if(_collectibleFollowType == CollectibleFollowType.Dynamic) 
                    SetCollectiblesAsChild(collectible, false);
                action?.Invoke();
            });
        }

        private void SetCollectiblesAsChild(Transform collectible,bool status)
        {
            if (status) collectible.transform.SetParent(transform);
            else collectible.transform.SetParent(null);
        }

        private IEnumerator WaitForCollectCoroutine(float duration,Action action = null)
        {
            yield return new WaitForSeconds(duration);
            
            action?.Invoke();
        }
    }
}
