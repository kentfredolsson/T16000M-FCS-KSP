using System.Collections.Generic;
using UnityEngine;

namespace KSPAlert
{
    public class AlertAudio : MonoBehaviour
    {
        private AudioSource audioSource;
        private Dictionary<AlertPriority, AudioClip> toneClips;

        private float lastPlayTime;
        private const float MIN_PLAY_INTERVAL = 0.5f;

        void Start()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f; // 2D sound

            GenerateTones();

            Debug.Log("[KSP-Alert] Audio system initialized");
        }

        private void GenerateTones()
        {
            toneClips = new Dictionary<AlertPriority, AudioClip>();

            // Warning: Urgent dual-tone (like GPWS)
            toneClips[AlertPriority.Warning] = GenerateWarningTone();

            // Caution: Single attention tone
            toneClips[AlertPriority.Caution] = GenerateCautionTone();

            // Advisory: Soft chime
            toneClips[AlertPriority.Advisory] = GenerateAdvisoryTone();
        }

        private AudioClip GenerateWarningTone()
        {
            // Fire bell alarm: rapid ding-ding-ding-ding pattern
            int sampleRate = 44100;
            float duration = 2.0f;  // Longer duration for multiple dings
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            // Bell parameters
            float bellFreq = 2400f;      // High pitched bell
            float bellFreq2 = 3000f;     // Second harmonic
            float bellFreq3 = 3600f;     // Third harmonic
            float dingInterval = 0.12f;  // Time between each ding (fast!)
            float dingDuration = 0.1f;   // Each ding length

            int numDings = (int)(duration / dingInterval);

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                data[i] = 0f;

                // Generate each ding
                for (int d = 0; d < numDings; d++)
                {
                    float dingStart = d * dingInterval;
                    float dingTime = t - dingStart;

                    if (dingTime >= 0 && dingTime < dingDuration)
                    {
                        // Sharp attack, fast decay (bell-like)
                        float attack = Mathf.Min(1f, dingTime / 0.002f);  // 2ms attack
                        float decay = Mathf.Exp(-dingTime * 40f);         // Fast decay
                        float envelope = attack * decay;

                        // Bell sound with harmonics
                        float bell = Mathf.Sin(2 * Mathf.PI * bellFreq * dingTime) * 0.5f;
                        bell += Mathf.Sin(2 * Mathf.PI * bellFreq2 * dingTime) * 0.3f;
                        bell += Mathf.Sin(2 * Mathf.PI * bellFreq3 * dingTime) * 0.15f;

                        // Add metallic shimmer
                        bell += Mathf.Sin(2 * Mathf.PI * (bellFreq * 2.4f) * dingTime) * 0.1f * envelope;

                        data[i] += bell * envelope * 0.6f;
                    }
                }

                // Clamp to prevent clipping
                data[i] = Mathf.Clamp(data[i], -1f, 1f);
            }

            AudioClip clip = AudioClip.Create("WarningBell", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateCautionTone()
        {
            int sampleRate = 44100;
            float duration = 0.4f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            float freq = 880f; // A5

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;

                // Bell-like envelope
                float envelope = Mathf.Exp(-t * 5f);
                if (t < 0.01f) envelope *= t / 0.01f;

                // Main tone with harmonics
                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.5f * envelope;
                data[i] += Mathf.Sin(2 * Mathf.PI * freq * 2 * t) * 0.25f * envelope;
                data[i] += Mathf.Sin(2 * Mathf.PI * freq * 3 * t) * 0.1f * envelope;
            }

            AudioClip clip = AudioClip.Create("CautionTone", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private AudioClip GenerateAdvisoryTone()
        {
            int sampleRate = 44100;
            float duration = 0.25f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            float freq = 660f; // E5

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;

                // Soft envelope
                float envelope = Mathf.Sin(Mathf.PI * t / duration);

                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.3f * envelope;
            }

            AudioClip clip = AudioClip.Create("AdvisoryTone", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void PlayAlert(Alert alert)
        {
            var config = AlertManager.Instance?.Config;
            if (config == null || !config.AudioEnabled) return;

            // Prevent audio spam
            if (Time.time - lastPlayTime < MIN_PLAY_INTERVAL) return;
            lastPlayTime = Time.time;

            if (toneClips.TryGetValue(alert.Priority, out AudioClip clip))
            {
                audioSource.volume = config.MasterVolume;
                audioSource.PlayOneShot(clip);

                Debug.Log($"[KSP-Alert] Playing {alert.Priority} alert: {alert.Message}");
            }
        }

        public void PlayTone(AlertPriority priority)
        {
            var config = AlertManager.Instance?.Config;
            if (config == null || !config.AudioEnabled) return;

            if (toneClips.TryGetValue(priority, out AudioClip clip))
            {
                audioSource.volume = config.MasterVolume;
                audioSource.PlayOneShot(clip);
            }
        }

        public void StopAll()
        {
            audioSource.Stop();
        }
    }
}
