// Dogukan Kaan Bozkurt
//      github.com/dkbozkurt

using System;
using Game.Scripts.Controllers;
using StackAndCollect.MyStackAndCollect.Scripts;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    /// <summary>
    /// 
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class StorageTextBehaviour : MonoBehaviour
    {
        [SerializeField] private bool _flag;
        [SerializeField] private BulkBehaviour _bulkBehaviour;

        private void OnTriggerEnter(Collider other)
        {
            if (other.TryGetComponent(out PlayerStackController playerStackController))
            {
                _bulkBehaviour.SetCollectibleBulk(_flag);
            }
        }
    }
}
