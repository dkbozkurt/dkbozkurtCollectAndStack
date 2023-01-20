// using System;
// using System.Collections.Generic;
// using System.Linq;
// using CpiTemplate.Game.Scripts.Behaviours.Characters.Helpers;
// using CpiTemplate.Game.Scripts.Behaviours.Collectible;
// using CpiTemplate.Game.Scripts.Controllers;
// using DG.Tweening;
// using FIMSpace.FTail;
// using Game.Scripts.Models;
// using Plugins.Utility;
// using Sirenix.OdinInspector;
// using Sirenix.Utilities;
// using UnityEngine;
//
// namespace CpiTemplate.Game.Scripts.Behaviours.Stack
// {
//     public abstract class TailStackBehaviour : MonoBehaviour
//     {
//         [SerializeField] private float _animationDuration;
//         [SerializeField] private string _saveName;
//         [SerializeField] private bool _saveActive = true;
//
//         protected StackPieceBehaviour[] StackPieces;
//         private TailAnimator2 _tailAnimator;
//         private List<Transform> _stackPiecesTransforms = new List<Transform>();
//
//         protected int MaxStackCount { get; set; }
//         [HideInInspector] public int CurrentStackIndex; //next empty index
//
//         protected int MaxStackIndex { get; set; }
//         public bool CanIncrease => CurrentStackIndex <= MaxStackIndex;
//         public bool CanDecrease => CurrentStackIndex > 0;
//
//         protected readonly List<CollectibleBehaviour> _collectibles = new List<CollectibleBehaviour>();
//         private CollectableInformationList _collectibleInformation = new CollectableInformationList();
//         
//         protected abstract void OnStackIncreased(CollectibleBehaviour collectible);
//         protected abstract void OnStackDecreased(StackPieceBehaviour piece);
//         private StackController _stackController;
//
//         // [HideInInspector]public int StackedChocolateCount{ get; set; }
//
//         protected int startChocolateCount = 0;
//
//         protected virtual void Awake()
//         {
//             _stackController = GetComponentInParent<StackController>();
//             _tailAnimator = GetComponent<TailAnimator2>();
//             StackPieces = GetComponentsInChildren<StackPieceBehaviour>();
//             StackPieces.ForEach(x => _stackPiecesTransforms.Add(x.transform));
//             InitializeStack();
//         }
//
//         private void Start()
//         {
//             LoadStack();
//         }
//
//         public void LoadStack()
//         {
//             if(!_saveActive)
//                 return;
//             
//             if (!PlayerData.Instance.SavedStackData.Keys.Contains(_saveName))
//             {
//                 PlayerData.Instance.SavedStackData.Add(_saveName, _collectibleInformation);
//                 return;
//             }
//
//             _collectibleInformation = PlayerData.Instance.SavedStackData[_saveName];
//             if (_collectibleInformation.CollectibleList.Count == 0) return;
//             foreach (var collectableInformation in _collectibleInformation.CollectibleList)
//             {
//                 IncreaseStack(collectableInformation.ObjectName, collectableInformation.AromaType);
//                 if (collectableInformation.ObjectName == ObjectName.Chocolate)
//                 {
//                     startChocolateCount++;
//                     Debug.Log("initial chocolate");
//                 }
//             }
//
//             UpdateTailChains();
//         }
//
//         public void SaveStack()
//         {
//             if(!_saveActive)
//                 return;
//             
//             _collectibleInformation.CollectibleList.Clear();
//             StackPieces.ForEach(x =>
//             {
//                 if (x.IsOpened)
//                     _collectibleInformation.CollectibleList.Add(
//                         new CollectableInformation(x.ObjectName, x.GetCurrentCollectible().AromaType));
//             });
//             PlayerData.Instance.SavedStackData[_saveName] = _collectibleInformation;
//         }
//
//         private void IncreaseStack(ObjectName objectName, AromaType aroma)
//         {
//             StackPieces[CurrentStackIndex].OpenPiece(objectName, aroma);
//             IncreaseStackIndex();
//         }
//
//         protected virtual void InitializeStack()
//         {
//             MaxStackCount = StackPieces.Length;
//             MaxStackIndex = MaxStackCount - 1;
//             StackPieces.ForEach(x => x.CloseCurrentPiece());
//             CheckTailState();
//         }
//
//         [Button]
//         public void IncreaseStackWithAnimation(CollectibleBehaviour collectible, Action callback)
//         {
//             if (!CanIncrease)
//                 return;
//             
//             Transform collectedTransform = collectible.transform;
//             OnStackIncreased(collectible);
//             collectedTransform.DOKill();
//             //Vector3 jumpTarget = StackPieces[CurrentStackIndex].transform.localPosition;
//             int index = CurrentStackIndex;
//             collectedTransform.SetParent(StackPieces[CurrentStackIndex].transform);
//             collectedTransform.DOLocalRotateQuaternion(
//                 StackPieces[CurrentStackIndex].GetRotationByType(collectible.ObjectName),
//                 _animationDuration*0.5f);
//             collectedTransform.DOLocalJump(Vector3.zero, 5, 1, _animationDuration)
//                 .OnComplete(() => { OnJumpComplete(collectible, collectedTransform, index, callback); });
//
//             IncreaseStackIndex();
//         }
//
//         private void OnJumpComplete(CollectibleBehaviour collectible, Transform collectedTransform, int index,
//             Action callback)
//         {
//             collectedTransform.gameObject.SetActive(false);
//             collectedTransform.SetParent(ObjectPoolController.Instance.transform);
//             collectedTransform.localScale = Vector3.one;
//             TogglePiece(collectible, index, true);
//             callback?.Invoke();
//         }
//
//         [Button]
//         public CollectibleBehaviour DecreaseStackWithAnimation(ObjectName objectName, Vector3 jumpPos,
//             Quaternion rotation, Transform parent, Action callback)
//         {
//             if (!CanDecrease)
//                 return null;
//
//             StackPieceBehaviour piece = GetLastObject(objectName);
//             AromaType aroma = piece.GetCurrentCollectible().AromaType;
//             if (!piece)
//                 return null;
//             
//             DecreaseStack(piece);
//
//
//             Vector3 spawnPoint = piece.transform.position;
//             CollectibleBehaviour stackPiece = ObjectPoolController.Instance
//                 .SpawnObject(objectName, spawnPoint, Quaternion.identity).GetComponent<CollectibleBehaviour>();
//             stackPiece.ChangeAroma(aroma);
//             stackPiece.transform.SetParent(parent);
//             stackPiece.transform.DORotateQuaternion(rotation, _animationDuration);
//
//             float diffY = Mathf.Abs(spawnPoint.y - jumpPos.y);
//             float jumpPower = Mathf.Clamp(diffY, 2, 25);
//
//             stackPiece.transform.DOJump(jumpPos, jumpPower, 1, _animationDuration)
//                 .OnComplete(() => { callback?.Invoke(); });
//
//             return stackPiece;
//         }
//
//         public CollectibleBehaviour DecreaseStackWithAnimation(ObjectName objectName, AromaType aromaType,
//             Vector3 jumpPos, Quaternion rotation, Transform parent, Action callback)
//         {
//             if (!CanDecrease)
//                 return null;
//
//             StackPieceBehaviour piece = GetLastObject(objectName, aromaType);
//             AromaType aroma = piece.GetCurrentCollectible().AromaType;
//             // if (!piece)
//             //     return null;
//
//             DecreaseStack(piece);
//
//
//             Vector3 spawnPoint = piece.transform.position;
//             CollectibleBehaviour stackPiece = ObjectPoolController.Instance
//                 .SpawnObject(objectName, spawnPoint, Quaternion.identity).GetComponent<CollectibleBehaviour>();
//             stackPiece.ChangeAroma(aroma);
//             stackPiece.transform.SetParent(parent);
//             stackPiece.transform.DORotateQuaternion(rotation, _animationDuration);
//
//             float diffY = Mathf.Abs(spawnPoint.y - jumpPos.y);
//             float jumpPower = Mathf.Clamp(diffY, 2, 25);
//
//             stackPiece.transform.DOJump(jumpPos, jumpPower, 1, _animationDuration)
//                 .OnComplete(() => { callback?.Invoke(); });
//
//             return stackPiece;
//         }
//
//         public CollectibleBehaviour DecreaseStackWithAnimation(Vector3 jumpPos, Transform parent, Action callback)
//         {
//             if (!CanDecrease)
//                 return null;
//
//             StackPieceBehaviour piece = GetLastObject();
//             ObjectName objectName = piece.ObjectName;
//             AromaType aroma = piece.GetCurrentCollectible().AromaType;
//             if (!piece)
//                 return null;
//
//             DecreaseStack(piece);
//
//
//             Vector3 spawnPoint = piece.transform.position;
//             CollectibleBehaviour collectibleBehaviour = ObjectPoolController.Instance
//                 .SpawnObject(objectName, spawnPoint, Quaternion.identity).GetComponent<CollectibleBehaviour>();
//             collectibleBehaviour.ChangeAroma(aroma);
//             collectibleBehaviour.transform.SetParent(parent);
//
//             float diffY = Mathf.Abs(spawnPoint.y - jumpPos.y);
//             float jumpPower = Mathf.Clamp(diffY, 2, 25);
//
//             collectibleBehaviour.transform.DOJump(jumpPos, jumpPower, 1, _animationDuration)
//                 .OnComplete(() =>
//                 {
//                     collectibleBehaviour.gameObject.SetActive(false);
//                     callback?.Invoke();
//                 });
//
//             return collectibleBehaviour;
//         }
//
//         [Button]
//         public void IncreaseStack(CollectibleBehaviour collectible)
//         {
//             if (!CanIncrease)
//                 return;
//             
//             TogglePiece(collectible, CurrentStackIndex, true);
//             CurrentStackIndex++;
//         }
//
//         private void IncreaseStackIndex()
//         {
//             if (!CanIncrease)
//                 return;
//             CurrentStackIndex++;
//         }
//
//         [Button]
//         private void DecreaseStack(StackPieceBehaviour piece)
//         {
//             if (!CanDecrease)
//                 return;
//             
//             piece.CloseCurrentPiece();
//             OnStackDecreased(piece);
//             _stackController.UpdateTails();
//         }
//
//         private void TogglePiece(CollectibleBehaviour collectible, int index, bool state)
//         {
//             if (state)
//             {
//                 StackPieces[index].OpenPiece(collectible);
//             }
//             else
//             {
//                 StackPieces[index].CloseCurrentPiece();
//             }
//
//             CheckTailState();
//             UpdateTailChains();
//             SaveStack();
//         }
//
//         protected void CheckTailState()
//         {
//             //_tailAnimator.enabled = CanDecrease;
//         }
//
//         public bool IsFull() => !CanIncrease;
//
//         public bool HasObject(ObjectName objectName, bool includeAroma)
//         {
//             return StackPieces.LastOrDefault(x => x.ObjectName == objectName && x.IsOpened
//                                                                              && AromaCheck(x, objectName,
//                                                                                  includeAroma));
//         }
//
//         public bool HasObject(ObjectName objectName, AromaType aromaType)
//         {
//             return StackPieces.LastOrDefault(x => x.ObjectName == objectName && x.IsOpened);
//         }
//
//         private bool AromaCheck(StackPieceBehaviour piece, ObjectName objectName, bool includeAroma)
//         {
//             if (includeAroma)
//                 return true;
//
//             if (objectName != ObjectName.Chocolate && objectName != ObjectName.ChocolateBar)
//                 return true;
//
//             CollectibleBehaviour collectible = piece.GetCurrentCollectible();
//             return collectible.AromaType == AromaType.Normal;
//         }
//
//         public StackPieceBehaviour GetLastObject()
//         {
//             return StackPieces.LastOrDefault(x => x.IsOpened);
//         }
//
//         public StackPieceBehaviour GetLastObject(ObjectName objectName)
//         {
//             return StackPieces.LastOrDefault(x => x.ObjectName == objectName && x.IsOpened);
//         }
//
//         public StackPieceBehaviour GetLastObject(ObjectName objectName, AromaType aromaType)
//         {
//             return StackPieces.LastOrDefault(x =>
//                 x.ObjectName == objectName && x.IsOpened);
//         }
//
//         public List<CollectibleBehaviour> GetCurrentCollectibles()
//         {
//             _collectibles.Clear();
//             for (int i = 0; i < StackPieces.Length; i++)
//             {
//                 if (StackPieces[i].IsOpened)
//                     _collectibles.Add(StackPieces[i].GetCurrentCollectible());
//             }
//
//             return _collectibles;
//         }
//
//         public void ResetIndex()
//         {
//             CurrentStackIndex = 0;
//             StackPieces.ForEach(x => x.CloseCurrentPiece());
//         }
//
//         public void UpdateTailChains()
//         {
//             _tailAnimator.User_SetTailTransforms(_stackPiecesTransforms);
//             _tailAnimator.RefreshTransformsList();
//         }
//
//         // public int CalculateStackedChocolate()
//         // {
//         //     StackedChocolateCount = 0;
//         //     for (int i = 0; i < _collectibles.Count ; i++)
//         //     {
//         //         if (_collectibles[i].ObjectName == ObjectName.Chocolate)
//         //         {
//         //             Debug.Log("cokocoko");
//         //             StackedChocolateCount++;
//         //         }
//         //     }
//         //     Debug.Log("stacked chocolate count "+ StackedChocolateCount);
//         //     return StackedChocolateCount;
//         // }
//     }
// }