using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }
    
    [Header("Movement Settings")]
    public float movementSensitivity = 1.0f;
    public float touchDeadZone = 50f;
    
    [Header("Input Mode")]
    public bool useTouchInput = true;
    
    // Input state
    private Vector2 moveInput = Vector2.zero;
    private bool firePressed = false;
    private bool abilityPressed = false;
    private bool fireHeld = false;
    private bool abilityHeld = false;
    
    // Touch input
    private Vector2 touchStartPosition;
    private bool isTouching = false;
    
    // Keyboard input
    private Vector2 keyboardInput = Vector2.zero;
    
    // Events
    public event Action<GameInputData> OnInputEvent;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (useTouchInput)
        {
            HandleTouchInput();
        }
        else
        {
            HandleKeyboardInput();
        }
        
        // Send input at 60 FPS
        SendInput();
    }
    
    private void HandleTouchInput()
    {
        // Reset input state
        moveInput = Vector2.zero;
        firePressed = false;
        abilityPressed = false;
        
        // Check for touch input
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
        {
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            
            if (!isTouching)
            {
                // Touch started
                touchStartPosition = touchPosition;
                isTouching = true;
            }
            else
            {
                // Touch moved
                Vector2 touchDelta = touchPosition - touchStartPosition;
                
                // Check if it's a significant movement (not just a tap)
                if (touchDelta.magnitude > touchDeadZone)
                {
                    moveInput = touchDelta.normalized * movementSensitivity;
                }
                else
                {
                    // Tap - treat as fire
                    firePressed = true;
                }
            }
        }
        else
        {
            // Touch ended
            if (isTouching)
            {
                Vector2 touchEndPosition = Touchscreen.current.primaryTouch.position.ReadValue();
                Vector2 touchDelta = touchEndPosition - touchStartPosition;
                
                // Check for swipe (ability)
                if (touchDelta.magnitude > touchDeadZone * 2)
                {
                    abilityPressed = true;
                }
                
                isTouching = false;
            }
        }
    }
    
    private void HandleKeyboardInput()
    {
        // Reset input state
        moveInput = Vector2.zero;
        firePressed = false;
        abilityPressed = false;
        
        // Keyboard movement
        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
            
            moveInput = moveInput.normalized * movementSensitivity;
            
            // Fire and ability
            firePressed = Keyboard.current.spaceKey.wasPressedThisFrame;
            abilityPressed = Keyboard.current.eKey.wasPressedThisFrame;
        }
    }
    
    private void SendInput()
    {
        if (OnInputEvent == null) return;
        
        int playerId = AuthManager.Instance != null ? AuthManager.Instance.GetUserId() : -1;
        if (playerId <= 0) return;

        var inputData = new GameInputData
        {
            playerId = playerId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            moveX = moveInput.x,
            moveY = moveInput.y,
            fire = firePressed,
            ability = abilityPressed
        };
        
        OnInputEvent.Invoke(inputData);
    }
    
    public Vector2 GetMoveInput()
    {
        return moveInput;
    }
    
    public bool IsFirePressed()
    {
        return firePressed;
    }
    
    public bool IsAbilityPressed()
    {
        return abilityPressed;
    }
}