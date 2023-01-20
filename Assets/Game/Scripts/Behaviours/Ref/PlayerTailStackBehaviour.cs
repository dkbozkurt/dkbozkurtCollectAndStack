// using System;
// using CpiTemplate.Game.Scripts.Behaviours.Collectible;
// using CpiTemplate.Game.Scripts.Controllers;
// using DG.Tweening;
// using UnityEngine;
//
// namespace CpiTemplate.Game.Scripts.Behaviours.Stack
// {
//     public class PlayerTailStackBehaviour : TailStackBehaviour
//     {
//         public static Action PackageReady;
//         public static Action TookChocolate;
//         public static Action TookBale;
//         public static Action TookMilk;
//
//         public int StackedChocolateCount { get; set; }
//
//
//         private void OnEnable()
//         {
//             DOVirtual.DelayedCall(0.5f, () =>
//             {
//                 StackedChocolateCount = startChocolateCount;
//                 Debug.Log("stacked chocolate : " + StackedChocolateCount);
//             });
//         }
//
//         protected override void OnStackIncreased(CollectibleBehaviour collectible)
//         {
//             if (collectible.ObjectName == ObjectName.ChocolateBar) PackageReady?.Invoke();
//             if (collectible.ObjectName == ObjectName.Chocolate)
//             {
//                 StackedChocolateCount++;
//                 TookChocolate?.Invoke();
//             }
//             if (collectible.ObjectName == ObjectName.Bale) TookBale?.Invoke();
//             if (collectible.ObjectName == ObjectName.Milk) TookMilk?.Invoke();
//         }
//         
//         protected override void OnStackDecreased(StackPieceBehaviour piece)
//         {
//             if (piece.ObjectName == ObjectName.Chocolate)
//             {
//                 StackedChocolateCount--;
//             }
//             // HapticController.Instance.StartSoftHaptic();
//         }
//     }
// }
