using Game.Scripts.Managers;
using UnityEngine;

namespace Game.Scripts.Controllers
{
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMovementController : MonoBehaviour
    {
        [Header("Movement Properties")]
        [SerializeField] private float _speed = 6f;
        [SerializeField] private float _turnSmoothTime = 0.05f;

        private CharacterController _characterController;
        // private Animator _animator;

        private float _inputX;
        private float _inputZ;
        private float _turnSmoothVelocity;

        public bool IsPlayerMoving { get; set; }

        private void Awake()
        {
            _characterController = GetComponent<CharacterController>();
            // _animator = GetComponentInChildren<Animator>();
        }

        private void Update()
        {
            // PullPlayerDown(transform.position.y);
            Move();
        }

        private void Move()
        {
            _inputX = JoystickManager.Instance.InputHorizontal();
            _inputZ = JoystickManager.Instance.InputVertical();

            Vector3 direction = new Vector3(_inputX, 0f, _inputZ).normalized;

            if (direction.magnitude >= 0.1f)
            {
                Rotate(direction);
                direction.y =0;
                _characterController.Move(direction * (_speed * Time.deltaTime));
                IsPlayerMoving = true;
                // AnimationBoolSetter("Move",true);

                return;
            }

            // AnimationBoolSetter("Move",false);
            
            IsPlayerMoving = false;
        }

        private void Rotate(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

            // Adding smooth turning
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity,
                _turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        // public void AnimationBoolSetter(string animName, bool status)
        // {
        //     _animator.SetBool(animName,status);
        // }

        private void PullPlayerDown(float heightFromGround)
        {
            if (heightFromGround <= 0) return;

            transform.position = new Vector3(transform.position.x,
                Mathf.Lerp(transform.position.y, 0, Time.deltaTime *50), transform.position.z);
        }
    }
}