// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using CpiTemplate.Game.Scripts.Behaviours.Stack;
// using CpiTemplate.Game.Scripts.Controllers;
// using DG.Tweening;
// using Game.Scripts.Models;
// using Plugins.Utility;
// using Sirenix.OdinInspector;
// using StackAndCollect.MyStackAndCollect.Scripts;
// using UnityEngine;
// using Random = UnityEngine.Random;
//
// namespace CpiTemplate.Game.Scripts.Behaviours.Collectible
// {
//     public class CollectibleBulkBehaviour : MonoBehaviour
//     {
//         public Action<CollectibleBehaviour> CollectibleAdded;
//         public Action<CollectibleBehaviour> CollectibleRemoved;
//
//         [Header("Settings")] [SerializeField] protected Transform[] _collectibleBasePoints;
//         [SerializeField] protected List<CollectibleBehaviour> _collectibles = new List<CollectibleBehaviour>();
//         [SerializeField] protected ObjectName _objectName;
//         [SerializeField] protected string _saveName;
//
//         [Header("Adjustable")] [SerializeField]
//         private float _offsetIncrement;
//
//         [SerializeField] private int _maxCollectibleCount;
//         [SerializeField] private bool _collectibleBulk;
//         [SerializeField] protected float _addDelay;
//         [SerializeField] protected float _removeDelay;
//
//         [HideIf("_collectibleBulk")] [SerializeField]
//         protected bool _includedAroma;
//
//         [SerializeField] private List<GameObject> _objectsToDisable = new List<GameObject>();
//
//         private CollectableInformationList _collectibleInformationList = new CollectableInformationList();
//         private Collider _collider;
//
//         private int _currentIndex = 0;
//
//         public bool IsReady;
//         public int CurrentCollectibleCount => _collectibles.Count;
//         public int MaxCollectibleCount => _maxCollectibleCount;
//         public bool HasStack => _collectibles.Count > 0;
//         public bool StackIsFull => _collectibles.Count >= _maxCollectibleCount;
//
//         private bool _isTriggerStay;
//
//         private void Awake()
//         {
//             _collider = GetComponent<Collider>();
//             _collider.enabled = false;
//
//             IsReady = true;
//             DOVirtual.DelayedCall(2.2f, () => _collider.enabled = true);
//             if (!PlayerData.Instance.SavedStackData.Keys.Contains(_saveName))
//             {
//                 PlayerData.Instance.SavedStackData.Add(_saveName, _collectibleInformationList);
//                 return;
//             }
//
//             _collectibleInformationList = PlayerData.Instance.SavedStackData[_saveName];
//             if (_collectibleInformationList.CollectibleList.Count == 0) return;
//             foreach (var bulkObject in _objectsToDisable)
//             {
//                 bulkObject.SetActive(false);
//             }
//
//             foreach (var collectableInformation in _collectibleInformationList.CollectibleList)
//             {
//                 var collectable = ObjectPoolController.Instance
//                     .SpawnObject(collectableInformation.ObjectName, transform).GetComponent<CollectibleBehaviour>();
//                 collectable.ChangeAroma(collectableInformation.AromaType);
//                 SetCollectiblePosNRot(collectable);
//                 AddCollectible(collectable, true);
//             }
//         }
//
//         protected virtual void SetCollectiblePosNRot(CollectibleBehaviour collectable)
//         {
//             collectable.transform.position = GetCurrentPos();
//             collectable.transform.rotation = GetCurrentBaseTransform().rotation;
//         }
//
//         private void AddCollectible(CollectibleBehaviour collectible, bool isLoading = false)
//         {
//             _collectibles.Add(collectible);
//             _currentIndex++;
//
//             if (_objectsToDisable.Count > 0)
//             {
//                 if (_isTriggerStay && !_collectibleBulk)
//                 {
//                     foreach (var bulkObject in _objectsToDisable)
//                     {
//                         bulkObject.SetActive(false);
//                     }
//                 }
//
//                 if (!_isTriggerStay && _collectibleBulk)
//                 {
//                     foreach (var bulkObject in _objectsToDisable)
//                     {
//                         bulkObject.SetActive(false);
//                     }
//                 }
//             }
//
//             if (!isLoading)
//             {
//                 SaveData();
//             }
//
//             CollectibleAdded?.Invoke(collectible);
//         }
//
//         private void SaveData()
//         {
//             _collectibleInformationList.CollectibleList.Clear();
//             _collectibles.ForEach(x =>
//             {
//                 _collectibleInformationList.CollectibleList.Add(
//                     new CollectableInformation(x.ObjectName, x.AromaType));
//             });
//
//             PlayerData.Instance.SavedStackData[_saveName] = _collectibleInformationList;
//         }
//
//         protected void RemoveCollectible(CollectibleBehaviour collectible)
//         {
//             _collectibles.Remove(collectible);
//             _currentIndex--;
//
//             if (_currentIndex == 0 && _objectsToDisable.Count > 0)
//             {
//                 foreach (var bulkObject in _objectsToDisable)
//                 {
//                     bulkObject.SetActive(true);
//                 }
//             }
//
//             SaveData();
//
//             CollectibleRemoved?.Invoke(collectible);
//         }
//
//         private void RemoveCollectible(CollectibleBehaviour collectible, CollectibleBulkBehaviour newBulk)
//         {
//             RemoveCollectible(collectible);
//
//             newBulk.PutCollectible(collectible);
//
//             collectible.transform.position = newBulk.GetCurrentPos();
//         }
//
//         public void DisableCollectibles()
//         {
//             foreach (var collectible in _collectibles)
//             {
//                 collectible.gameObject.SetActive(false);
//             }
//
//             _collectibles.Clear();
//             _currentIndex = 0;
//         }
//
//         public CollectibleBehaviour GetCollectible()
//         {
//             if (!HasStack)
//                 return null;
//
//             CollectibleBehaviour collectible = _collectibles.Last();
//
//
//             collectible.transform.DOKill();
//             RemoveCollectible(collectible);
//             return collectible;
//         }
//
//         public CollectibleBehaviour GetCollectible(AromaType aroma)
//         {
//             if (!HasStack)
//                 return null;
//
//             CollectibleBehaviour collectible = _collectibles.LastOrDefault(x => x.AromaType == aroma);
//             if (!collectible)
//                 return null;
//
//             collectible.transform.DOKill();
//             RemoveCollectible(collectible);
//             return collectible;
//         }
//
//         public void PutCollectible(CollectibleBehaviour collectible)
//         {
//             if (!collectible || StackIsFull)
//                 return;
//
//             AddCollectible(collectible);
//         }
//
//         public void SetCollectibleBulk(bool value) => _collectibleBulk = value;
//
//
//         public virtual Vector3 GetCurrentPos()
//         {
//             int heightMultiplier = _currentIndex / _collectibleBasePoints.Length;
//             Vector3 position = GetCurrentBaseTransform().position;
//             position.y += heightMultiplier * _offsetIncrement;
//             return position;
//         }
//
//         public virtual Transform GetCurrentBaseTransform()
//         {
//             int index = _currentIndex % _collectibleBasePoints.Length;
//             return _collectibleBasePoints[index];
//         }
//
//         public void LoadAllToAnotherBulk(CollectibleBulkBehaviour bulkBehaviour)
//         {
//             foreach (var collectible in _collectibles)
//             {
//                 RemoveCollectible(collectible, bulkBehaviour);
//             }
//         }
//
//         public int GetCountByAroma(AromaType aromaType)
//         {
//             return _collectibles.Count(x => x.AromaType == aromaType);
//         }
//
//         protected void ResetIndex()
//         {
//             _currentIndex = 0;
//         }
//
//         protected virtual void OnTriggerStay(Collider other)
//         {
//             if (!IsReady) return;
//             if (!other.TryGetComponent(out StackController stackController)) return;
//
//             TailStackBehaviour stack;
//
//             CollectibleBehaviour collectible;
//             _isTriggerStay = true;
//             if (_collectibleBulk)
//             {
//                 stack = stackController.GetStackForIncrease(_objectName, gameObject.GetInstanceID());
//
//                 if (!stack)
//                     return;
//
//                 StartCoroutine(ActionWithDelay());
//
//                 IEnumerator ActionWithDelay()
//                 {
//                     while (stack && stack.CanIncrease && HasStack)
//                     {
//                         IsReady = false;
//                         stackController.IsAnimPlaying = true;
//
//                         collectible = GetCollectible();
//                         stack.IncreaseStackWithAnimation(collectible, null);
//                         yield return new WaitForSeconds(_removeDelay);
//                         stack = stackController.GetStackForIncrease(_objectName, gameObject.GetInstanceID());
//                     }
//
//                     if (!IsReady)
//                     {
//                         yield return new WaitForSeconds(stackController.AnimationDuration);
//                         IsReady = true;
//                         stackController.IsAnimPlaying = false;
//                     }
//                 }
//             }
//             else
//             {
//                 stack = stackController.GetStackForDecrease(_objectName, _includedAroma,
//                     gameObject.GetInstanceID());
//
//                 if (!stack)
//                     return;
//
//                 StartCoroutine(ActionWithDelay());
//
//                 IEnumerator ActionWithDelay()
//                 {
//                     while (stack && stack.CanDecrease && !StackIsFull)
//                     {
//                         IsReady = false;
//                         stackController.IsAnimPlaying = true;
//                         if (_includedAroma)
//                             collectible = stack.DecreaseStackWithAnimation(_objectName, GetCurrentPos(),
//                                 GetCurrentBaseTransform().rotation, transform, null);
//                         else
//                             collectible = stack.DecreaseStackWithAnimation(_objectName, GetAromaByObjectType(),
//                                 GetCurrentPos(), GetCurrentBaseTransform().rotation, transform, null);
//                         PutCollectible(collectible);
//
//                         yield return new WaitForSeconds(_addDelay);
//                         stack = stackController.GetStackForDecrease(_objectName, _includedAroma,
//                             gameObject.GetInstanceID());
//                     }
//
//                     if (!IsReady)
//                     {
//                         yield return new WaitForSeconds(stackController.AnimationDuration);
//                         IsReady = true;
//                         stackController.IsAnimPlaying = false;
//                     }
//                 }
//             }
//         }
//
//         private AromaType GetAromaByObjectType()
//         {
//             switch (_objectName)
//             {
//                 case ObjectName.Strawberry:
//                     return AromaType.Strawberry;
//                 case ObjectName.Milk:
//                     return AromaType.Milk;
//                 case ObjectName.Banana:
//                     return AromaType.Banana;
//                 default:
//                     return AromaType.Normal;
//             }
//         }
//
//         protected virtual void OnTriggerExit(Collider other)
//         {
//             if (!other.TryGetComponent(out PlayerStackBehaviour playerStackController)) return;
//
//             _isTriggerStay = false;
//             if (_currentIndex > 0)
//             {
//                 foreach (var bulkObject in _objectsToDisable)
//                 {
//                     bulkObject.SetActive(false);
//                 }
//             }
//             else
//             {
//                 foreach (var bulkObject in _objectsToDisable)
//                 {
//                     bulkObject.SetActive(true);
//                 }
//             }
//         }
//     }
// }