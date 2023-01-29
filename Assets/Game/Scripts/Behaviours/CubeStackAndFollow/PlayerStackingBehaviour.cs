using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.Behaviours.CubeStackAndFollow
{
    public class PlayerStackingBehaviour : MonoBehaviour
    {
        [SerializeField] private Transform _stackPosition;
        private Vector3 _currentCubePosition;

        private List<GameObject> _stackedCubeList = new List<GameObject>();
        private int _cubeListIndexCounter = 0;

        private void OnTriggerEnter(Collider other)
        {
            CollectCube(other);
        }

        private void CollectCube(Collider other)
        {
            if (other.CompareTag("Cube"))
            {
                _stackedCubeList.Add(other.gameObject);
                if (_stackedCubeList.Count==1)
                {
                    _currentCubePosition = new Vector3(other.transform.position.x, _stackPosition.position.y, other.transform.position.z);
                    other.gameObject.transform.position = _currentCubePosition;
                    _currentCubePosition = new Vector3(other.transform.position.x, transform.position.y + 0.3f, other.transform.position.z);
                    other.gameObject.GetComponent<CollectibleCubeBehaviour>().UpdateCubePosition(transform, true);
                }
                else if (_stackedCubeList.Count > 1)
                {
                    other.gameObject.transform.position = _currentCubePosition;
                    _currentCubePosition = new Vector3(other.transform.position.x, other.gameObject.transform.position.y + 0.3f, other.transform.position.z);
                    other.gameObject.GetComponent<CollectibleCubeBehaviour>().UpdateCubePosition(_stackedCubeList[_cubeListIndexCounter].transform, true);
                    _cubeListIndexCounter++;
                }
            }
        }
    }
}
