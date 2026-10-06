using background;
using UnityEngine;

namespace recordPlayer
{
    /**
     * @brief Drives the turntable: plays a selected record, animates platter and
     *        tonearm, and shows the matching vinyl disc and sleeve for its size.
     *
     * The controller is size-aware. Each physical record size (7" / 12") is described
     * by a VinylVariant that bundles its disc model, its cover sleeve and the two
     * tonearm angles that frame that disc. SetRecord() picks the variant from the
     * record's VinylType, shows only that disc and sleeve, and hides the other.
     *
     * Playback is manual: the connected flow calls SetRecord() to load a record, but
     * audio only starts once Play()/TogglePlay() is triggered (e.g. by the physical
     * Start/Stop button via RecordButton). SetPitch() is driven by the 33/45 RPM
     * buttons; the per-record Speed enum sets the base pitch in Play().
     *
     * The tonearm tracks overall progress across all tracks of a record, so at the
     * end of the last track it sits at armEndAngle. Track skipping (NextTrack /
     * PreviousTrack) keeps a running sum of the time played before the current track
     * so the arm position stays correct even on multi-track records.
     */
    public class RecordPlayerController : MonoBehaviour
    {
        /**
         * @brief Inspector-configured description of one record size on the player.
         *
         * Assign the on-platter disc model (vinyl), its separate cover sleeve
         * (coverObject / coverRenderer) and the tonearm start/end angles for that
         * size. The renderer caches are filled at runtime by CacheVariant().
         */
        [System.Serializable]
        private class VinylVariant
        {
            public VinylType type;
            [Tooltip("The vinyl model on the platter for this size.")]
            public Transform vinyl;
            [Tooltip("Tonearm angle at the start of the record (outer edge).")]
            public float armPlayAngle = 45f;
            [Tooltip("Tonearm angle at the end of the record (inner groove).")]
            public float armEndAngle = 80f;

            [Header("Cover (next to the player)")]
            public GameObject coverObject;
            public Renderer coverRenderer;

            /// All renderers of the disc model, cached so visibility can be toggled cheaply.
            [System.NonSerialized] public Renderer[] renderers;
            /// The renderer named "LabelFront", used to swap the record's label texture.
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

        /// Zero-based index of the track currently selected for playback.
        public int CurrentTrackIndex => _currentTrackIndex;
        /// Number of tracks on the loaded record, or 0 when none is loaded.
        public int TrackCount => currentRecord?.TrackCount ?? 0;
        /// Name of the current track's AudioClip, or empty when none is loaded.
        public string CurrentTrackName => currentRecord?.GetTrack(_currentTrackIndex)?.name ?? "";

        private bool _isPlaying;
        private int _currentTrackIndex;
        private float _currentRPM = 33f;
        /// Total length of all tracks on the loaded record; used to map arm angle to progress.
        private float _totalDuration;
        /// Summed length of all tracks before the current one; offsets the arm for multi-track records.
        private float _playedBeforeCurrentTrack;
        /// The size variant matching the loaded record; null when nothing is loaded.
        private VinylVariant _activeVariant;

        /**
         * @brief Caches both size variants and starts with every disc and sleeve hidden.
         */
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

        /**
         * @brief Collects the disc's renderers and finds its label renderer once.
         * @param variant The size variant to cache; ignored when its disc is unassigned.
         *
         * Caching the renderers lets SetVariantVisible() toggle the disc via
         * Renderer.enabled instead of GameObject.SetActive(), which keeps the object
         * active so attached systems (e.g. the ColorReveal coroutines) are not stopped.
         */
        private void CacheVariant(VinylVariant variant)
        {
            if (variant == null || variant.vinyl == null)
            {
                Debug.LogWarning($"CacheVariant: vinyl transform missing for variant {variant?.type}.", this);
                return;
            }

            variant.renderers = variant.vinyl.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer r in variant.renderers)
            {
                if (r.name == "LabelFront")
                    variant.labelRenderer = r;
            }
        }

        /**
         * @brief Spins platter and disc while playing and eases the tonearm toward its target angle.
         *
         * The target angle maps overall playback progress (time already played plus the
         * current clip time, divided by the record's total duration) onto the active
         * variant's play/end angle range. With no record loaded the arm returns to rest.
         */
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

