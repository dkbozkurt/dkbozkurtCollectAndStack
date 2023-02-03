// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Game.Scripts.Behaviours;
using Game.Scripts.Behaviours.CubeStackAndFollow;
using Game.Scripts.Enums;
using UnityEngine;
using Random = UnityEngine.Random;

namespace StackAndCollect.MyStackAndCollect.Scripts
{
    /// <summary>
    /// Ref : Myself
    /// </summary>
    
    [RequireComponent(typeof(Collider))]
    public class BulkBehaviour : MonoBehaviour
    {
        public static event Action<CollectibleBehaviour> CollectibleAdded;
        public static event Action<CollectibleBehaviour> CollectibleRemoved;
        
        [Header("Preferences")]
        [SerializeField] protected Transform[] _collectibleBasePoints;
        [SerializeField] protected List<CollectibleBehaviour> _collectibles = new List<CollectibleBehaviour>();
        [SerializeField] protected ObjectName _objectName;

        [Header("Adjustable")] 
        [SerializeField] private float _verticalOffSetIncrement =0.1f;
        [SerializeField] private int _maxCollectilbeCount = 20;
        [SerializeField] private bool _isCollectibleBulk = false;
        [SerializeField] protected float _addDelay = 0.05f;
        [SerializeField] protected float _removeDelay = 0.05f;
        [SerializeField] protected float _jumpDuration = 0.4f;

        [SerializeField] private List<GameObject> _objectsToDisable = new List<GameObject>();

        // [Header("Collect Specified At Point")]
        // [SerializeField] private bool _collectAtOnce = true;
        // [SerializeField] private bool _collectAtSpecificPoint = false;
        // [SerializeField] private Transform _collectTargetTransform;

        public bool IsReady;
        public int CurrentCollectibleCount => _collectibles.Count;

        public int MaxCollectibleCount => _maxCollectilbeCount;
        
        public bool HasStack => _collectibles.Count > 0;

        public bool StackIsFull => _collectibles.Count >= _maxCollectilbeCount;

        private int _currentIndex = 0;
        private bool _isTriggerStay;
        private Collider _collider;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.enabled = true;
            
            IsReady = true;

            SetObjectToDisables(true);
        }

