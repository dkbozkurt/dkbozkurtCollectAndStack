// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Scripts.Enums;
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
        public static event Action<CollectibleBehaviour> OnCollectibleAdded;
        public static event Action<CollectibleBehaviour> OnCollectibleRemoved;
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

        public bool CanIncrease => StackedObjectCount < _maxStackCount;

        public bool CanDecrease => StackedObjectCount > 0;

        public float AnimationDuration => _collectibleJumpDuration;
        public bool IsAnimPlaying;
        
        private List<CollectibleBehaviour> _stackedCollectibleList = new List<CollectibleBehaviour>();
        
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
        
        public void IncreaseWithAnimation(CollectibleBehaviour collectible,Action callback)
        {
            if(!CanIncrease) return;

            collectible.ColliderSetter(false);
            _stackedCollectibleList.Add(collectible);
            SetCollectiblesAsChild(collectible.transform, true);
            
            Transform collectedTransform = collectible.transform;
            collectedTransform.DOKill();

            collectedTransform.DOLocalRotateQuaternion(collectible.GetInitialRotation(), AnimationDuration);
            collectedTransform.DOLocalJump(GetLastEmptyStackIndexPosition(), 3, 1, AnimationDuration).OnComplete(() =>
                OnJumpComplete(collectible, collectedTransform, callback));
        }
        
        private void OnJumpComplete(CollectibleBehaviour collectible,Transform collectedTransform, Action callback= null)
        {
            OnCollectibleAdded?.Invoke(collectible);
            collectedTransform.localPosition = GetLastEmptyStackIndexPosition();
            collectedTransform.localScale = Vector3.one;
            callback?.Invoke();
        }

        public CollectibleBehaviour DecreaseStackWithAnimation(ObjectName objectName, Vector3 jumpPosition,
            Quaternion rotation, Transform parent, Action callback = null)
        {
            if (!CanDecrease) return null;

            var collectible = GetLastObject(objectName);
            if (!collectible) return null;
            
            collectible.ColliderSetter(collectible.IsIndividuallyCollectable);
            _stackedCollectibleList.Remove(collectible);
            
            collectible.transform.SetParent(parent);
            collectible.transform.DORotateQuaternion(rotation, AnimationDuration);

            collectible.transform.DOJump(jumpPosition, CalculateJumpPowerByDistance(collectible.transform.position,jumpPosition)
                , 1, AnimationDuration).OnComplete(
                () =>
                    callback?.Invoke())
                ;

            return collectible;
        }
        
        public CollectibleBehaviour DecreaseStackWithAnimation(Vector3 jumpPosition,Transform parent, Action callback = null)
        {
            if (!CanDecrease) return null;

            var collectible = GetLastObject();
            if (!collectible) return null;
            
            collectible.ColliderSetter(collectible.IsIndividuallyCollectable);
            _stackedCollectibleList.Remove(collectible);
            
            collectible.transform.SetParent(parent);
            collectible.transform.DORotateQuaternion(collectible.GetInitialRotation(), AnimationDuration);

            collectible.transform.DOJump(jumpPosition, CalculateJumpPowerByDistance(collectible.transform.position,jumpPosition)
                    , 1, AnimationDuration).OnComplete(
                    () =>
                        callback?.Invoke())
                ;

            return collectible;
        }

        private CollectibleBehaviour GetLastObject()
        {
            return _stackedCollectibleList.Last();
        }
        
        private CollectibleBehaviour GetLastObject(ObjectName objectName)
        {
            return _stackedCollectibleList.LastOrDefault(x => x.ObjectName == objectName);
        }


        private float CalculateJumpPowerByDistance(Vector3 spawnPoint,Vector3 jumpPos)
        {
            float diffY = Mathf.Abs(spawnPoint.y - jumpPos.y);
            float jumpPower = Mathf.Clamp(diffY, 1, 25);

            return jumpPower;
        }
        
        private void CollectCollectible(CollectibleBehaviour collectible)
        {
            if(IsStackFull) return;

            collectible.ColliderSetter(false);
            _stackedCollectibleList.Add(collectible);
            SetCollectiblesAsChild(collectible.transform, true);
            
            Transform followTransform =
                CubeIndexToFollow < 0 ? _stackOriginPoint : _stackedCollectibleList[CubeIndexToFollow].transform;
            Action action = _collectibleFollowType == CollectibleFollowType.Static
                ? null
                : () => collectible.UpdateCubePosition(followTransform, _collectibleFollowSpeed,true); 
            CollectibleMove(collectible.transform,GetLastEmptyStackIndexPosition(),
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

        private Vector3 GetLastEmptyStackIndexPosition()
        {
            return new Vector3(_stackOriginPoint.localPosition.x + (StackedObjectCount -1) * _stackPeaceFollowOffSetValue.x,
                _stackOriginPoint.localPosition.y + (StackedObjectCount -1) * _stackPeaceFollowOffSetValue.y,
                _stackOriginPoint.localPosition.z + (StackedObjectCount -1 ) * _stackPeaceFollowOffSetValue.z);
        }

        private IEnumerator WaitForCollectCoroutine(float duration,Action action = null)
        {
            yield return new WaitForSeconds(duration);
            
            action?.Invoke();
        }
    }
}
