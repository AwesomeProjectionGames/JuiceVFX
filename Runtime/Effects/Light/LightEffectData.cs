using UnityEngine;

namespace JuiceVFX
{
    [CreateAssetMenu(fileName = "NewLightEffect", menuName = "AwesomeProjection/JuiceVFX/Effects/Light/Light")]
    public class LightEffectData : JuiceEffectData
    {
        [Tooltip("What target should this effect apply to?")]
        public JuiceTargetType TargetType = JuiceTargetType.Target;

        [Tooltip("Color of the light.")]
        public Color LightColor = Color.white;

        [Tooltip("Intensity multiplier curve.")]
        public AnimationCurve IntensityCurve = AnimationCurve.EaseInOut(0, 5, 1, 0);

        [Tooltip("Range of the light.")]
        public float Range = 10f;

        [Tooltip("Local offset for the light.")]
        public Vector3 LocalOffset;

        public override JuiceEffectRunner CreateRunner()
        {
            return new LightEffectRunner(this);
        }

        public override bool IsSameEffect(JuiceEffectData other)
        {
            return false; // This effect can run in parallel with other instances of itself without caching issues, so we return false to allow multiple instances to run simultaneously.
        }
    }

    public class LightEffectRunner : JuiceEffectRunner
    {
        private LightEffectData _data;
        private GameObject _lightObj;
        private Light _lightComp;

        public LightEffectRunner(LightEffectData data)
        {
            _data = data;
        }

        public override void OnStart(JuicePlayer player)
        {
            _lightObj = new GameObject("JuiceLight");
            if (_data.TargetType == JuiceTargetType.ContactPoint && Context.ContactPoint != null)
            {
                _lightObj.transform.position = Context.ContactPoint.Value + _data.LocalOffset;
            }
            else
            {
                _lightObj.transform.position = player.transform.position + player.transform.TransformDirection(_data.LocalOffset);
            }
            _lightObj.transform.SetParent(player.transform, true);

            _lightComp = _lightObj.AddComponent<Light>();
            _lightComp.type = LightType.Point;
            _lightComp.range = _data.Range;
            _lightComp.color = _data.LightColor;
        }

        public override void OnUpdate(float deltaTime)
        {
            _timer += deltaTime;
            float t = Mathf.Clamp01(_timer / _data.Duration);
            if (_data.Duration <= 0) t = 1f;

            float intensity = _data.IntensityCurve.Evaluate(t);
            _lightComp.intensity = intensity;

            if (t >= 1f)
            {
                Stop();
            }
        }

        public override void OnStop()
        {
            if (_lightObj != null)
            {
                Object.Destroy(_lightObj);
            }
        }
    }
}
