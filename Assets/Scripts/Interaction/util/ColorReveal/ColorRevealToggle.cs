using System.Collections;
using DefaultNamespace.GrayScale;
using UnityEngine;

namespace util.GrayScale
{
    [RequireComponent(typeof(Renderer))]
    public class ColorRevealToggle : MonoBehaviour, IColorRevealable
    {
        [SerializeField] private float transitionDuration = 1f;
        [SerializeField] private bool startGrayscale = true;

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        // for animations
        private Coroutine _transitionRoutine;
        private bool _isGrayscale;

        // get shader properties
        private static readonly int ColorAmountId = Shader.PropertyToID("_ColorAmount");
        
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            // https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html
            _propertyBlock = new MaterialPropertyBlock();

            if (_renderer.sharedMaterial == null || !_renderer.sharedMaterial.HasProperty(ColorAmountId))
            {
                Debug.LogWarning($"{name} uses the script GrayscaleToggle, but does not uses the custom shader", this);
            }

            _isGrayscale = startGrayscale;
            SetColorAmount(_isGrayscale ? 1f : 0f);
        }

        // main interface function 
        public void ToggleColor()
        {
            _isGrayscale = !_isGrayscale;

            float targetAmount = _isGrayscale ? 1f : 0f;

            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            _transitionRoutine = StartCoroutine(AnimateColorAmount(targetAmount));

            Debug.Log($"Toggle Grayscale on Object {gameObject.name}");
        }

        // slowly fade the color in 
        private IEnumerator AnimateColorAmount(float targetAmount)
        {
            float startAmount = GetColorAmount();
            float time = 0f;

            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / transitionDuration);

                float amount = Mathf.Lerp(startAmount, targetAmount, t);
                SetColorAmount(amount);

                yield return null;
            }

            SetColorAmount(targetAmount);
        }

        // set new Color Amount each step
        private void SetColorAmount(float amount)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(ColorAmountId, amount);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        // get current Color Amount
        private float GetColorAmount()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            return _propertyBlock.GetFloat(ColorAmountId);
        }
    }
}
