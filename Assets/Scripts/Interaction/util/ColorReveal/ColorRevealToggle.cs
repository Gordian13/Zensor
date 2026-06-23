using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction.util.ColorReveal
{
    public class ColorRevealToggle : MonoBehaviour, IColorRevealable
    {
        [SerializeField] private float transitionDuration = 1f;
        [SerializeField] private bool startGrayscale = true;
        [SerializeField] private bool includeChildRenderers = false;
        [SerializeField] private bool includeInactiveChildren = true;
        [SerializeField] private bool stayColored = false;

        private readonly List<Renderer> _renderers = new List<Renderer>();
        private MaterialPropertyBlock _propertyBlock;
        private Coroutine _transitionRoutine;
        private bool _isGrayscale;

        // _ColorAmount ID in shader
        private static readonly int ColorAmountId = Shader.PropertyToID("_ColorAmount");
        private const float GrayscaleAmount = 0f;
        private const float ColorAmount = 1f;

        /**
         * Sets the initial color amount based on startGrayscale.
         * Source: https://docs.unity3d.com/ScriptReference/MaterialPropertyBlock.html
         */
        private void Awake()
        {
            _propertyBlock = new MaterialPropertyBlock();
            CacheRenderers();

            if (_renderers.Count == 0)
            {
                Debug.LogWarning($"{name} uses ColorRevealToggle, but no renderer with the custom shader was found.", this);
            }

            _isGrayscale = startGrayscale;
            SetColorAmount(_isGrayscale ? GrayscaleAmount : ColorAmount);
        }

        /**
         * Toggles between grayscale and color, restarting the fade animation if one is already running.
         */
        public void ToggleColor()
        {
            SetColor(_isGrayscale);

            Debug.Log($"Toggled grayscale on object {gameObject.name}");
        }

        public void SetColorReveal(bool revealed)
        {
            SetColor(revealed);
        }

        /**
         * Sets the object to color or grayscale directly.
         * showColor true means _ColorAmount 1, showColor false means _ColorAmount 0.
         */
        public void SetColor(bool showColor)
        {
            if (stayColored && !showColor)
                return;

            _isGrayscale = !showColor;
            StartColorTransition(_isGrayscale ? GrayscaleAmount : ColorAmount);
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
            float[] startAmounts = GetColorAmounts();
            float time = 0f;

            while (time < transitionDuration)
            {
                time += Time.deltaTime;
                // e.g. Clamp01(10 / 2) = Clamp01(5) = 1
                float t = Mathf.Clamp01(time / transitionDuration);
                SetColorAmounts(startAmounts, targetAmount, t);

                yield return null;
            }

            SetColorAmount(targetAmount);
        }

        private void StartColorTransition(float targetAmount)
        {
            if (_transitionRoutine != null)
                StopCoroutine(_transitionRoutine);

            _transitionRoutine = StartCoroutine(AnimateColorAmount(targetAmount));
        }

        /**
         * Caches this object's renderer, or this object's renderer plus all child renderers.
         */
        private void CacheRenderers()
        {
            _renderers.Clear();

            if (includeChildRenderers)
            {
                Renderer[] childRenderers = GetComponentsInChildren<Renderer>(includeInactiveChildren);

                foreach (Renderer childRenderer in childRenderers)
                {
                    AddRendererIfCompatible(childRenderer);
                }

                return;
            }

            AddRendererIfCompatible(GetComponent<Renderer>());
        }

        private void AddRendererIfCompatible(Renderer rendererToAdd)
        {
            if (rendererToAdd == null)
                return;

            if (!UsesColorRevealShader(rendererToAdd))
                return;

            _renderers.Add(rendererToAdd);
        }

        private bool UsesColorRevealShader(Renderer rendererToCheck)
        {
            foreach (Material material in rendererToCheck.sharedMaterials)
            {
                if (material != null && material.HasProperty(ColorAmountId))
                    return true;
            }

            return false;
        }

        /**
         * Sets the _ColorAmount value in the shader via the renderer's property block.
         */
        private void SetColorAmount(float amount)
        {
            foreach (Renderer rendererToSet in _renderers)
            {
                SetRendererColorAmount(rendererToSet, amount);
            }
        }

        private void SetColorAmounts(float[] startAmounts, float targetAmount, float t)
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                // e.g. Lerp(1, 2, 0.5) = 1 + (2 - 1) * 0.5 = 1.5
                float amount = Mathf.Lerp(startAmounts[i], targetAmount, t);
                SetRendererColorAmount(_renderers[i], amount);
            }
        }

        private void SetRendererColorAmount(Renderer rendererToSet, float amount)
        {
            rendererToSet.GetPropertyBlock(_propertyBlock);
            _propertyBlock.SetFloat(ColorAmountId, amount);
            rendererToSet.SetPropertyBlock(_propertyBlock);
        }

        /**
         * Reads the current _ColorAmount value from the shader via the renderer's property block.
         */
        private float[] GetColorAmounts()
        {
            float[] colorAmounts = new float[_renderers.Count];

            for (int i = 0; i < _renderers.Count; i++)
            {
                _renderers[i].GetPropertyBlock(_propertyBlock);
                colorAmounts[i] = _propertyBlock.GetFloat(ColorAmountId);
            }

            return colorAmounts;
        }
        
        public void SetStayColored(bool stayColored)
        {
            this.stayColored = stayColored;
            if (stayColored)
            {
                SetColor(true);
            }
        }
    }
}
