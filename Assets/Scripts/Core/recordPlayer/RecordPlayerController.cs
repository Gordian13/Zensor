using background;
using UnityEngine;

namespace recordPlayer
{
    public class RecordPlayerController : MonoBehaviour
    {
        [System.Serializable]
        private class VinylVariant
        {
            public VinylType type;
            [Tooltip("Das Vinyl-Modell auf dem Teller fuer diese Groesse.")]
            public Transform vinyl;
            [Tooltip("Winkel des Tonarms am Anfang der Platte (aussen).")]
            public float armPlayAngle = 45f;
            [Tooltip("Winkel des Tonarms am Ende der Platte (innen).")]
            public float armEndAngle = 80f;

            [Header("Cover (neben dem Player)")]
            public GameObject coverObject;
            public Renderer coverRenderer;

            [System.NonSerialized] public Renderer[] renderers;
            [System.NonSerialized] public Renderer labelRenderer;
        }

        [Header("References")]
        [SerializeField] private AudioSource audioSource;

        [Header("Record")]
        [SerializeField] private RecordData currentRecord;

        [Header("Animation")]
        [SerializeField] private Transform toneArmPivot;
        [SerializeField] private Transform platter;
        [SerializeField] private Vector3 vinylRotationAxis = Vector3.up;
        [SerializeField] private float armRestAngle = 0f;
        [SerializeField] private float armMoveSpeed = 1.5f;

        [Header("Vinyl Variants")]
        [SerializeField] private VinylVariant twelveInch;
        [SerializeField] private VinylVariant sevenInch;

        public int CurrentTrackIndex => _currentTrackIndex;
        public int TrackCount => currentRecord?.TrackCount ?? 0;
        public string CurrentTrackName => currentRecord?.GetTrack(_currentTrackIndex)?.name ?? "";

        private bool _isPlaying;
        private int _currentTrackIndex;
        private float _currentRPM = 33f;
        private float _totalDuration;
        private float _playedBeforeCurrentTrack;
        private VinylVariant _activeVariant;

        private void Awake()
        {
            _isPlaying = false;
            _currentTrackIndex = 0;

            CacheVariant(twelveInch);
            CacheVariant(sevenInch);

            SetVariantVisible(twelveInch, false);
            SetVariantVisible(sevenInch, false);
            HideCover(twelveInch);
            HideCover(sevenInch);
        }

        private void CacheVariant(VinylVariant variant)
        {
            if (variant == null || variant.vinyl == null)
            {
                Debug.LogWarning($"CacheVariant: Vinyl-Transform fehlt fuer Variante {variant?.type}.", this);
                return;
            }

            variant.renderers = variant.vinyl.GetComponentsInChildren<Renderer>(true);
            Debug.Log($"CacheVariant {variant.type}: {variant.renderers.Length} Renderer gefunden auf '{variant.vinyl.name}'.", this);
            foreach (Renderer r in variant.renderers)
            {
                if (r.name == "LabelFront")
                    variant.labelRenderer = r;
            }
        }

        void Update()
        {
            if (_isPlaying && _activeVariant?.vinyl != null)
            {
                platter.Rotate(Vector3.forward, _currentRPM * 6f * Time.deltaTime);
                _activeVariant.vinyl.Rotate(vinylRotationAxis, _currentRPM * 6f * Time.deltaTime);
            }

            float targetAngle;
            if (_activeVariant != null && _totalDuration > 0)
            {
                float progress = _isPlaying
                    ? (_playedBeforeCurrentTrack + audioSource.time) / _totalDuration
                    : _playedBeforeCurrentTrack / _totalDuration;
                targetAngle = Mathf.Lerp(_activeVariant.armPlayAngle, _activeVariant.armEndAngle, progress);
            }
            else
            {
                targetAngle = armRestAngle;
            }

            float currentAngle = toneArmPivot.localEulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, armMoveSpeed * Time.deltaTime);
            toneArmPivot.localEulerAngles = new Vector3(
                toneArmPivot.localEulerAngles.x,
                toneArmPivot.localEulerAngles.y,
                newAngle
            );
        }

        public void TogglePlay()
        {
            if (_isPlaying) Stop();
            else Play();
        }

        public void Play()
        {
            if (currentRecord == null || currentRecord.TrackCount == 0) return;

            audioSource.clip = currentRecord.GetTrack(_currentTrackIndex);
            audioSource.pitch = currentRecord.speed switch
            {
                Speed.Slow => 0.65f,
                Speed.Fast => 1.25f,
                Speed.Normal => 1f,
                _ => 1f
            };

            audioSource.Play();
            _isPlaying = true;
        }

