using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ShipBattle.Network;
using ShipBattle.Core;

namespace ShipBattle.Gameplay
{
    public class InputSender : MonoBehaviour
    {
        public static InputSender Instance { get; private set; }

        [Header("Input Configuration")]
        [SerializeField] private float sendRate = 20f; // 20Hz
        [SerializeField] private Joystick virtualJoystick; // For Android

        private Vector2 currentDirection;
        private bool isFiring;
        private bool abilityPressed;

        private float sendTimer;
        private float sendInterval;

        private InputAction moveAction;
        private InputAction fireAction;
        private InputAction abilityAction;

        private SocketClient socketClient;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            sendInterval = 1f / sendRate;
        }

        private void Start()
        {
            socketClient = GameManager.Instance.SocketClient;
            
            // Input disabled by default until GameController enables it
            this.enabled = false;
        }

        public void EnableInput()
        {
            this.enabled = true;
            SetupInputActions();
            Debug.Log("[InputSender] Input enabled");
        }

        public void DisableInput()
        {
            this.enabled = false;
            moveAction?.Disable();
            fireAction?.Disable();
            abilityAction?.Disable();
            Debug.Log("[InputSender] Input disabled");
        }

        private void SetupInputActions()
        {
            if (moveAction != null) return; // Already setup

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
            #if UNITY_ANDROID && !UNITY_EDITOR
            ReadTouchInput();
            #else
            ReadKeyboardInput();
            #endif
        }

        private void ReadKeyboardInput()
        {
            if (moveAction == null) return;

            currentDirection = moveAction.ReadValue<Vector2>();
            if (currentDirection.magnitude > 1f)
            {
                currentDirection.Normalize();
            }

            isFiring = fireAction.IsPressed();
            abilityPressed = abilityAction.WasPressedThisFrame();
        }

        private void ReadTouchInput()
        {
            if (virtualJoystick != null)
            {
                currentDirection = virtualJoystick.Direction;
            }
            else
            {
                currentDirection = Vector2.zero;
            }
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

            socketClient.SendEvent("game:input", inputData);

            // Reset ability after sending (trigger only)
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
            moveAction?.Disable();
            fireAction?.Disable();
            abilityAction?.Disable();
        }
    }

    public class Joystick : MonoBehaviour
    {
        public Vector2 Direction { get; private set; }
        private void Update() { Direction = Vector2.zero; }
    }

    [Serializable]
    public class InputData
    {
        public Vector2Data direction;
        public bool isFiring;
        public bool ability;
        public long timestamp;
    }

    [Serializable]
    public class Vector2Data
    {
        public float x;
        public float y;
    }
}
