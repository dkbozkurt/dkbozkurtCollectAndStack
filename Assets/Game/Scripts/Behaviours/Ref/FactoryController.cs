// using System;
// using System.Collections;
// using System.Collections.Generic;
// using CpiTemplate.Game.Scripts.Behaviours;
// using CpiTemplate.Game.Scripts.Behaviours.Collectible;
// using DG.Tweening;
// using Facebook.Unity;
// using Game.Scripts.Models;
// using TMPro;
// using Unity.Mathematics;
// using UnityEngine;
//
// namespace CpiTemplate.Game.Scripts.Controllers
// {
//     public class FactoryController : MonoBehaviour
//     {
//         public static Action TookSeeds,ChocolateReady;
//         public static Action ReadyForTakeChocolate;
//         [SerializeField] private string _factoryId;
//
//         [SerializeField] private int _unlockID;
//
//         [Header("References")] [SerializeField]
//         protected CollectibleBulkBehaviour _inputBulk;
//
//         [SerializeField] protected CollectibleBulkBehaviour _outputBulk;
//         [SerializeField] protected List<Transform> _processPoints;
//         [SerializeField] private List<Renderer> _beltRenderers;
//
//         [Header("Adjustable")] [SerializeField]
//         private float _startDelay = 3f;
//
//         [SerializeField] protected float _jumpDuration = .5f;
//         [SerializeField] private float _moveSpeed;
//         [SerializeField] private float _beltAnimSpeed;
//         [Header("MachineV1")] [SerializeField] protected Transform _hammer;
//         [SerializeField] protected Vector3 _hammerTargetLocalPos;
//         [SerializeField] protected ParticleSystem _hammerParticle;
//
//         [Header("MachineV2")] [SerializeField] private ParticleSystem _furnaceParticleSmoke;
//
//         // [Header("MachineV3")]
//         // [SerializeField] private Transform _shredderCylinder1;
//         // [SerializeField] private Transform _shredderCylinder2;
//         // [SerializeField] private ParticleSystem _shredderParticle;
//         [Header("MachineV3")] [SerializeField] protected Transform _mold;
//         [SerializeField] protected Vector3 _moldTargetLocalPos;
//         [SerializeField] protected ParticleSystem _moldParticle;
//
//         [Header("Cooldown")] [SerializeField] protected float _cooldownDuration;
//         [SerializeField] private TextMeshProUGUI _cooldownText;
//
//
//         public Transform InputTransform => _inputBulk.transform;
//         public Transform OutputTransform => _outputBulk.transform;
//         protected bool IsReady { get; set; }
//         public int InputBulkItemCount => _inputBulk.CurrentCollectibleCount;
//         public int OutputBulkItemCount => _outputBulk.CurrentCollectibleCount;
//
//         private static readonly int _baseMap = Shader.PropertyToID("_BaseMap");
//
//         protected int _inProcessCount = 0;
//         protected Vector3 _hammerInitialLocalPos;
//         protected Vector3 _moldInitialLocalPos;
//         private Vector2 _offsetVec = new Vector2(0, 1);
//
//         protected int _producedProductCount = 0;
//         protected int _maxProductionCount = 10;
//         
//
//         protected void Awake()
//         {
//             _hammerInitialLocalPos = _hammer.localPosition;
//             _moldInitialLocalPos = _mold.localPosition;
//         }
//
//         public void Initialize()
//         {
//             // _shredderCylinder1.DOLocalRotate(new Vector3(0, 0, 360), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
//             // _shredderCylinder2.DOLocalRotate(new Vector3(0, 0, -360), 1f, RotateMode.LocalAxisAdd).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
//             IsReady = true;
//             LoadFactoryCooldown();
//             _furnaceParticleSmoke.Play();
//             // _shredderParticle.Play();
//
//             DOVirtual.DelayedCall(2f, () => _inputBulk.IsReady = true);
//             StartCoroutine(ProcessCoroutine());
//             StartCoroutine(BeltAnimationRoutine());
//         }
//
//         protected virtual IEnumerator ProcessCoroutine()
//         {
//             while (true)
//             {
//                 yield return new WaitForSeconds(_startDelay);
//                 bool outPutStackFull = _outputBulk.CurrentCollectibleCount + _inProcessCount >=
//                                        _outputBulk.MaxCollectibleCount;
//                 if (IsReady && !outPutStackFull && _inputBulk.HasStack)
//                 {
//                     _inProcessCount++;
//                     CollectibleBehaviour collectible = _inputBulk.GetCollectible();
//                     TookSeeds?.Invoke();
//                     StartProcessV1(collectible);
//                 }
//             }
//         }
//
//         protected virtual void StartProcessV1(CollectibleBehaviour collectible)
//         {
//             IsReady = false;
//             collectible.transform.DOJump(_processPoints[0].position, 2, 1, _jumpDuration)
//                 .OnComplete(() =>
//                 {
//                     collectible.transform
//                         .DOMove(_processPoints[1].position, GetDuration(collectible.transform, _processPoints[1]))
//                         .SetEase(Ease.Linear).OnComplete(() => { PlayProcessV1Machine(collectible); });
//                 });
//         }
//
//         protected virtual void PlayProcessV1Machine(CollectibleBehaviour collectible)
//         {
//             _hammerParticle.Play();
//             _hammer.DOLocalMoveY(_hammerTargetLocalPos.y, .1f).OnComplete(() =>
//             {
//                 _hammer.DOLocalMoveY(_hammerInitialLocalPos.y, .5f);
//                 collectible.gameObject.SetActive(false);
//                 GameObject v1 = ObjectPoolController.Instance.SpawnObject(ObjectName.CacaoCoreV1,
//                     _processPoints[1].position, quaternion.identity, transform);
//                 StartProcessV2(v1);
//             });
//         }
//
//         protected virtual void StartProcessV2(GameObject v1)
//         {
//             v1.transform.DOMove(_processPoints[2].position, GetDuration(v1.transform, _processPoints[2]))
//                 .SetEase(Ease.Linear).OnComplete(() => { PlayProcessV2Machine(v1); });
//         }
//
//         protected virtual void PlayProcessV2Machine(GameObject v1)
//         {
//             v1.SetActive(false);
//             GameObject v2 = ObjectPoolController.Instance.SpawnObject(ObjectName.CacaoCoreV2,
//                 _processPoints[2].position, quaternion.identity, transform);
//             StartProcessV3(v2);
//         }
//
//         protected virtual void StartProcessV3(GameObject v2)
//         {
//             v2.transform.DOMove(_processPoints[3].position, GetDuration(v2.transform, _processPoints[3]))
//                 .SetEase(Ease.Linear).OnComplete(() => { PlayProcessV3Machine(v2); });
//         }
//
//         protected virtual void PlayProcessV3Machine(GameObject v2)
//         {
//             _moldParticle.Play();
//             _mold.DOLocalMoveY(_moldTargetLocalPos.y, .2f).OnComplete(() =>
//             {
//                 v2.SetActive(false);
//                 CollectibleBehaviour v3 = ObjectPoolController.Instance
//                     .SpawnObject(ObjectName.Chocolate, _processPoints[3].position, quaternion.identity, transform)
//                     .GetComponent<CollectibleBehaviour>();
//                 v3.ChangeAroma(AromaType.Normal);
//                 StartProcessEnd(v3);
//                 _mold.DOLocalMoveY(_moldInitialLocalPos.y, .5f);
//             });
//         }
//
//         protected virtual void StartProcessEnd(CollectibleBehaviour v3)
//         {
//             Transform v3Transform = v3.transform;
//             v3Transform.DOMove(_processPoints[4].position, GetDuration(v3Transform, _processPoints[4]))
//                 .SetEase(Ease.Linear)
//                 .OnComplete(() =>
//                 {
//                     v3Transform.DOJump(_outputBulk.GetCurrentPos(), 3, 1, _jumpDuration).SetEase(Ease.Linear);
//                     v3Transform.DORotateQuaternion(_outputBulk.GetCurrentBaseTransform().rotation,
//                         _jumpDuration * 0.5f);
//                     v3Transform.parent = _outputBulk.transform;
//                     _outputBulk.PutCollectible(v3);
//                     _inProcessCount--;
//                     _producedProductCount++;
//                     if (_producedProductCount == _maxProductionCount)
//                     {
//                         SaveFactoryCooldown();
//                         StartCoroutine(FactoryCooldown(_cooldownDuration));
//                     }
//                     else
//                         IsReady = true;
//                 });
//         }
//
//         protected IEnumerator FactoryCooldown(float duration)
//         {
//             float elapsedTime = 0;
//             var second = new WaitForSeconds(1.0f);
//             while (duration >= elapsedTime)
//             {
//                 yield return second;
//                 elapsedTime++;
//                 _cooldownText.text = "Cooldown \n" +
//                                      $"<color=red>{TruckTimerBehaviour.FormatTime(duration - elapsedTime)}</color>";
//             }
//
//             _cooldownText.text = "Chocolate \n Factory";
//             _producedProductCount = 0;
//             IsReady = true;
//         }
//
//         protected void OnEnable()
//         {
//             _inputBulk.CollectibleAdded += CollectibleAdded;
//         }
//
//         protected void CollectibleAdded(CollectibleBehaviour obj)
//         {
//             TookSeeds?.Invoke();
//         }
//
//         private void OnDisable()
//         {
//             _inputBulk.CollectibleAdded -= CollectibleAdded;
//             //_shredderCylinder1.DOKill();
//             //_shredderCylinder2.DOKill();
//             StopAllCoroutines();
//         }
//
//         protected float GetDuration(Transform target1, Transform target2)
//         {
//             float distance = Vector3.Distance(target1.position, target2.position);
//             return distance / _moveSpeed;
//         }
//
//         protected IEnumerator BeltAnimationRoutine()
//         {
//             WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
//             float offset;
//             while (true)
//             {
//                 offset = Time.time * _beltAnimSpeed;
//                 _beltRenderers.ForEach(x => { x.materials[1].SetTextureOffset(_baseMap, _offsetVec * offset); });
//                 yield return waitForEndOfFrame;
//             }
//         }
//
//         protected void LoadFactoryCooldown()
//         {
//             if (PlayerData.Instance.FactoryCooldownDateTime.ContainsKey(
//                     $"{PlayerData.Instance.CurrentLevel}-{_factoryId}"))
//             {
//                 var savedTime = DateTime.Parse(
//                     PlayerData.Instance.FactoryCooldownDateTime[
//                         $"{PlayerData.Instance.CurrentLevel}-{_factoryId}"]);
//
//                 var elapsedTime = DateTime.Now - savedTime;
//
//
//                 if (elapsedTime.TotalSeconds < _cooldownDuration)
//                 {
//                     IsReady = false;
//                     StartCoroutine(FactoryCooldown(_cooldownDuration - (float)elapsedTime.TotalSeconds));
//                 }
//             }
//         }
//
//         protected void SaveFactoryCooldown()
//         {
//             if (!PlayerData.Instance.FactoryCooldownDateTime.ContainsKey($"{PlayerData.Instance.CurrentLevel}-{_factoryId}"))
//             {
//                 PlayerData.Instance.FactoryCooldownDateTime.Add($"{PlayerData.Instance.CurrentLevel}-{_factoryId}",
//                     DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
//             }
//             else
//                 PlayerData.Instance.FactoryCooldownDateTime[$"{PlayerData.Instance.CurrentLevel}-{_factoryId}"] =
//                     DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
//         }
//     }
// }