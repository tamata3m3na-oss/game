using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ShipBattle.Network;
using ShipBattle.Core;

namespace ShipBattle.Gameplay
{
    /// <summary>
    /// Captures player input and sends it to the server at 20Hz.
    /// Supports both keyboard (Windows) and touch (Android) input.
    /// Phase 2 - New implementation.
    /// </summary>
    public class InputSender : MonoBehaviour
    {
        [Header("Input Configuration")]
        [SerializeField] private float sendRate = 20f; // 20Hz
        [SerializeField] private Joystick virtualJoystick; // For Android

        private Vector2 currentDirection;
        private bool isFiring;
        private bool abilityPressed;

        private float sendTimer;
        private float sendInterval;

        // Input System actions (for keyboard)
        private InputAction moveAction;
        private InputAction fireAction;
        private InputAction abilityAction;

        private SocketClient socketClient;

        private void Awake()
        {
            sendInterval = 1f / sendRate;

            // Setup Input System for keyboard/gamepad
            SetupInputActions();
        }

        private void Start()
        {
            socketClient = GameManager.Instance.SocketClient;

            if (socketClient == null)
            {
                Debug.LogError("[InputSender] SocketClient not found!");
            }
        }

        private void SetupInputActions()
        {
            // Movement (WASD + Arrows)
            moveAction = new InputAction("Move", InputActionType.Value);
            moveAction.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Up", "<Keyboard>/upArrow")
                .With("Down", "<Keyboard>/s")
                .With("Down", "<Keyboard>/downArrow")
                .With("Left", "<Keyboard>/a")
                .With("Left", "<Keyboard>/leftArrow")
                .With("Right", "<Keyboard>/d")
                .With("Right", "<Keyboard>/rightArrow");
            moveAction.Enable();

            // Fire (Space + Left Click)
            fireAction = new InputAction("Fire", InputActionType.Button);
            fireAction.AddBinding("<Keyboard>/space");
            fireAction.AddBinding("<Mouse>/leftButton");
            fireAction.Enable();

            // Ability (Q key)
            abilityAction = new InputAction("Ability", InputActionType.Button);
            abilityAction.AddBinding("<Keyboard>/q");
            abilityAction.Enable();
        }

        private void Update()
        {
            // Read input
            ReadInput();

            // Send input at fixed rate
            sendTimer += Time.deltaTime;
            if (sendTimer >= sendInterval)
            {
                sendTimer = 0f;
                SendInput();
            }
        }

        private void ReadInput()
        {
            // Check platform and read appropriate input
            #if UNITY_ANDROID && !UNITY_EDITOR
            ReadTouchInput();
            #else
            ReadKeyboardInput();
            #endif
        }

        private void ReadKeyboardInput()
        {
            // Read movement from Input System
            currentDirection = moveAction.ReadValue<Vector2>();

            // Normalize diagonal movement
            if (currentDirection.magnitude > 1f)
            {
                currentDirection.Normalize();
            }

            // Read fire button
            isFiring = fireAction.IsPressed();

            // Read ability button
            abilityPressed = abilityAction.WasPressedThisFrame();
        }

        private void ReadTouchInput()
        {
            // Read from virtual joystick (if present)
            if (virtualJoystick != null)
            {
                currentDirection = virtualJoystick.Direction;
            }
            else
            {
                currentDirection = Vector2.zero;
            }

            // Touch fire button is handled by UI button callbacks
            // (See MatchHUD.cs for fire button implementation)
        }

        private void SendInput()
        {
            if (socketClient == null || !socketClient.IsConnected)
            {
                return;
            }

            var inputData = new InputData
            {
                direction = new Vector2Data
                {
                    x = currentDirection.x,
                    y = currentDirection.y
                },
                isFiring = isFiring,
                ability = abilityPressed,
                timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            // Send asynchronously (fire and forget at 20Hz is acceptable)
            _ = socketClient.SendInputAsync(inputData);

            // Reset ability after sending
            abilityPressed = false;
        }

        // Public API for UI buttons (Android)
        public void OnFireButtonPressed()
        {
            isFiring = true;
        }

        public void OnFireButtonReleased()
        {
            isFiring = false;
        }

        public void OnAbilityButtonPressed()
        {
            abilityPressed = true;
        }

        private void OnDestroy()
        {
            // Clean up Input System actions
            moveAction?.Disable();
            fireAction?.Disable();
            abilityAction?.Disable();
        }
    }

    /// <summary>
    /// Simple virtual joystick for touch input.
    /// This is a placeholder - implement full joystick UI as needed.
    /// </summary>
    public class Joystick : MonoBehaviour
    {
        public Vector2 Direction { get; private set; }

        // Implement touch joystick logic here
        // For Phase 2, this is a basic placeholder
        private void Update()
        {
            // Placeholder - real implementation would track touch input
            Direction = Vector2.zero;
        }
    }
}
