using System.Collections;
using UnityEngine;

namespace Interaction.util.ColorReveal
{
    [RequireComponent(typeof(Renderer))]
    public class ColorRevealToggle : MonoBehaviour, IColorRevealable
    {
        [SerializeField] private float transitionDuration = 1f;
        [SerializeField] private bool startGrayscale = true;

        private Renderer _renderer;
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _transitionRoutine;
        private bool _isGrayscale;

        // _ColorAmount ID in shader
        private static readonly int ColorAmountId = Shader.PropertyToID("_ColorAmount");

        /**
         * Sets the initial color amount based on startGrayscale.
         * Source: https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html
         */
        private void Awake()
        {
            _renderer = GetComponent<Renderer>();
            _propertyBlock = new MaterialPropertyBlock();

            if (_renderer.sharedMaterial == null || !_renderer.sharedMaterial.HasProperty(ColorAmountId))
            {
                Debug.LogWarning($"{name} uses ColorRevealToggle, but its material does not use the custom shader.", this);
            }

            _isGrayscale = startGrayscale;
            SetColorAmount(_isGrayscale ? 1f : 0f);
        }

        /**
         * Toggles between grayscale and color, restarting the fade animation if one is already running.
         */
        public void ToggleColor()
        {
            _isGrayscale = !_isGrayscale;

            float targetAmount = _isGrayscale ? 1f : 0f;

            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            _transitionRoutine = StartCoroutine(AnimateColorAmount(targetAmount));

            Debug.Log($"Toggled grayscale on object {gameObject.name}");
        }

        /**
         * Fades the color amount from its current value to targetAmount over
         * transitionDuration seconds using linear interpolation.
         *
         * Mathf.Lerp: interpolates between startAmount and targetAmount based on t.
         * Mathf.Clamp01: keeps t between 0 and 1, even if a frame spike pushes time past the duration.
         *
         * Sources:
         *   https://docs.unity3d.com/Manual/Coroutines.html
         *   https://docs.unity3d.com/ScriptReference/Mathf.Lerp.html
         *   https://docs.unity3d.com/ScriptReference/Mathf.Clamp01.html
         */
        private IEnumerator AnimateColorAmount(float targetAmount)
        {
            float startAmount = GetColorAmount();
            float time = 0f;

            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                // e.g. Clamp01(10 / 2) = Clamp01(5) = 1
                float t = Mathf.Clamp01(time / transitionDuration);
                // e.g. Lerp(1, 2, 0.5) = 1 + (2 - 1) * 0.5 = 1.5
                float amount = Mathf.Lerp(startAmount, targetAmount, t);
                SetColorAmount(amount);

                yield return null;
            }

            SetColorAmount(targetAmount);
        }

        /**
         * Sets the _ColorAmount value in the shader via the renderer's property block.
         */
        private void SetColorAmount(float amount)
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(ColorAmountId, amount);
            _renderer.SetPropertyBlock(_propertyBlock);
        }

        /**
         * Reads the current _ColorAmount value from the shader via the renderer's property block.
         */
        private float GetColorAmount()
        {
            _renderer.GetPropertyBlock(_propertyBlock);
            return _propertyBlock.GetFloat(ColorAmountId);
        }
    }
}