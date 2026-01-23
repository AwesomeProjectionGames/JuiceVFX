using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewInstantiateEffect", menuName = "JuiceVFX/Effects/Instantiate")]
    public class InstantiateEffectData : JuiceEffectData
    {
        [Tooltip("Prefab to instantiate.")]
        public GameObject Prefab;
        
        [Tooltip("Local position offset.")]
        public Vector3 LocalOffset;
        
        [Tooltip("Parent the instantiated object to the player?")]
        public bool ParentToPlayer = false;

        [Tooltip("Destroy the instantiated object after duration? (If false, it stays).")]
        public bool DestroyAfterDuration = true;

        [Tooltip("Parent the instantiated object to the resolved Target? (Overrides ParentToPlayer)")]
        public bool AttachToTarget = false;

        public override JuiceEffectRunner CreateRunner()
        {
            return new InstantiateEffectRunner(this);
        }
    }

    public class InstantiateEffectRunner : JuiceEffectRunner
    {
        private InstantiateEffectData _data;
        private GameObject _instance;

        public InstantiateEffectRunner(InstantiateEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            if (_data.Prefab == null)
            {
                Stop();
                return;
            }

            Vector3 basePos = GetTargetPosition(_data.TargetType);
            Quaternion baseRot = GetTargetRotation(_data.TargetType);
            Transform targetTransform = GetTargetTransform(_data.TargetType);

            // Calculate offset logic
            Vector3 offset = _data.LocalOffset;
            if (targetTransform != null)
            {
                // If we have a transform, treat offset as local
                offset = targetTransform.TransformDirection(_data.LocalOffset);
            }
            else
            {
                // Otherwise rotate offset by base rotation (e.g. contact normal)
                offset = baseRot * _data.LocalOffset;
            }

            Vector3 spawnPos = basePos + offset;
            Quaternion spawnRot = baseRot; // User might want to offset rotation too, but currently only position offset in data

            _instance = Object.Instantiate(_data.Prefab, spawnPos, spawnRot);
            
            if (_data.AttachToTarget && targetTransform != null)
            {
                 _instance.transform.SetParent(targetTransform, true);
            }
            else if (_data.ParentToPlayer)
            {
                _instance.transform.SetParent(player.transform, true);
            }
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
            if (_data.DestroyAfterDuration && _instance != null)
            {
                Object.Destroy(_instance);
            }
        }
    }
}
