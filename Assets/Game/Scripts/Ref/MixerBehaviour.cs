// using System;
// using System.Collections;
// using System.Collections.Generic;
// using System.Linq;
// using CpiTemplate.Game.Creative.Scripts.Behaviours;
// using CpiTemplate.Game.Scripts.Behaviours.Collectible;
// using CpiTemplate.Game.Scripts.Controllers;
// using CpiTemplate.Game.Scripts.Managers;
// using DG.Tweening;
// using UnityEngine;
// namespace CpiTemplate.Game.Scripts.Behaviours
// {
//     public class MixerBehaviour : MonoBehaviour
//     {
//         public static event Action<bool> OnComponentsLoaded;
//         public Action MixedChocolate;
//         
//         [Header("References")]
//         [SerializeField] private CollectibleBulkBehaviour _inputBulk;
//         [SerializeField] private List<CollectibleBulkBehaviour> _aromaBulks;
//         [SerializeField] private CollectibleBulkBehaviour _outputBulk;
//         [SerializeField] private Transform _jumpTarget;
//         [SerializeField] private Transform _mixer;
//         [SerializeField] private ParticleSystem _particle;
//         [Header("Adjustments")]
//         [SerializeField] private float _startDelay = 3f;
//         [SerializeField] private float _progressDelay = 2f;
//         [SerializeField] private float _jumpPower = 3f;
//         [SerializeField] private float _jumpDuration = .4f;
//         [SerializeField] private MixerType _mixerType;
//         [SerializeField] private Transform _tutorialIndicator;
//
//         public bool IsReady { get; private set; }
//         public MixerType MixerType => _mixerType;
//         public Transform TutorialIndicator => _tutorialIndicator;
//
//         private void Awake()
//         {
//             DOVirtual.DelayedCall(2, Initialize);
//         }
//
//         public void Initialize()
//         {
//             _mixer.DOLocalRotate(new Vector3(0, 360, 0), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
//             IsReady = true;
//             StartCoroutine(ProcessRoutine());
//         }
//
//         private IEnumerator ProcessRoutine()
//         {
//             while (true)
//             {
//                 yield return new WaitForSeconds(_startDelay);
//                 // CollectibleBulkBehaviour aromaBulk = GetRandomBulk();
//                 
//                 if (IsReady && (_outputBulk.StackIsFull || !_inputBulk.HasStack))
//                 {
//                     CreativeBakerBehaviour.Instance.CanMixTheBowlSetter(false);
//                     if(CreativeBakerBehaviour.Instance.IsAnimationPlaying) CreativeBakerBehaviour.Instance.StopMixUpAnimation();
//                 }
//                 if (_inputBulk.HasStack)
//                 {
//                     CreativeBakerBehaviour.Instance.CanMixTheBowlSetter(true);
//                 }
//                 
//                 if (IsReady && !_outputBulk.StackIsFull && _inputBulk.HasStack && CreativeBakerBehaviour.Instance.CanMixTheBowl())
//                 {
//                     
//                     IsReady = false;
//                     // CreativeMixUpButton.Instance.CanMix = false;
//                     CollectibleBehaviour cacao = _inputBulk.GetCollectible();
//                     //CollectibleBehaviour aroma = aromaBulk.GetCollectible();
//
//                     if (!CreativeBakerBehaviour.Instance.IsAnimationPlaying) CreativeBakerBehaviour.Instance.StartMixUpAnimation();
//
//                     StartProcess(cacao);
//                 }
//             }
//         }
//
//         private void StartProcess(CollectibleBehaviour cacao)
//         {
//             cacao.transform.DOJump(_jumpTarget.position, _jumpPower, 1, .45f).OnComplete(() =>
//             {
//                 cacao.gameObject.SetActive(false);
//             });
//             DOVirtual.DelayedCall(_progressDelay, () =>
//             {
//                 IsReady = true;
//                 _particle.Play();
//                 //cacao.gameObject.SetActive(true);
//                 //cacao.ChangeAroma(aroma.AromaType);
//
//                 CollectibleBehaviour chocolate = ObjectPoolController.Instance.SpawnObject(ObjectName.Chocolate,
//                     cacao.transform.position, Quaternion.identity).GetComponent<CollectibleBehaviour>();
//                 
//                 chocolate.transform.DOJump(_outputBulk.GetCurrentPos(), _jumpPower, 1, _jumpDuration);
//                 chocolate.transform.DORotateQuaternion(_outputBulk.GetCurrentBaseTransform().rotation, _jumpDuration);
//                 chocolate.transform.parent = _outputBulk.transform;
//                 _outputBulk.PutCollectible(chocolate);
//                 MixedChocolate?.Invoke();
//                 _particle.Stop();
//             });
//             
//         }
//
//         private CollectibleBulkBehaviour GetRandomBulk()
//         {
//             _aromaBulks = _aromaBulks.OrderBy(a => Guid.NewGuid()).ToList();
//             CollectibleBulkBehaviour bulk = _aromaBulks.FirstOrDefault(x => x.HasStack);
//             return bulk;
//         }
//
//         private void OnDisable()
//         {
//             _mixer.DOKill();
//             StopAllCoroutines();
//         }
//     }
//
//     [Serializable]
//     public enum MixerType
//     {
//         Strawberry,
//         Milk,
//         Orange,
//         Banana,
//     }
// }
