// using System;
// using CpiTemplate.Game.Scripts.Behaviours.Collectible;
// using CpiTemplate.Game.Scripts.Controllers;
// using RocketUtils.SerializableDictionary;
// using UnityEngine;
//
// namespace CpiTemplate.Game.Scripts.Behaviours.Stack
// {
//     public class StackPieceBehaviour : MonoBehaviour
//     {
//         public Transform ModelTransform => _pieceDictionary[_currentObjectName].Model;
//         
//         private ObjectName _currentObjectName;
//         public ObjectName ObjectName => _currentObjectName;
//         public bool IsOpened { get; private set; }
//         [Serializable] public class PieceProperties
//         {
//             public CollectibleBehaviour Collectible;
//             public Transform Model;
//             public float Offset;
//         }
//         [Serializable] public class PieceDictionary : SerializableDictionary<ObjectName, PieceProperties> { }
//
//         [SerializeField] private PieceDictionary _pieceDictionary = new PieceDictionary();
//         private StackPieceBehaviour _lowerPiece;
//
//         private void Awake()
//         {
//             _lowerPiece = transform.parent.GetComponent<StackPieceBehaviour>();
//         }
//
//         public void OpenPiece(CollectibleBehaviour collectible)
//         {
//             CloseCurrentPiece();
//             IsOpened = true;
//             _currentObjectName = collectible.ObjectName;
//             _pieceDictionary[_currentObjectName].Model.gameObject.SetActive(true);
//             _pieceDictionary[_currentObjectName].Collectible.ChangeAroma(collectible.AromaType);
//             UpdateOffset();
//         }
//         
//         public void OpenPiece(ObjectName objectName, AromaType aroma)
//         {
//             //CloseCurrentPiece();
//             IsOpened = true;
//             _currentObjectName = objectName;
//             _pieceDictionary[_currentObjectName].Model.gameObject.SetActive(true);
//             _pieceDictionary[_currentObjectName].Collectible.ChangeAroma(aroma);
//             UpdateOffset();
//         }
//         
//         public void CloseCurrentPiece()
//         {
//             IsOpened = false;
//             _pieceDictionary[_currentObjectName].Model.gameObject.SetActive(false);
//             UpdateOffset();
//         }
//
//         public float GetCurrentOffset()
//         {
//             return _pieceDictionary[_currentObjectName].Offset;
//         }
//
//         private void UpdateOffset()
//         {
//             Vector3 targetLocalPos = Vector3.zero;
//             if (_lowerPiece)
//                 targetLocalPos.y = GetCurrentOffset() + _lowerPiece.GetCurrentOffset();
//             
//             transform.localPosition = targetLocalPos;
//             transform.localRotation = Quaternion.identity;
//         }
//
//         public CollectibleBehaviour GetCurrentCollectible()
//         {
//             return _pieceDictionary[_currentObjectName].Collectible;
//         }
//
//         public Quaternion GetRotationByType(ObjectName objectName)
//         {
//             return _pieceDictionary[objectName].Model.localRotation;
//         }
//         
//     }
// }
