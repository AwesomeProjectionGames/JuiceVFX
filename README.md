# Juice VFX

A powerful, customizable, and artist-friendly system for adding "juice" (game feel) to your Unity projects.

## Features

- **Event-Driven**: Trigger effects via UnityEvents or C# Events.
- **Modular**: Mix and match effects using `JuiceFeedback` ScriptableObjects.
- **Artist Friendly**: Heavily relies on AnimationCurves and visual cues.

## Effects Included

- **Squash & Stretch**: Procedural deformation using curves.
- **Flash**: Material flashing on damage/events.
- **Object Shake**: Versatile shake implementation for any object (Camera, UI, Meshes).
- **Freeze Frame**: Impact frames and hit stop.
- **Time Scaling**: Slow motion effects.
- **Post Processing**: Generic volume adjustments.
- **Audio**: SFX, Ducking, Low-Pass filter.
- **Haptics**: Gamepad vibration.
- **Instantiate**: Spawn particles or debris.

## Dependencies

- **Unity Input System**: Used for gamepad vibration.
- **URP (Optional)**: For Post Processing effects.
- **DOTween (Optional)**: For DOTween-based easing on effects.

## Integrations

Optional third-party integrations are gated behind scripting define symbols. Add the relevant symbol in **Edit → Project Settings → Player → Scripting Define Symbols** to enable the corresponding integration.

| Integration | Scripting Define Symbol | Effects unlocked |
|---|---|---|
| Universal Render Pipeline | `URP` | Bloom, Chromatic Aberration, Color Adjustments, Depth of Field, Lens Distortion, Motion Blur, Panini Projection, Vignette, White Balance, Global PP Volume Auto Blend |
| DOTween | `DOTWEEN` | DOTween-based easing overrides for all existing effects (Shake, Scale, Squash & Stretch, Camera, Light, Material, Audio Mixer, Haptics, Freeze Frame) |

> **Note**: Without the corresponding scripting define symbol, the integration files are fully excluded from compilation, so missing the package will never cause compilation errors.