        public void Stop()
        {
            audioSource.Stop();
            _isPlaying = false;
        }

        public void NextTrack()
        {
            if (currentRecord == null) return;
            if (_currentTrackIndex >= currentRecord.TrackCount - 1) return;

            AudioClip current = currentRecord.GetTrack(_currentTrackIndex);
            if (current != null) _playedBeforeCurrentTrack += current.length;
            _currentTrackIndex++;
            SnapArmToProgress();
            if (_isPlaying) Play();
        }

        public void PreviousTrack()
        {
            if (currentRecord == null) return;
            if (_currentTrackIndex <= 0) return;

            _currentTrackIndex--;
            AudioClip prev = currentRecord.GetTrack(_currentTrackIndex);
            if (prev != null) _playedBeforeCurrentTrack -= prev.length;
            _playedBeforeCurrentTrack = Mathf.Max(0f, _playedBeforeCurrentTrack);
            SnapArmToProgress();
            if (_isPlaying) Play();
        }

        private void SnapArmToProgress()
        {
            if (_activeVariant == null || _totalDuration <= 0) return;
            float progress = _playedBeforeCurrentTrack / _totalDuration;
            float angle = Mathf.Lerp(_activeVariant.armPlayAngle, _activeVariant.armEndAngle, progress);
            toneArmPivot.localEulerAngles = new Vector3(
                toneArmPivot.localEulerAngles.x,
                toneArmPivot.localEulerAngles.y,
                angle
            );
        }

        public void SetRecord(RecordData record)
        {
            Stop();
            currentRecord = record;
            _currentTrackIndex = 0;
            _playedBeforeCurrentTrack = 0f;
            _totalDuration = 0f;
            for (int i = 0; i < record.TrackCount; i++)
            {
                AudioClip clip = record.GetTrack(i);
                if (clip != null) _totalDuration += clip.length;
            }

            _activeVariant = GetVariant(record.vinylType);

            // Nur die passende Groesse anzeigen.
            SetVariantVisible(twelveInch, _activeVariant == twelveInch);
            SetVariantVisible(sevenInch, _activeVariant == sevenInch);

            ApplyLabel(record);
            ApplyCover(record);
        }

        public void ClearRecord()
        {
            Stop();
            currentRecord = null;
            _currentTrackIndex = 0;
            _activeVariant = null;
            SetVariantVisible(twelveInch, false);
            SetVariantVisible(sevenInch, false);
            HideCover(twelveInch);
            HideCover(sevenInch);
        }

        private VinylVariant GetVariant(VinylType type)
        {
            if (sevenInch != null && sevenInch.type == type && sevenInch.vinyl != null) return sevenInch;
            if (twelveInch != null && twelveInch.type == type && twelveInch.vinyl != null) return twelveInch;
            return twelveInch; // Fallback
        }

        private static void SetVariantVisible(VinylVariant variant, bool visible)
        {
            if (variant?.renderers == null) return;
            foreach (Renderer r in variant.renderers)
                r.enabled = visible;
        }

        public void SetPitch(float rpm)
        {
            _currentRPM = rpm;
            audioSource.pitch = rpm == 45f ? 1.0f : 0.65f;
        }

        private void ApplyLabel(RecordData record)
        {
            Renderer labelRenderer = _activeVariant?.labelRenderer;
            if (labelRenderer == null || record == null || record.labelFrontTexture == null) return;

            Material mat = labelRenderer.material;
            if (mat.HasProperty("_BaseMap"))
                mat.SetTexture("_BaseMap", record.labelFrontTexture);
            else if (mat.HasProperty("_MainTex"))
                mat.SetTexture("_MainTex", record.labelFrontTexture);
        }

        private void ApplyCover(RecordData record)
        {
            // Cover der nicht-aktiven Groesse ausblenden.
            if (_activeVariant != twelveInch) HideCover(twelveInch);
            if (_activeVariant != sevenInch) HideCover(sevenInch);

            if (_activeVariant == null) return;

            bool hasCover = record != null && record.coverFrontTexture != null;

            if (_activeVariant.coverObject != null)
                _activeVariant.coverObject.SetActive(hasCover);

            if (_activeVariant.coverRenderer != null && hasCover)
            {
                Material mat = _activeVariant.coverRenderer.material;
                if (mat.HasProperty("_BaseMap"))
                    mat.SetTexture("_BaseMap", record.coverFrontTexture);
                else if (mat.HasProperty("_MainTex"))
                    mat.SetTexture("_MainTex", record.coverFrontTexture);
            }
        }

        private static void HideCover(VinylVariant variant)
        {
            if (variant?.coverObject != null)
                variant.coverObject.SetActive(false);
        }
    }
}
