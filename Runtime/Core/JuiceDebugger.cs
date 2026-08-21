#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace JuiceVFX
{
#if UNITY_EDITOR
    /// <summary>
    /// Snapshot information of a Gamepad connected to a Juice feedback context.
    /// </summary>
    [Serializable]
    public class JuiceGamepadDebugInfo
    {
        public string Name = string.Empty;
        public string DisplayName = string.Empty;
        public int DeviceId;
        public string Layout = string.Empty;
        public bool IsCurrent;
        public bool IsAdded;

#if ENABLE_INPUT_SYSTEM
        public static JuiceGamepadDebugInfo FromGamepad(Gamepad gamepad)
        {
            if (gamepad == null) return new JuiceGamepadDebugInfo { Name = "Null Gamepad" };

            return new JuiceGamepadDebugInfo
            {
                Name = gamepad.name,
                DisplayName = string.IsNullOrEmpty(gamepad.displayName) ? gamepad.name : gamepad.displayName,
                DeviceId = gamepad.deviceId,
                Layout = gamepad.layout,
                IsCurrent = Gamepad.current == gamepad,
                IsAdded = gamepad.added
            };
        }
#endif
    }

    /// <summary>
    /// Snapshot information of a Renderer connected to a Juice feedback context.
    /// </summary>
    [Serializable]
    public class JuiceRendererDebugInfo
    {
        public Renderer? Renderer;
        public string Name = string.Empty;
        public string TypeName = string.Empty;
        public bool Enabled;
        public bool IsVisible;
        public string[] MaterialNames = Array.Empty<string>();

        public static JuiceRendererDebugInfo FromRenderer(Renderer? renderer)
        {
            if (renderer == null) return new JuiceRendererDebugInfo { Name = "Null Renderer" };

            var mats = Array.Empty<string>();
            try
            {
                if (renderer.sharedMaterials != null)
                {
                    mats = renderer.sharedMaterials
                        .Where(m => m != null)
                        .Select(m => m.name)
                        .ToArray();
                }
            }
            catch
            {
                // ignored
            }

            return new JuiceRendererDebugInfo
            {
                Renderer = renderer,
                Name = renderer.gameObject.name,
                TypeName = renderer.GetType().Name,
                Enabled = renderer.enabled,
                IsVisible = renderer.isVisible,
                MaterialNames = mats
            };
        }
    }

    /// <summary>
    /// Represents a recorded Juice effect playback event for debugging and analytics.
    /// </summary>
    [Serializable]
    public class JuiceDebugEntry
    {
        public int Id;
        public float TimeStamp;
        public int FrameCount;
        public DateTime RealTime;

        // Effect info
        public string EffectName = string.Empty;
        public Type? EffectType;
        public JuiceEffectData? EffectData;
        public JuiceEffectTarget Target;
        public string Category = "General";

        // Player / Target info
        public AbstractJuicePlayer? Player;
        public string PlayerName = string.Empty;
        public string PlayerTypeName = string.Empty;
        public string HierarchyPath = string.Empty;

        // Spatial / Transform
        public Transform? RootTransform;
        public string RootTransformPath = string.Empty;
        public Vector3? RootPosition;
        public Quaternion? RootRotation;
        public Vector3? ContactPoint;
        public Quaternion? ContactRotation;

        // Parameters
        public float Multiplier = 1f;
        public float Duration = 1f;
        public float Delay = 0f;
        public bool HasDurationOverride;

        // Renderers & Devices
        public List<JuiceRendererDebugInfo> Renderers = new List<JuiceRendererDebugInfo>();
        public List<JuiceGamepadDebugInfo> Gamepads = new List<JuiceGamepadDebugInfo>();

        // Invoker / Caller
        public string InvokerClass = string.Empty;
        public string InvokerMethod = string.Empty;
        public string InvokerFullInfo = string.Empty;

        // Live runner tracking
        private WeakReference<JuiceEffectRunner>? _runnerRef;

        public void SetRunner(JuiceEffectRunner? runner)
        {
            _runnerRef = runner != null ? new WeakReference<JuiceEffectRunner>(runner) : null;
        }

        public JuiceEffectRunner? GetRunner()
        {
            if (_runnerRef != null && _runnerRef.TryGetTarget(out var runner))
            {
                return runner;
            }
            return null;
        }

        public bool IsRunnerActive
        {
            get
            {
                var runner = GetRunner();
                return runner != null && runner.IsPlaying && !runner.IsFinished;
            }
        }

        public float Progress
        {
            get
            {
                var runner = GetRunner();
                if (runner != null)
                {
                    return runner.Duration > 0f ? Mathf.Clamp01(runner.ElapsedTime / runner.Duration) : 1f;
                }
                return 1f;
            }
        }
    }
#endif

    /// <summary>
    /// Centralized debug monitor and history buffer for JuiceVFX.
    /// Tracks all played effects, invokers, players, gamepads, renderers and contexts in Editor.
    /// Completely stripped at compile-time in player builds.
    /// </summary>
    public static class JuiceDebugger
    {
#if UNITY_EDITOR
        private static readonly List<JuiceDebugEntry> _history = new List<JuiceDebugEntry>();
        private static int _nextId = 1;

        public static bool IsRecording { get; set; } = true;
        public static int MaxEntries { get; set; } = 200;

        public static IReadOnlyList<JuiceDebugEntry> History => _history;

        public static event Action<JuiceDebugEntry>? OnEntryAdded;
        public static event Action? OnHistoryCleared;
#endif

        /// <summary>
        /// Records an effect invocation into the debug history.
        /// Decorating with [Conditional("UNITY_EDITOR")] strips all call sites in standalone builds.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        public static void RecordEffect(
            AbstractJuicePlayer player,
            JuiceEffectData effectData,
            JuiceFeedbackContext context,
            float? durationOverride,
            JuiceEffectRunner? runner)
        {
#if UNITY_EDITOR
            if (!IsRecording || effectData == null) return;

            var entry = new JuiceDebugEntry
            {
                Id = _nextId++,
                TimeStamp = Time.time,
                FrameCount = Time.frameCount,
                RealTime = DateTime.Now,

                EffectName = effectData.name,
                EffectType = effectData.GetType(),
                EffectData = effectData,
                Target = effectData.Target,
                Category = DetermineCategory(effectData),

                Player = player,
                PlayerName = player != null ? player.gameObject.name : "Unknown",
                PlayerTypeName = player != null ? player.GetType().Name : "Unknown",
                HierarchyPath = player != null ? GetHierarchyPath(player.transform) : string.Empty,

                RootTransform = context.RootTransform,
                RootTransformPath = context.RootTransform != null ? GetHierarchyPath(context.RootTransform) : string.Empty,
                RootPosition = context.RootTransform != null ? context.RootTransform.position : (Vector3?)null,
                RootRotation = context.RootTransform != null ? context.RootTransform.rotation : (Quaternion?)null,
                ContactPoint = context.ContactPoint,
                ContactRotation = context.Rotation,

                Multiplier = context.Multiplier,
                Duration = durationOverride.HasValue && effectData.AllowDurationOverride ? durationOverride.Value : effectData.Duration,
                Delay = effectData.Delay,
                HasDurationOverride = durationOverride.HasValue && effectData.AllowDurationOverride
            };

            // Connected Renderers
            if (context.Renderers != null)
            {
                foreach (var renderer in context.Renderers)
                {
                    if (renderer != null)
                    {
                        entry.Renderers.Add(JuiceRendererDebugInfo.FromRenderer(renderer));
                    }
                }
            }

#if ENABLE_INPUT_SYSTEM
            // Connected Gamepads
            if (context.Gamepads != null)
            {
                foreach (var gp in context.Gamepads)
                {
                    if (gp != null)
                    {
                        entry.Gamepads.Add(JuiceGamepadDebugInfo.FromGamepad(gp));
                    }
                }
            }
#endif

            // Caller / Invoker Info
            ExtractInvokerInfo(out entry.InvokerClass, out entry.InvokerMethod, out entry.InvokerFullInfo);

            // Runner tracking
            entry.SetRunner(runner);

            // Buffer management
            _history.Add(entry);
            if (_history.Count > MaxEntries)
            {
                _history.RemoveAt(0);
            }

            OnEntryAdded?.Invoke(entry);
#endif
        }

#if UNITY_EDITOR
        public static void ClearHistory()
        {
            _history.Clear();
            OnHistoryCleared?.Invoke();
        }

        public static string DetermineCategory(JuiceEffectData effectData)
        {
            if (effectData == null) return "General";
            var typeName = effectData.GetType().Name;
            var typeNamespace = effectData.GetType().Namespace ?? string.Empty;

            if (typeName.Contains("Camera") || typeNamespace.Contains("Camera")) return "Camera";
            if (typeName.Contains("Vibration") || typeName.Contains("Haptic") || typeNamespace.Contains("Haptics")) return "Haptics";
            if (typeName.Contains("Audio") || typeName.Contains("Sfx") || typeNamespace.Contains("Audio") || typeNamespace.Contains("Sound")) return "Audio";
            if (typeName.Contains("Scale") || typeName.Contains("Shake") || typeName.Contains("Squash") || typeNamespace.Contains("Transform")) return "Transform";
            if (typeName.Contains("Flash") || typeName.Contains("Material") || typeNamespace.Contains("Material")) return "Material";
            if (typeName.Contains("Light") || typeNamespace.Contains("Light")) return "Light";
            if (typeName.Contains("Freeze") || typeName.Contains("Time") || typeNamespace.Contains("Time")) return "Time";
            if (typeName.Contains("Instantiate") || typeName.Contains("Blink") || typeNamespace.Contains("GameObject")) return "GameObject";
            if (typeName.Contains("Volume") || typeName.Contains("PP") || typeNamespace.Contains("URP")) return "PostProcess";

            return "General";
        }

        private static string GetHierarchyPath(Transform? transform)
        {
            if (transform == null) return string.Empty;
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static void ExtractInvokerInfo(out string className, out string methodName, out string fullInfo)
        {
            className = "Direct";
            methodName = "Play";
            fullInfo = "Direct Play()";

            try
            {
                var stackTrace = new StackTrace(2, false);
                for (int i = 0; i < stackTrace.FrameCount; i++)
                {
                    var frame = stackTrace.GetFrame(i);
                    var method = frame?.GetMethod();
                    if (method == null) continue;

                    var declaringType = method.DeclaringType;
                    if (declaringType == null) continue;

                    // Skip Juice internal classes
                    string ns = declaringType.Namespace ?? string.Empty;
                    string name = declaringType.Name;

                    if (ns == "JuiceVFX" || ns == "JuiceVFX.Editor")
                    {
                        if (!name.Contains("Trigger") && !name.Contains("Sample"))
                        {
                            continue;
                        }
                    }

                    if (name.Contains("AbstractJuicePlayer") ||
                        name.Contains("JuicePlayer") ||
                        name.Contains("JuiceDebugger") ||
                        name.Contains("JuiceEntityComponent") ||
                        name.Contains("JuiceControllerComponent") ||
                        name.Contains("JuiceSpectateControllerComponent"))
                    {
                        // Check if method is Play or internal helper, continue to caller
                        if (method.Name == "Play" || method.Name.StartsWith("StartNewEffectRunner") || method.Name.StartsWith("OnJuiceEvent"))
                        {
                            // If it's OnJuiceEvent, note the event system
                            if (method.Name == "OnJuiceEvent")
                            {
                                className = name;
                                methodName = "OnJuiceEvent";
                                fullInfo = $"{name} (Event Bus)";
                            }
                            continue;
                        }
                    }

                    className = name;
                    methodName = method.Name;
                    fullInfo = $"{className}.{methodName}()";
                    return;
                }
            }
            catch
            {
                // Fallback gracefully
            }
        }
#endif
    }
}
