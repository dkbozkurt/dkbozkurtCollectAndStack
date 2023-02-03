using System;
using UnityEngine;

namespace Game.Scripts.Behaviours
{
    public class PlayerStackBehaviour : MonoBehaviour
    {

        public bool CanIncrease;
        public bool CanDecrease;
        public float AnimationDuration;
        public bool IsAnimPlaying;
        
        public void IncreaseWithAnimation(CollectibleBehaviour collectible,Action action = null)
        {
            
        }

        public GameObject DecreaseStackWithAnimation()
        {
            return null;
        }
    }
}
