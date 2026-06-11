using System;
using UnityEngine;

namespace LaserParticleAnalyzer.Core
{
    public class MeasurementController : MonoBehaviour
    {
        public enum MeasurementState
        {
            Idle,
            WarmingUp,
            Measuring,
            Processing,
            Complete
        }

        [Header("State Control")]
        [SerializeField]
        private MeasurementState currentState = MeasurementState.Idle;

        [SerializeField]
        public float laserIntensity = 1.0f;

        [SerializeField]
        public float measurementDuration = 7.0f;

        [SerializeField]
        public float continuousMeasurementInterval = 5.0f;

        [SerializeField]
        private bool continuousMode = false;

        private float stateTimer = 0f;
        private float continuousTimer = 0f;

        [Header("References")]
        [SerializeField]
        private OpticalSimulator opticalSimulator;

        [SerializeField]
        private DataProcessor dataProcessor;

        [SerializeField]
        private UI.UIManager uiManager;

        public event Action<MeasurementState> OnStateChanged;
        public event Action OnMeasurementStarted;
        public event Action OnMeasurementCompleted;

        private AudioSource audioSource;
        private AudioClip laserStartClip;
        private AudioClip completionClip;

        private void Awake()
        {
            if (opticalSimulator == null)
                opticalSimulator = GetComponentInChildren<OpticalSimulator>();
            if (dataProcessor == null)
                dataProcessor = GetComponentInChildren<DataProcessor>();
            if (uiManager == null)
                uiManager = GetComponentInChildren<UI.UIManager>();

            InitializeAudio();
        }

        private void Start()
        {
            Debug.Log("[MeasurementController] System initialized. State: " + currentState);
        }

        private void Update()
        {
            UpdateState();
            HandleKeyboardInput();
        }

        private void UpdateState()
        {
            stateTimer += Time.deltaTime;

            switch (currentState)
            {
                case MeasurementState.Idle:
                    HandleIdleState();
                    break;
                case MeasurementState.WarmingUp:
                    HandleWarmingUpState();
                    break;
                case MeasurementState.Measuring:
                    HandleMeasuringState();
                    break;
                case MeasurementState.Processing:
                    HandleProcessingState();
                    break;
                case MeasurementState.Complete:
                    HandleCompleteState();
                    break;
            }
        }

        private void HandleIdleState()
        {
            if (continuousMode)
            {
                continuousTimer += Time.deltaTime;
                if (continuousTimer >= continuousMeasurementInterval)
                {
                    StartMeasurement();
                    continuousTimer = 0f;
                }
            }
        }

        private void HandleWarmingUpState()
        {
            if (opticalSimulator != null)
            {
                float warmupProgress = Mathf.Clamp01(stateTimer / 0.5f);
                opticalSimulator.SetLaserIntensity(warmupProgress * laserIntensity);

                if (stateTimer >= 0.5f)
                {
                    SetState(MeasurementState.Measuring);
                }
            }
        }

        private void HandleMeasuringState()
        {
            if (opticalSimulator != null)
            {
                opticalSimulator.SetLaserIntensity(laserIntensity);

                if (stateTimer < 2.0f)
                {
                    opticalSimulator.UpdateBeamExtension(stateTimer / 2.0f);
                }

                if (stateTimer >= 2.0f && stateTimer < 3.5f)
                {
                    float scatterProgress = (stateTimer - 2.0f) / 1.5f;
                    opticalSimulator.UpdateScattering(scatterProgress);
                }

                if (stateTimer >= 3.5f && stateTimer < 5.0f)
                {
                    float focusProgress = (stateTimer - 3.5f) / 1.5f;
                    opticalSimulator.UpdateDetectorResponse(focusProgress);
                }

                if (stateTimer >= measurementDuration)
                {
                    SetState(MeasurementState.Processing);
                }
            }
        }

        private void HandleProcessingState()
        {
            if (dataProcessor != null)
            {
                dataProcessor.ProcessMeasurementData();

                if (stateTimer >= 1.0f)
                {
                    SetState(MeasurementState.Complete);
                }
            }
        }

        private void HandleCompleteState()
        {
            if (stateTimer >= 1.0f)
            {
                OnMeasurementCompleted?.Invoke();
                PlayCompletionSound();

                if (uiManager != null)
                {
                    uiManager.DisplayResults(dataProcessor.GetResults());
                }

                SetState(MeasurementState.Idle);
            }
        }

        private void SetState(MeasurementState newState)
        {
            if (currentState != newState)
            {
                currentState = newState;
                stateTimer = 0f;
                OnStateChanged?.Invoke(currentState);
                Debug.Log($"[MeasurementController] State changed to: {currentState}");
            }
        }

        public void StartMeasurement()
        {
            if (currentState == MeasurementState.Idle)
            {
                SetState(MeasurementState.WarmingUp);
                OnMeasurementStarted?.Invoke();
                PlayLaserStartSound();
                Debug.Log("[MeasurementController] Measurement started");
            }
        }

        public void PauseResume()
        {
            Debug.Log("[MeasurementController] Pause/Resume (placeholder)");
        }

        public void ResetAll()
        {
            SetState(MeasurementState.Idle);
            stateTimer = 0f;
            continuousTimer = 0f;

            if (opticalSimulator != null)
                opticalSimulator.Reset();
            if (dataProcessor != null)
                dataProcessor.Reset();
            if (uiManager != null)
                uiManager.Reset();

            Debug.Log("[MeasurementController] System reset");
        }

        public void SetContinuousMode(bool enable)
        {
            continuousMode = enable;
            Debug.Log($"[MeasurementController] Continuous mode: {(enable ? "enabled" : "disabled")}");
        }

        public MeasurementState GetCurrentState() => currentState;

        public void SetConcentration(float percent)
        {
            if (dataProcessor != null)
                dataProcessor.SetConcentration(Mathf.Clamp01(percent / 100f));
        }

        public void SetDistributionPreset(int presetIndex)
        {
            if (dataProcessor != null)
                dataProcessor.SetDistributionType((Models.DistributionType)presetIndex);
        }

        private void HandleKeyboardInput()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (currentState == MeasurementState.Idle)
                    StartMeasurement();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ResetAll();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                Screen.fullScreen = !Screen.fullScreen;
            }
        }

        private void InitializeAudio()
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            laserStartClip = GenerateSineWaveClip(800f, 0.2f, 22050);
            completionClip = GenerateSineWaveClip(440f, 0.5f, 22050);

            Debug.Log("[MeasurementController] Audio system initialized");
        }

        private void PlayLaserStartSound()
        {
            if (audioSource != null && laserStartClip != null)
            {
                audioSource.PlayOneShot(laserStartClip);
            }
        }

        private void PlayCompletionSound()
        {
            if (audioSource != null && completionClip != null)
            {
                audioSource.PlayOneShot(completionClip);
            }
        }

        private static AudioClip GenerateSineWaveClip(float frequency, float duration, int sampleRate)
        {
            int samples = (int)(duration * sampleRate);
            AudioClip clip = AudioClip.Create("SineWave_" + frequency, samples, 1, sampleRate, false);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-3f * t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.3f;
            }

            clip.SetData(data, 0);
            return clip;
        }
    }
}
