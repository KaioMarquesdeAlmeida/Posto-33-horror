using KinematicCharacterController;
using KinematicCharacterController.Examples;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        public Animator animator;
        public ExampleCharacterController Character;

        private Vector2 moveInput;

        private PlayerControl controls;

        void Awake()
        {
            controls = new PlayerControl();

            controls.Player.Move.performed += ctx =>
            {
                animator.SetBool("walking", true);

                moveInput = ctx.ReadValue<Vector2>();
            };
            controls.Player.Move.canceled += ctx => {
                animator.SetBool("walking", false);

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

            Character.SetInputs(ref characterInputs);
        }
    }
}