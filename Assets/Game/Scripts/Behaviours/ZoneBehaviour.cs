using System;
using System.Collections;
using DG.Tweening;
using Game.Scripts.Controllers;
using Game.Scripts.Enums;
using Game.Scripts.Managers;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    public class ZoneBehaviour : MonoBehaviour
    {
        [Header("Preferences")] 
        [SerializeField] protected ObjectName _objectNameToSpawn;

        [Header("Adjustable")] 
        [SerializeField] protected bool _isSpawnZone = true;
        [SerializeField] protected bool _IsLimitedAmount = false;
        [SerializeField] protected int _zoneSpawnLimit = 3;

        public bool IsReady = true;

        public bool CanSpawn => _spawnedObjectCount < _zoneSpawnLimit;
        private int _spawnedObjectCount = 0;
        
        protected void OnTriggerStay(Collider other)
        {
            if(!IsReady) return;
            
            if(!other.TryGetComponent(out PlayerStackController playerStackController)) return;
            
            if (_IsLimitedAmount && !CanSpawn)
            {
                return;
            }
            
            if (_isSpawnZone)
            {
                StartCoroutine(ActionWithDelay());

                IEnumerator ActionWithDelay()
                {
                    while (playerStackController.CanIncrease && !playerStackController.IsAnimPlaying)
                    {
                        IsReady = false;
                        playerStackController.IsAnimPlaying = true;
                        
                        Spawn(playerStackController,_objectNameToSpawn);

                        yield return new WaitForSeconds(playerStackController.AnimationDuration);
                    }
                    
                    if (!IsReady)
                    {
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
                    while (playerStackController.CanDecrease && !playerStackController.IsAnimPlaying)
                    {
                        IsReady = false;
                        playerStackController.IsAnimPlaying = true;
                        
                        Remove(playerStackController);

                        yield return new WaitForSeconds(playerStackController.AnimationDuration);
                    }
                    
                    if (!IsReady)
                    {
                        IsReady = true;
                        playerStackController.IsAnimPlaying = false;
                    }
                }
            }
        }

        public void Spawn(PlayerStackController playerStackController,ObjectName objectName)
        {
            CollectibleBehaviour collectible = ObjectPoolManager.Instance.GetPooledObject(objectName,Vector3.zero,
                    Quaternion.identity,transform,true)
                .GetComponent<CollectibleBehaviour>();
            
            _spawnedObjectCount++;
            
            playerStackController.IncreaseWithAnimation(collectible);
        }

        public void Remove(PlayerStackController playerStackController)
        {
            var collectibleBehaviour = playerStackController.DecreaseStackWithAnimation(transform.position,transform);

            DOVirtual.DelayedCall(playerStackController.AnimationDuration,
                () => collectibleBehaviour.gameObject.SetActive(false));
        }
    }
}
