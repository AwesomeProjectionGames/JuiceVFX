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

            Vector3 spawnPos = player.transform.position + player.transform.TransformDirection(_data.LocalOffset); // Apply offset in local space
            Quaternion spawnRot = player.transform.rotation;

            _instance = Object.Instantiate(_data.Prefab, spawnPos, spawnRot);
            
            if (_data.ParentToPlayer)
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
