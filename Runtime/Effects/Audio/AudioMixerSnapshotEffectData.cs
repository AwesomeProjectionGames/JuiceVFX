#nullable enable

using UnityEngine;
using UnityEngine.Audio;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewAudioMixerSnapshotEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Audio Mixer Snapshot")]
    public class AudioMixerSnapshotEffectData : JuiceEffectData
    {
        [Header("Audio Mixer Snapshot")]
        [Tooltip("The Audio Mixer Snapshot to transition to.")]
        public AudioMixerSnapshot? Snapshot;

        [Tooltip("Use the effect's Duration for the transition duration. If false, use CustomTransitionDuration.")]
        public bool UseEffectDuration = true;

        [Tooltip("Custom transition duration in seconds (if UseEffectDuration is false).")]
        public float CustomTransitionDuration = 1f;

        public override JuiceEffectRunner CreateRunner()
        {
            return new AudioMixerSnapshotEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            return other is AudioMixerSnapshotEffectData otherData &&
                   Snapshot == otherData.Snapshot;
        }
    }

    public class AudioMixerSnapshotEffectRunner : JuiceEffectRunner
    {
        private AudioMixerSnapshotEffectData _data;

        public AudioMixerSnapshotEffectRunner(AudioMixerSnapshotEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            if (_data.Snapshot != null)
            {
                float transitionDuration = _data.UseEffectDuration ? _data.Duration : _data.CustomTransitionDuration;
                _data.Snapshot.TransitionTo(transitionDuration);
            }
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            // Snapshot transition handles its own time internally in Unity.
            // There is no automatic resetting for snapshots here unless another effect transitions back.
        }
    }
}
