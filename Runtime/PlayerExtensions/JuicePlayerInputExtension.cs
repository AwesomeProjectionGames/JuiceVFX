#nullable enable

using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace JuiceVFX
{
    public class JuicePlayerInputExtension : JuicePlayerExtension
    {
        public PlayerInput? playerInput;

        private void OnEnable()
        {
            UpdateTargetGamepads();
            // PlayerInput doesn't have a direct C# event for device changes (it uses SendMessage or UnityEvent by default)
            // But we can hook into the global InputSystem device change to update if needed.
            InputSystem.onDeviceChange += OnDeviceChange;
            if (playerInput == null)
            {
                Debug.LogWarning("JuicePlayerInputExtension: No PlayerInput assigned. Trying to find one on the same GameObject.");
                playerInput = GetComponent<PlayerInput>();
            }
            if (playerInput == null)
            {
                Debug.LogWarning("JuicePlayerInputExtension: No PlayerInput found on the GameObject. Gamepad.current may be used as fallback.");
            }
        }

        private void OnDisable()
        {
            InputSystem.onDeviceChange -= OnDeviceChange;
        }

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            // Re-evaluate gamepads if there's any device change (e.g. disconnected or reconnected)
            UpdateTargetGamepads();
        }

        private void UpdateTargetGamepads()
        {
            if (playerInput == null || _juicePlayer == null) return;

            // Find all Gamepads currently associated with this PlayerInput
            _juicePlayer.TargetGamepads = playerInput.devices
                .OfType<Gamepad>()
                .ToArray();
        }
    }
}
