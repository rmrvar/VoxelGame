using UnityEngine;
using UnityEngine.Serialization;

namespace VoxelGame.Player
{
    [RequireComponent(typeof(CharacterController))]

    public class MoveAndLook : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("moveSpeed")]
        private float _moveSpeed;
        [SerializeField, FormerlySerializedAs("turnSpeed")]
        private float _turnSpeed;
        [SerializeField, FormerlySerializedAs("lookRoot")]
        private Transform _lookRoot;
        [SerializeField, FormerlySerializedAs("controller")]
        private CharacterController _controller;
        [SerializeField] 
        private Transform crosshairUI = null;

        private void Awake()
        {
            if (_controller == null)
            {
                _controller = GetComponent<CharacterController>();
            }

            Cursor.lockState = CursorLockMode.Locked;
            if (crosshairUI != null)
            {
                crosshairUI.gameObject.SetActive(true);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Cursor.lockState = CursorLockMode.None;
                if (crosshairUI != null)
                {
                    crosshairUI.gameObject.SetActive(false);
                }
            }
            if (Input.GetKeyDown(KeyCode.Mouse0) || Input.GetKeyDown(KeyCode.Mouse1))
            {
                Cursor.lockState = CursorLockMode.Locked;
                if (crosshairUI != null)
                {
                    crosshairUI.gameObject.SetActive(true);
                }
            }

            var moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            var turnInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

            var moveDir = _lookRoot.forward * moveInput.y + _lookRoot.right * moveInput.x;

            if (Input.GetKey(KeyCode.Space))
            {
                // Make the Player move mostly upwards (the move amount will be normalized of course).
                moveDir.y += 1;
            }
            if (moveDir.sqrMagnitude > 1)
            {
                // Make sure that we can't move faster than _moveSpeed;
                moveDir.Normalize();
            }

            var transSpeed = Input.GetKey(KeyCode.LeftShift) ? _moveSpeed * 2.5F : _moveSpeed;

            _controller.Move(moveDir * transSpeed * Time.deltaTime);

            var prevRot = _lookRoot.rotation.eulerAngles;

            _lookRoot.localRotation = Quaternion.Euler(prevRot.x - turnInput.y * _turnSpeed * Time.deltaTime, 0, 0);
            transform.Rotate(0, turnInput.x * _turnSpeed * Time.deltaTime, 0);
        }
    }
}
