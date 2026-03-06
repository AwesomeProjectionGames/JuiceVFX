using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewInstantiateEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Instantiate")]
    public class InstantiateEffectData : JuiceEffectData
    {
        [Tooltip("Prefab to instantiate.")]
        public GameObject Prefab;
        
        [Tooltip("Local position offset.")]
        public Vector3 LocalOffset;
        
        [Tooltip("What target should this effect apply to?")]
        public JuiceTargetType TargetType = JuiceTargetType.Target;
        
        [Tooltip("Parent the instantiated object to the resolved Target? (Overrides ParentToPlayer)")]
        public bool AttachToTarget = false;

        [Tooltip("Destroy the instantiated object after duration? (If false, it stays).")]
        public bool DestroyAfterDuration = true;

        public override JuiceEffectRunner CreateRunner()
        {
            return new InstantiateEffectRunner(this);
        }
        
        public override bool IsSameEffect(JuiceEffectData other)
        {
            return false; // This effect can run in parallel with other instances of itself without caching issues, so we return false to allow multiple instances to run simultaneously.
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

            Vector3 offset = baseRot * _data.LocalOffset;

            Vector3 spawnPos = basePos + offset;
            Quaternion spawnRot = baseRot; // User might want to offset rotation too, but currently only position offset in data

            _instance = Object.Instantiate(_data.Prefab, spawnPos, spawnRot);
            
            if (_data.AttachToTarget)
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
