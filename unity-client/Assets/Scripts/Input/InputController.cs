using UnityEngine;
using UnityEngine.InputSystem;
using PvpGame.Utils;
using PvpGame.Game;

namespace PvpGame.Input
{
    public class InputController : MonoBehaviour
    {
        public GameInputData CurrentInput { get; private set; }

        private Vector2 moveInput;
        private bool fireInput;
        private bool abilityInput;
        private bool isTouchActive;
        private Vector2 touchStartPos;

        private void Awake()
        {
            CurrentInput = new GameInputData();
        }

        private void Update()
        {
            ReadKeyboardInput();
            ReadTouchInput();
            UpdateCurrentInput();
        }

        private void ReadKeyboardInput()
        {
            if (Keyboard.current == null) return;

            Vector2 keyboardMove = Vector2.zero;

            if (Keyboard.current.wKey.isPressed) keyboardMove.y += 1;
            if (Keyboard.current.sKey.isPressed) keyboardMove.y -= 1;
            if (Keyboard.current.aKey.isPressed) keyboardMove.x -= 1;
            if (Keyboard.current.dKey.isPressed) keyboardMove.x += 1;

            if (keyboardMove.sqrMagnitude > 0)
            {
                moveInput = keyboardMove.normalized;
            }
            else if (!isTouchActive)
            {
                moveInput = Vector2.zero;
            }

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                fireInput = true;
            }

            if (Keyboard.current.eKey.wasPressedThisFrame)
            {
                abilityInput = true;
            }
        }

        private void ReadTouchInput()
        {
            if (Touchscreen.current == null) return;

            var touch = Touchscreen.current.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                touchStartPos = touch.position.ReadValue();
                isTouchActive = true;
            }
            else if (touch.press.isPressed && isTouchActive)
            {
                Vector2 currentPos = touch.position.ReadValue();
                Vector2 delta = currentPos - touchStartPos;

                if (delta.magnitude > 50f)
                {
                    moveInput = delta.normalized;
                }

                if (delta.magnitude < 20f)
                {
                    fireInput = true;
                }
            }
            else if (touch.press.wasReleasedThisFrame)
            {
                Vector2 currentPos = touch.position.ReadValue();
                Vector2 delta = currentPos - touchStartPos;

                if (delta.magnitude > 100f)
                {
                    abilityInput = true;
                }

                isTouchActive = false;
                moveInput = Vector2.zero;
            }
        }

        private void UpdateCurrentInput()
        {
            CurrentInput.moveX = moveInput.x;
            CurrentInput.moveY = moveInput.y;
            CurrentInput.fire = fireInput;
            CurrentInput.ability = abilityInput;
            CurrentInput.timestamp = GetCurrentTimestamp();

            fireInput = false;
            abilityInput = false;
        }

        public GameInputData GetInputData()
        {
            return CurrentInput;
        }

        public void ResetInput()
        {
            moveInput = Vector2.zero;
            fireInput = false;
            abilityInput = false;
            CurrentInput = new GameInputData();
        }

        private long GetCurrentTimestamp()
        {
            return (long)(Time.realtimeSinceStartup * 1000);
        }
    }
}
