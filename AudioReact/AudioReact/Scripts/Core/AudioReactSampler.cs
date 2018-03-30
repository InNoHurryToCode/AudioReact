using System;
using UnityEngine;
using UnityEngine.Audio;

namespace AudioReact
{
    public enum FrequencyRange
    {
        SubBase,        // 20 - 60 Hz
        Bass,           // 60 - 250 Hz
        LowMidrange,    // 250 - 500 Hz
        Midrange,       // 500 - 2000 Hz
        UpperMidrange,  // 2000 - 4000 Hz
        High,           // 4000 - 6000 Hz
        VeryHigh,       // 6000 - 20000 Hz
        Decibel,        // 20 - 20000 Hz
    };

    [Serializable]
    public class AudioReactInput
    {
        public const float Frequency = 24000.0f;
        public string CurrentAudioInput { get; private set; }
        public float Delay { get; private set; }
        public string[] InputDevices { get; private set; }
        public int ActiveDevice { get; private set; }

        public bool Use;
        public float VolumeThreshold = 0.01f;
        public float AudioMultiplier;
        public AudioMixerGroup SilentMixer;

        private void Awake()
        {
            if (SilentMixer == null)
            {
                Debug.LogError("AudioReactInput: mixer not assigned");
            }

            CurrentAudioInput = "none";
            Delay = 0.03f;
        }

        public string[] GetDevices()
        {
            InputDevices = new string[Microphone.devices.Length];

            for (int i = 0; i < Microphone.devices.Length; i++)
            {
                InputDevices[i] = Microphone.devices[i].ToString();
                Debug.Log("AudioReactInput: device[" + i + "]: " + InputDevices[i]);
            }

            return InputDevices;
        }
    }

    public class AudioReactSampler : MonoBehaviour
    {
        private ThreadPool threadPool;
        private int waitForJoinTime;
        public float[] FrequencySamples { get; private set; }
        private float[] frequencyData;  // used for GetFrequencyVol
        private float[] outputData;     // used for GetRMS
        private float frequencyMax;
        private int samplesAmount;
        private float audioMultiplier;
        private float audioVolume;

        public float AudioSourceMultiplier;
        public AudioSource AudioSource;
        public AudioReactInput Input;

        // singleton
        private static AudioReactSampler instance;
        public static AudioReactSampler Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AudioReactSampler>();
                }
                if (instance == null)
                {
                    Debug.LogError("AudioReactSampler could not be found");
                }

