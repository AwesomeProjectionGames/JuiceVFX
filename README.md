# Juice VFX

A powerful, customizable, and artist-friendly system for adding "juice" (game feel) to your Unity projects.

## Features

- **Event-Driven**: Trigger effects via UnityEvents or C# Events.
- **Modular**: Mix and match effects using `JuiceFeedback` ScriptableObjects.
- **Artist Friendly**: Heavily relies on AnimationCurves and visual cues.
- **Enhanced Editor UX**: Seamlessly manage effects as sub-assets or shared presets directly within the `JuiceFeedback` inspector.
- **Live Debugger**: Dedicated Editor Window to inspect and monitor played effects in real-time with full contextual diagnostics.

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

- **Unity Input System (Optional)**: Used for gamepad vibration haptics. Automatically detected and enabled via `versionDefines` when `com.unity.inputsystem` is installed. If absent, the package compiles cleanly without gamepad support.
- **URP (Optional)**: For Post Processing effects.
- **DOTween (Optional)**: For DOTween-based easing on effects.

## Integrations

Optional third-party integrations are gated behind scripting define symbols. Add the relevant symbol in **Edit → Project Settings → Player → Scripting Define Symbols** to enable the corresponding integration.

| Integration | Scripting Define Symbol | Effects unlocked |
|---|---|---|
| Universal Render Pipeline | `URP` | Bloom, Chromatic Aberration, Color Adjustments, Depth of Field, Lens Distortion, Motion Blur, Panini Projection, Vignette, White Balance, Global PP Volume Auto Blend |
| DOTween | `DOTWEEN` | DOTween-based easing overrides for all existing effects (Shake, Scale, Squash & Stretch, Camera, Light, Material, Audio Mixer, Haptics, Freeze Frame) |

> **Note**: Without the corresponding scripting define symbol, the integration files are fully excluded from compilation, so missing the package will never cause compilation errors.

## Juice Debugger

Open the debugger via **Tools → JuiceVFX → Juice Debugger** or **Window → Analysis → JuiceVFX Debugger**.

- **Playback History & Timeline**: Real-time event log with category color badges, durations, and multiplier info.
- **Context Inspection**: Track who invoked the effect, target player entity/component, connected input devices/gamepads, root transform & contact point, and connected renderers.
- **Live Runner State**: Track active runners, progress percentage, remaining delay, and stop active runners on demand.
- **Editor Actions**: Ping effect ScriptableObjects, ping player GameObjects, replay effects live in Play Mode, or copy markdown debug reports to clipboard.
- **Zero Build Overhead**: Completely stripped at compile-time in standalone builds via `[Conditional("UNITY_EDITOR")]`.