using System;
using UnityEngine;

/**
 * Responsible only for detecting user input.
 * It is completely decoupled from the network or game logic.
 */
public class InputHandler : MonoBehaviour
{
    // Event triggered when the user presses Y/y
    public event Action OnUserAction;

    public event Action<string> OnInvalidInput;

    // We keep this private so other classes can't randomly change the input state.
    // They must use the exposed public method to enable/disable it.
    private bool isInputActive = false;

    private void Update()
    {
        // If input is not active, we do nothing
        if (!isInputActive) return;

        // Detect any key press or mouse click
        if (Input.anyKeyDown)
        {
            if (Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.y))
            {
                OnUserAction?.Invoke();
                // Disable input after the first successful action to prevent spamming
                SetInputActive(false); 
            }
            else
            {
                OnInvalidInput?.Invoke("Invalid input! Please press Y to interact.");
            } 

            OnUserAction?.Invoke(); 
                       
        }
    }

    /**
     * Public method to expose control over the input state, 
     * without exposing the variable itself.
     */
    public void SetInputActive(bool active)
    {
        isInputActive = active;
        if (active)
        {
            Debug.Log("[INPUT] Listening for 'Y' key press...");
        }
    }
}