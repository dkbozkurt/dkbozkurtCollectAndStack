using System;
using UnityEngine;

namespace StackAndCollect.MyStackAndCollect.Scripts
{
    public class PlayerStackBehaviour : MonoBehaviour
    {

        public bool CanIncrease;
        public bool CanDecrease;
        public float AnimationDuration;
        public bool IsAnimPlaying;
        
        public void IncreaseWithAnimation(GameObject collectible,Action action = null)
        {
            
        }

        public GameObject DecreaseStackWithAnimation()
        {
            return null;
        }
    }
}