        protected void OnTriggerStay(Collider other)
        {
            if(!IsReady) return;
            if(!other.TryGetComponent(out PlayerStackController playerStackController)) return;
            
            CollectibleBehaviour collectible;
            
            _isTriggerStay = true;
            
            if (_isCollectibleBulk)
            {
                StartCoroutine(ActionWithDelay());
                
                IEnumerator ActionWithDelay()
                {
                    while (playerStackController.CanIncrease && HasStack)
                    {
                        IsReady = false;
                        playerStackController.IsAnimPlaying = true;

                        collectible = GetCollectible();
                        playerStackController.IncreaseWithAnimation(collectible,null);
                        yield return new WaitForSeconds(_removeDelay);
                    }

                    if (!IsReady)
                    {
                        yield return new WaitForSeconds(playerStackController.AnimationDuration);
                        IsReady = true;
                        playerStackController.IsAnimPlaying = false;
                    }
                }
            }
            else
            {
                StartCoroutine(ActionWithDelay());

                IEnumerator ActionWithDelay()
                {
                    while (playerStackController.CanDecrease && !StackIsFull)
                    {
                        IsReady = false;
                        playerStackController.IsAnimPlaying = true;
                        // TODO : Check following line !!!
                        // collectible = playerStackBehaviour.DecreaseStackWithAnimation(_objectName, GetAromaByObjectType(),
                        //     GetCurrentPos(), GetCurrentBaseTransform().rotation, transform, null);
                        // PutCollectible(collectible);
                        
                        // From cacao
                        // if (_includedAroma)
                        //     collectible = stack.DecreaseStackWithAnimation(_objectName, GetCurrentPos(),
                        //         GetCurrentBaseTransform().rotation, transform, null);
                        // else
                        //     collectible = stack.DecreaseStackWithAnimation(_objectName, GetAromaByObjectType(),
                        //         GetCurrentPos(), GetCurrentBaseTransform().rotation, transform, null);
                        // PutCollectible(collectible);

                        yield return new WaitForSeconds(_addDelay);
                    }
                    
                    if (!IsReady)
                    {
                        yield return new WaitForSeconds(playerStackController.AnimationDuration);
                        IsReady = true;
                        playerStackController.IsAnimPlaying = false;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(!other.TryGetComponent(out PlayerStackController playerStackController)) return;

            _isTriggerStay = false;
            
            SetObjectToDisables(_currentIndex <= 0);
        }

        protected virtual void SetCollectiblePosNRot(CollectibleBehaviour collectible)
        {
            collectible.transform.position = GetCurrentPos();
            collectible.transform.rotation = GetCurrentBaseTransform().rotation;
        }
        
        private void AddCollectible(CollectibleBehaviour collectible)
        {
            var collectibleTransform = collectible.transform;
            var targetPosition = GetCurrentPos();
            var targetRotation = GetCurrentBaseTransform().rotation;
            
            _collectibles.Add(collectible);
            _currentIndex++;
            
            SetObjectToDisables(!((_isTriggerStay && !_isCollectibleBulk) || (!_isTriggerStay && _isCollectibleBulk)));
            
            collectibleTransform.DORotateQuaternion(targetRotation, _jumpDuration);
            collectibleTransform.DOJump(targetPosition, 3f, 1, _jumpDuration).OnComplete(() =>
            {
                SetCollectiblePosNRot(collectible);
            });
            collectibleTransform.SetParent(transform);
            
            CollectibleAdded?.Invoke(collectible);
        }
        
        protected void RemoveCollectible(CollectibleBehaviour collectible)
        {
            _collectibles.Remove(collectible);
            _currentIndex--;
            
            if(_currentIndex == 0) SetObjectToDisables(true);
            
            CollectibleRemoved?.Invoke(collectible);
        }

        private void RemoveCollectible(CollectibleBehaviour collectible, BulkBehaviour newBulk)
        {
            RemoveCollectible(collectible);
            
            newBulk.PutCollectible(collectible);

            collectible.transform.position = newBulk.GetCurrentPos();
        }
        
        public void DisableCollectibles()
        {
            foreach (var collectible in _collectibles)
            {
                collectible.gameObject.SetActive(false);
            }
            
            _collectibles.Clear();
            _currentIndex = 0;
        }
        
        public CollectibleBehaviour GetCollectible()
        {
            if(!HasStack)
                return null;

            CollectibleBehaviour collectible = _collectibles.Last();

            collectible.transform.DOKill();
            RemoveCollectible(collectible);
            return collectible;
        }
        
        public void PutCollectible(CollectibleBehaviour collectible)
        {
            if (!collectible || StackIsFull) return;

            AddCollectible(collectible);
        }

        public void SetCollectibleBulk(bool value) => _isCollectibleBulk = value;
        
        public virtual Vector3 GetCurrentPos()
        {
            int heightMultiplier = _currentIndex / _collectibleBasePoints.Length;
            Vector3 position = GetCurrentBaseTransform().position;
            position.y += heightMultiplier * _verticalOffSetIncrement;
            return position;
        }

        public virtual Transform GetCurrentBaseTransform()
        {
            int index = _currentIndex % _collectibleBasePoints.Length;
            return _collectibleBasePoints[index];
        }

        protected void ResetIndex()
        {
            _currentIndex = 0;
        }
        
        private void SetObjectToDisables(bool status)
        {
            foreach (var bulkObject in _objectsToDisable)
            {
                bulkObject.SetActive(status);
            }
        }

        // public void StackObject(Transform objectToStack)
        // {
        //     if(!_isStackable) return;
        //     
        //     objectToStack.SetParent(transform);
        //     _collectibles.Add(objectToStack.gameObject);
        //
        //     var targetPoint = GetLastStackPointPosition();
        //     
        //     objectToStack.transform.DOJump(transform.position, 5f, 1, 0.5f).SetEase(Ease.Linear).OnComplete(() =>
        //     {
        //         objectToStack.transform.position = targetPoint;
        //     });
        //
        // }
        //
        // public GameObject GetStackedObject()
        // {
        //     if (!_isCollectable) return null;
        //     if (!HasStack) return null;
        //
        //     if (_collectAtOnce)
        //     {
        //         if (_collectAtSpecificPoint)
        //         {
        //             foreach (var child  in _collectibles)
        //             {
        //                 AnimateCollectAtSpecifiedPoint(child.transform);
        //             }    
        //         }
        //         else
        //         {
        //             foreach (var child  in _collectibles)
        //             {
        //                 return AnimateCollect(child.transform);
        //             }
        //         }
        //
        //         _collectibles.Clear();
        //     }
        //     else
        //     {
        //         
        //     }
        //     
        //
        //     return null;
        // }
        //
        //
        //
        // private Vector3 GetLastStackPointPosition()
        // {
        //     var x = (StackedObjectCount - 1) % _collectibleBasePoints.Length;
        //     var y = (StackedObjectCount - 1) / _collectibleBasePoints.Length;
        //     var target = new Vector3(_collectibleBasePoints[x].position.x, _collectibleBasePoints[x].position.y +
        //                                                              (y * _verticalOffSetIncrement),
        //         _collectibleBasePoints[x].position.z);
        //     return target;
        // }
        //
        // private GameObject AnimateCollect(Transform stackedObject)
        // {
        //     stackedObject.DOLocalRotate(Vector3.up * GetRandomFloatValue(-50f, 50f), 0.7f).SetEase(Ease.Linear);
        //     stackedObject.DOMove(new Vector3(stackedObject.transform.position.x + GetRandomFloatValue(-1f, 1f),
        //             stackedObject.transform.position.y + GetRandomFloatValue(0.7f, 1.5f),
        //             stackedObject.transform.position.z + GetRandomFloatValue(-1f, 1f)),
        //         0.7f).SetEase(Ease.OutBack).OnComplete(() =>
        //     {
        //         //return stackedObject.gameObject;
        //     });
        //     return stackedObject.gameObject;
        // }
        //
        // private void AnimateCollectAtSpecifiedPoint(Transform stackedObject)
        // {
        //     stackedObject.DOLocalRotate(Vector3.up * GetRandomFloatValue(-50f, 50f), 0.7f).SetEase(Ease.Linear);
        //     stackedObject.DOMove(new Vector3(stackedObject.transform.position.x + GetRandomFloatValue(-1f, 1f),
        //             stackedObject.transform.position.y + GetRandomFloatValue(0.7f, 1.5f),
        //             stackedObject.transform.position.z + GetRandomFloatValue(-1f, 1f)),
        //         0.7f).SetEase(Ease.OutBack).OnComplete(() =>
        //     {
        //         stackedObject.DOMove(_collectTargetTransform.position, 0.1f).OnComplete(() =>
        //         {
        //             Debug.Log("Send object back to Object Pool here !");
        //             Destroy(stackedObject.gameObject);
        //         });    
        //         
        //     });
        // }
        //
        // private float GetRandomFloatValue(float from, float to)
        // {
        //     return Random.Range(from, to);
        // }
        
    }
}
