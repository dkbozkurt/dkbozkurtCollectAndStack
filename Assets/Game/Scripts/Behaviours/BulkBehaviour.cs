// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
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
        [Header("Preferences")]
        [SerializeField] private Transform[] _collectibleBasePoints;
        [SerializeField] protected List<GameObject> _collectibles = new List<GameObject>();
        
        [Header("Adjustable")] 
        [SerializeField] private float _verticalOffSetIncrement =1f;
        [SerializeField] private int _maxCollectilbeCount = 20;
        [SerializeField] private bool _isCollectibleBulk = false;
        [SerializeField] private float _preProcessDelay = 0.5f;
        [SerializeField] private float _addDelay = 0.5f;
        [SerializeField] private float _removeDelay = 0.5f;
        [SerializeField] private float _jumpDuration = 0.4f;
        [SerializeField] private float _jumpPower = 3f;

        [SerializeField] private List<GameObject> _objectToDisable = new List<GameObject>();

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

        private void Awake()
        {
            IsReady = true;
        }

        protected void OnTriggerStay(Collider other)
        {
            if(!IsReady) return;
            if(!other.TryGetComponent(out PlayerStackBehaviour playerStackBehaviour)) return;
            
            GameObject collectible;
            
            _isTriggerStay = true;
            
            if (_isCollectibleBulk)
            {
                StartCoroutine(ActionWithDelay());
                
                IEnumerator ActionWithDelay()
                {
                    while (playerStackBehaviour.CanIncrease && HasStack)
                    {
                        IsReady = false;
                        playerStackBehaviour.IsAnimPlaying = true;

                        collectible = GetCollectible();
                        playerStackBehaviour.IncreaseWithAnimation(collectible,null);
                        yield return new WaitForSeconds(_removeDelay);
                    }

                    if (!IsReady)
                    {
                        yield return new WaitForSeconds(playerStackBehaviour.AnimationDuration);
                        IsReady = true;
                        playerStackBehaviour.IsAnimPlaying = false;
                    }
                }
            }
            else
            {
                StartCoroutine(ActionWithDelay());

                IEnumerator ActionWithDelay()
                {
                    while (playerStackBehaviour.CanDecrease && !StackIsFull)
                    {
                        IsReady = false;
                        playerStackBehaviour.IsAnimPlaying = true;
                        // TODO : Check following line !!!
                        // collectible = playerStackBehaviour.DecreaseStackWithAnimation(_objectName, GetAromaByObjectType(),
                        //     GetCurrentPos(), GetCurrentBaseTransform().rotation, transform, null);
                        // PutCollectible(collectible);

                        yield return new WaitForSeconds(_addDelay);
                    }
                    
                    if (!IsReady)
                    {
                        yield return new WaitForSeconds(playerStackBehaviour.AnimationDuration);
                        IsReady = true;
                        playerStackBehaviour.IsAnimPlaying = false;
                    }
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if(!other.TryGetComponent(out PlayerStackBehaviour playerStackBehaviour)) return;

            _isTriggerStay = false;
            
        }

        public void SetCollectibleBulk(bool value) => _isCollectibleBulk = value;
        public void PutCollectible(GameObject collectible)
        {
            if (!collectible || StackIsFull)
            {
                return;
            }
            
            AddCollectible(collectible);
        }
        
        public GameObject GetCollectible()
        {
            if(!HasStack)
                return null;

            GameObject collectible = _collectibles.Last();

            collectible.transform.DOKill();
            RemoveCollectible(collectible);
            return collectible;
        }
        
        private void AddCollectible(GameObject collectible)
        {
            var collectibleTransform = collectible.transform;
            var targetPosition = GetCurrentPos();
            var targetRotation = GetCurrentBaseTransform().rotation;
            
            _collectibles.Add(collectible);
            _currentIndex++;
            
            SetObjectToDisables(!((_isTriggerStay && !_isCollectibleBulk) || (!_isTriggerStay && _isCollectibleBulk)));
            
            collectibleTransform.DORotateQuaternion(targetRotation, _jumpDuration);
            collectibleTransform.DOJump(targetPosition, _jumpPower, 1, _jumpDuration).OnComplete(() =>
            {
                SetCollectiblePosNRot(collectible,targetPosition,targetRotation);
            });
            collectibleTransform.SetParent(transform);
        }

        protected void RemoveCollectible(GameObject collectible)
        {
            _collectibles.Remove(collectible);
            _currentIndex--;
        }

        private void RemoveCollectible(GameObject collectible, BulkBehaviour newBulk)
        {
            RemoveCollectible(collectible);
            
            newBulk.PutCollectible(collectible);
        }

        public void DisableCollectibles()
        {
            foreach (var collectible in _collectibles)
            {
                Debug.Log("Add back to pool !!");
                collectible.gameObject.SetActive(false);
            }
            _collectibles.Clear();
            _currentIndex = 0;
        }
        
        protected virtual void SetCollectiblePosNRot(GameObject collectible,Vector3 targetPos, Quaternion targetRot)
        {
            collectible.transform.position = targetPos;
            collectible.transform.rotation = targetRot;
        }
        
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

        private void SetObjectToDisables(bool status)
        {
            foreach (var bulkObject in _objectToDisable)
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