                return instance;
            }
            set
            {
                instance = value;
            }
        }

        void Awake()
        {
            waitForJoinTime = 2000;
            threadPool = new ThreadPool(SystemInfo.processorCount, waitForJoinTime);

            samplesAmount = 1024;
            frequencyData = new float[samplesAmount];
            outputData = new float[samplesAmount];

            frequencyMax = (float)AudioSettings.outputSampleRate / 2;
            FrequencySamples = new float[Enum.GetNames(typeof(FrequencyRange)).Length];

            if (AudioSource == null)
            {
                Debug.LogError("AudioReactSampler: no audio sources assigned");
            }

            if (Input.Use)
            {
                SwitchInputDevice(Input.ActiveDevice);
            }
        }

        private void Update()
        {
            if (AudioSource.mute && !Input.Use)
            {
                return;
            }

            if (Input.Use)
            {
                if (!AudioSource.isPlaying)
                {
                    int microphoneSamples = Microphone.GetPosition(Input.CurrentAudioInput);

                    if (microphoneSamples / AudioReactInput.Frequency > Input.Delay)
                    {
                        AudioSource.timeSamples = (int)(microphoneSamples - (Input.Delay * AudioReactInput.Frequency));
                        AudioSource.Play();
                    }
                }
                
                audioMultiplier = Input.AudioMultiplier;
            }
            else
            {
                audioMultiplier = AudioSourceMultiplier;
            }

            AudioSource.GetSpectrumData(frequencyData, 0, FFTWindow.BlackmanHarris);
            AudioSource.GetOutputData(outputData, 0);
            audioVolume = AudioSource.volume;

            for (int i = 0; i < FrequencySamples.Length; i++)
            {
                Action action;
                FrequencyRange frequencyRange = (FrequencyRange)i;

                if (frequencyRange != FrequencyRange.Decibel)
                {
                    // Get Frequency Volume
                    action = new Action(() =>
                    {
                        float[] range = GetFrequencyRange(frequencyRange);
                        float frequencyLow = range[0];
                        float frequencyHigh = range[1];

                        int n1 = (int)Mathf.Floor(frequencyLow * samplesAmount / frequencyMax);
                        int n2 = (int)Mathf.Floor(frequencyHigh * samplesAmount / frequencyMax);

                        float sum = 0;

                        for (int j = n1; j <= n2; j++)
                        {
                            if (j < frequencyData.Length && j >= 0)
                            {
                                sum += Mathf.Abs(frequencyData[j]);
                            }
                        }

                        if (Input.Use)
                        {
                            if (sum < Input.VolumeThreshold)
                            {
                                sum = 0;
                            }
                        }
                        else
                        {
                            sum = sum * audioVolume;
                        }

                        lock (FrequencySamples)
                        {
                            FrequencySamples[i] = sum / (n2 - n1 + 1);
                        }
                    });
                }
                else
                {
                    // get RMS
                    action = new Action(() =>
                    {
                        float sum = 0;

                        for (int j = 0; j < outputData.Length; j++)
                        {
                            sum += outputData[j] * outputData[j];
                        }

                        float rmsValue = Mathf.Sqrt(sum / samplesAmount);

                        if (Input.Use)
                        {
                            if (rmsValue < Input.VolumeThreshold)
                            {
                                rmsValue = 0;
                            }
                        }
                        else
                        {
                            rmsValue = rmsValue * audioVolume;
                        }

                        lock (FrequencySamples)
                        {
                            FrequencySamples[i] = rmsValue;
                        }
                    });
                }

                if (threadPool.MaxThreads > 1)
                {
                    threadPool.CreateThread(action, "GetFrequencySample[" + i + "]");
                }
                else
                {
                    action();
                }
            }

            if (threadPool.MaxThreads > 1)
            {
                for (int i = 0; i < FrequencySamples.Length; i++)
                {
                    threadPool.JoinThread(i);
                }

                threadPool.OnUpdate();
            }
            
            for (int i = 0; i < FrequencySamples.Length; i++)
            {
                FrequencySamples[i] *= audioMultiplier;
            }
        }

        public float[] GetFrequencyRange(FrequencyRange frequencyRange)
        {
            switch (frequencyRange)
            {
                case FrequencyRange.SubBase:
                    return new float[2] { 0, 60 };
                case FrequencyRange.Bass:
                    return new float[2] { 60, 250 };
                case FrequencyRange.LowMidrange:
                    return new float[2] { 250, 500 };
                case FrequencyRange.Midrange:
                    return new float[2] { 500, 2000 };
                case FrequencyRange.UpperMidrange:
                    return new float[2] { 2000, 4000 };
                case FrequencyRange.High:
                    return new float[2] { 4000, 6000 };
                case FrequencyRange.VeryHigh:
                    return new float[2] { 6000, 20000 };
                case FrequencyRange.Decibel:
                    return new float[2] { 0, 20000 };
                default:
                    Debug.LogError("GetFrequencyRange: value out of range");
                    return new float[2] { float.NaN, float.NaN };
            }
        }

        public void SwitchInputDevice(int device)
        {
            string[] audioInput = Input.GetDevices();

            if (audioInput.Length == 0)
            {
                Debug.LogWarning("AudioReactSampler: no input device found");\
                Input.Use = false;
                return;
            }

            if (AudioSource.isPlaying)
            {
                AudioSource.Stop();
            }

            AudioSource.outputAudioMixerGroup = Input.SilentMixer;
            AudioSource.clip = Microphone.Start(audioInput[device], true, 5, (int)AudioReactInput.Frequency);
            AudioSource.Play();
        }

        public string[] GetInputDevices()
        {
            return Input.GetDevices();
        }
    }
}