        /**
         * @brief Toggles between playing and stopped. Intended for the Start/Stop button.
         */
        public void TogglePlay()
        {
            if (_isPlaying) Stop();
            else Play();
        }

        /**
         * @brief Starts playback of the current track, applying the record's base pitch.
         *
         * Does nothing when no record (or an empty record) is loaded. The per-record
         * Speed enum sets the base pitch here; the 33/45 RPM buttons override it later
         * through SetPitch().
         */
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

        /**
         * @brief Stops audio playback. The arm then eases back toward its progress position.
         */
        public void Stop()
        {
            audioSource.Stop();
            _isPlaying = false;
        }

        /**
         * @brief Advances to the next track. No-op when already on the last track.
         *
         * Adds the finished track's length to the played-before total so the arm keeps
         * tracking overall progress, then restarts playback if the player was playing.
         */
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

        /**
         * @brief Goes back to the previous track. No-op when already on the first track.
         *
         * Subtracts the now-current track's length from the played-before total (clamped
         * at zero), then restarts playback if the player was playing.
         */
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

        /**
         * @brief Immediately sets the tonearm to the start of the current track.
         *
         * Used when skipping tracks so the arm jumps to the new position instead of
         * sweeping there slowly over several frames via Update().
         */
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

        /**
         * @brief Loads a record: resets playback state, picks the size variant and shows it.
         * @param record The record to place on the turntable.
         *
         * Precomputes the total duration across all tracks (for arm progress), selects the
         * variant from record.vinylType, shows only that disc, and applies the label and
         * cover textures. Playback is not started here; the user triggers it manually.
         */
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

            // Show only the matching size.
            SetVariantVisible(twelveInch, _activeVariant == twelveInch);
            SetVariantVisible(sevenInch, _activeVariant == sevenInch);

            ApplyLabel(record);
            ApplyCover(record);
        }

        /**
         * @brief Unloads the record and hides every disc and cover.
         *
         * Called when leaving the player view, so the turntable returns to its empty state.
         */
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

        /**
         * @brief Returns the variant whose type matches and whose disc is assigned.
         * @param type The record's vinyl size.
         * @return The matching variant, or the 12" variant as a fallback.
         */
        private VinylVariant GetVariant(VinylType type)
        {
            if (sevenInch != null && sevenInch.type == type && sevenInch.vinyl != null) return sevenInch;
            if (twelveInch != null && twelveInch.type == type && twelveInch.vinyl != null) return twelveInch;
            return twelveInch; // Fallback
        }

        /**
         * @brief Shows or hides a variant's disc via Renderer.enabled (keeps the object active).
         * @param variant The size variant to toggle.
         * @param visible True to show the disc, false to hide it.
         */
        private static void SetVariantVisible(VinylVariant variant, bool visible)
        {
            if (variant?.renderers == null) return;
            foreach (Renderer r in variant.renderers)
                r.enabled = visible;
        }

        /**
         * @brief Sets playback speed from the RPM buttons.
         * @param rpm 45 plays at normal pitch (1.0); any other value (33) plays slower (0.65).
         */
        public void SetPitch(float rpm)
        {
            _currentRPM = rpm;
            audioSource.pitch = rpm == 45f ? 1.0f : 0.65f;
        }

        /**
         * @brief Applies the record's front-label texture to the active disc's label renderer.
         * @param record The record whose label texture to show.
         *
         * Supports both the URP (_BaseMap) and built-in (_MainTex) material property names.
         */
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

        /**
         * @brief Shows the active size's cover sleeve with the record's front-cover texture.
         * @param record The record whose cover texture to show; null hides the cover.
         *
         * The non-active size's cover is always hidden first, so only one sleeve is ever
         * visible. Supports both the URP (_BaseMap) and built-in (_MainTex) property names.
         */
        private void ApplyCover(RecordData record)
        {
            // Hide the cover of the non-active size.
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

        /**
         * @brief Hides a variant's cover sleeve if one is assigned.
         * @param variant The size variant whose cover to hide.
         */
        private static void HideCover(VinylVariant variant)
        {
            if (variant?.coverObject != null)
                variant.coverObject.SetActive(false);
        }
    }
}
