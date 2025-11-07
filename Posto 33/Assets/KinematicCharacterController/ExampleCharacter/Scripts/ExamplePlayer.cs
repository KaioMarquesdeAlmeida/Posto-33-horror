using KinematicCharacterController;
using KinematicCharacterController.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        public ExampleCharacterController Character;

        private const string MouseXInput = "Mouse X";
        private const string MouseYInput = "Mouse Y";
        private const string MouseScrollInput = "Mouse ScrollWheel";
        private const string HorizontalInput = "Horizontal";
        private const string VerticalInput = "Vertical";

        private Vector2 moveInput;

        private PlayerControl controls;

        void Awake()
        {
            controls = new PlayerControl();

            controls.Player.Move.performed += ctx =>
            {

                moveInput = ctx.ReadValue<Vector2>();
            };
            controls.Player.Move.canceled += ctx => {

                moveInput = Vector2.zero;
            };

        }

        void OnEnable()
        {
            controls.Enable();
        }

        void OnDisable()
        {
            controls.Disable();
        }

        private void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
    
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            HandleCharacterInput();
        }


     

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new()
            {
                // Build the CharacterInputs struct
                MoveAxisForward = moveInput.x,
                MoveAxisRight = moveInput.y
            };
            //   characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            //   characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
            // characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
            // characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);

            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}