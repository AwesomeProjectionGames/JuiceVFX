using UnityEngine;
using SoundManager;
using SoundManager.Modifier;

namespace JuiceVFX.Integrations.SoundManager
{
    /// <summary>
    /// Effect that plays a sound using the SoundManager system and modifiers.
    /// </summary>
    [CreateAssetMenu(fileName = "NewSfxEffect", menuName = "JuiceVFX/Effects/SoundManager SFX")]
    public class SfxEffectData : JuiceEffectData
    {
        [Header("Sound Settings")]
        public AudioClip Clip;
        [Tooltip("If null, a default AudioSource will be created.")]
        public UnityEngine.Audio.AudioMixerGroup MixerGroup;

        [Header("Modifiers")]
        public PitchModifier Pitch = new PitchModifier();
        public VolumeModifier Volume = new VolumeModifier();
        // RandomClipModifier might be redundant if we specify one clip, but useful if we want to pick from a list?
        // SoundManager's RandomClipModifier has a list of clips.
        public RandomClipModifier RandomClip = new RandomClipModifier();
        
        public override JuiceEffectRunner CreateRunner()
        {
            return new SfxEffectRunner(this);
        }
    }

    public class SfxEffectRunner : JuiceEffectRunner
    {
        private SfxEffectData _data;
        private AudioSource _source;
        private GameObject _audioObj;

        public SfxEffectRunner(SfxEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            // Spawn a temporary audio object
            _audioObj = new GameObject("JuiceSFX_" + (_data.Clip ? _data.Clip.name : "Null"));
            _audioObj.transform.position = player.transform.position; // 3D sound at location
            
            _source = _audioObj.AddComponent<AudioSource>();
            _source.clip = _data.Clip;
            _source.outputAudioMixerGroup = _data.MixerGroup;
            _source.playOnAwake = false;
            _source.spatialBlend = 1f; // 3D sound

            // Apply modifiers directly?
            // Modifiers in SoundManager take an AudioSource.
            
            // Random Clip Logic
            // The SoundManager RandomClipModifier likely overrides the clip.
             _data.RandomClip.ModifyAudio(_source);
            
            // If the RandomClipModifier didn't set a clip (because it was disabled or empty), keep the default one.
            if (_source.clip == null) _source.clip = _data.Clip;

            // Apply others
            _data.Pitch.ModifyAudio(_source);
            _data.Volume.ModifyAudio(_source);

            _source.Play();

            // Auto-destroy helper
            // We can rely on basic Destroy(obj, duration) but since we are in a runner, we can manage it.
            // If duration > clip length, we wait. If duration < clip length, we might cut it effectively if we maintain reference.
            // Usually SFX are fire and forget.
            // However, JuiceEffectRunner assumes control. 
            // If we destroy the object in OnStop(), the sound stops.
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            if (_timer >= _data.Duration)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_audioObj != null)
            {
                Object.Destroy(_audioObj);
            }
        }
    }
}
