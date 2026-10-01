using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interaction.util.ColorReveal
{
    /**
     * Makes an object gray and fades it into color when it is hovered, revealed by a spot or locked.
     *
     * The object needs a material with the FadeColor shader (_ColorAmount property).
     * The color is set per renderer with a MaterialPropertyBlock, so the material itself is not changed.
     */
    public class ColorRevealToggle : MonoBehaviour, IColorRevealable
    {
        /** How long the fade between gray and color takes (in seconds). */
        private const float TransitionDuration = 0.3f;
        /** If true, the renderers of all children get colored too, not only the one on this object.
         * This is important for the current vinyls
         */
        [Header("Renderers")]
        [SerializeField] private bool includeChildRenderers = false;

        /** True while the object is locked in color (e.g. the selected vinyl). */
        private bool stayColored;

        /** True while the current camera spot reveals this object. */
        private bool revealedBySpot;
        /** True while the mouse is over this object. */
        private bool revealedByHover;
        
        /** All renderers that use the FadeColor shader. */
        private readonly List<Renderer> _renderers = new List<Renderer>();
        /** Used to set _ColorAmount without changing the shared material. */
        private MaterialPropertyBlock _propertyBlock;
        /** The fade that is running right now, or null. */
        private Coroutine _transitionRoutine;
        /** True while the object is gray. */
        private bool _isGrayscale;

        /** _ColorAmount ID in shader */
        private static readonly int ColorAmountId = Shader.PropertyToID("_ColorAmount");
        /** _ColorAmount value for gray. */
        private const float GrayscaleAmount = 0f;
        /** _ColorAmount value for full color. */
        private const float ColorAmount = 1f;

        /**
         * Starts the object in grayscale.
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

            _isGrayscale = true;
            SetColorAmount(GrayscaleAmount);
        }

        /**
         * Sets whether the current camera spot reveals this object.
         *
         * @param revealed True if the spot shows the object in color.
         */
        public void SetColorReveal(bool revealed)
        {
            revealedBySpot = revealed;
            ApplyState();
        }

        /**
         * Sets whether the mouse is over the object.
         * showColor true means _ColorAmount 1, showColor false means _ColorAmount 0,
         * unless the spot or stayColored still keeps it in color.
         *
         * @param showColor True if the object is hovered.
         */
        public void SetColor(bool showColor)
        {
            revealedByHover = showColor;
            ApplyState();
        }

        /**
         * Checks spot, hover and stayColored and fades to color if one of them is true, otherwise to gray.
         * Does nothing if the object already has the right state.
         */
        private void ApplyState()
        {
            bool showColor = revealedBySpot || revealedByHover || stayColored;
            
            if (_isGrayscale == !showColor)
                return;
            
            _isGrayscale = !showColor;
            StartColorTransition(_isGrayscale ? GrayscaleAmount : ColorAmount);
        }

        /**
         * Fades the color amount from its current value to targetAmount over
         * TransitionDuration seconds using linear interpolation.
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

            while (time < TransitionDuration)
            {
                time += Time.deltaTime;
                // e.g. Clamp01(10 / 2) = Clamp01(5) = 1
                float t = Mathf.Clamp01(time / TransitionDuration);
                SetColorAmounts(startAmounts, targetAmount, t);

                yield return null;
            }

            SetColorAmount(targetAmount);
        }

        /**
         * Starts the fade to targetAmount and stops the old fade if one is still running.
         *
         * @param targetAmount The _ColorAmount to fade to.
         */
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
                Renderer[] childRenderers = GetComponentsInChildren<Renderer>(true);

                foreach (Renderer childRenderer in childRenderers)
                {
                    AddRendererIfCompatible(childRenderer);
                }

                return;
            }

            AddRendererIfCompatible(GetComponent<Renderer>());
        }

        /**
         * Adds the renderer to _renderers if it uses the FadeColor shader.
         *
         * @param rendererToAdd The renderer to check, can be null.
         */
        private void AddRendererIfCompatible(Renderer rendererToAdd)
        {
            if (rendererToAdd == null)
                return;

            if (!UsesColorRevealShader(rendererToAdd))
                return;

            _renderers.Add(rendererToAdd);
        }

        /**
         * Checks if one of the renderer's materials has the _ColorAmount property.
         *
         * @param rendererToCheck The renderer to check.
         * @return True if the renderer uses the FadeColor shader.
         */
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

        /**
         * Sets every renderer to a value between its start amount and targetAmount.
         *
         * @param startAmounts The _ColorAmount of each renderer when the fade started.
         * @param targetAmount The _ColorAmount to fade to.
         * @param t How far the fade is, from 0 to 1.
         */
        private void SetColorAmounts(float[] startAmounts, float targetAmount, float t)
        {
            for (int i = 0; i < _renderers.Count; i++)
            {
                // e.g. Lerp(1, 2, 0.5) = 1 + (2 - 1) * 0.5 = 1.5
                float amount = Mathf.Lerp(startAmounts[i], targetAmount, t);
                SetRendererColorAmount(_renderers[i], amount);
            }
        }

        /**
         * Sets _ColorAmount on one renderer via the property block.
         *
         * @param rendererToSet The renderer to change.
         * @param amount The new _ColorAmount.
         */
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
        
        /**
         * Locks or unlocks the object in color.
         *
         * @param stayColored True to keep the object colored, false to unlock it.
         */
        public void SetStayColored(bool stayColored)
        {
            this.stayColored = stayColored;
            SetColor(stayColored);
        }
    }
}
