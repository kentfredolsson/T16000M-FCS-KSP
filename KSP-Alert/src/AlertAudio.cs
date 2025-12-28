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
            int sampleRate = 44100;
            float duration = 0.8f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            // Alternating high-low tone (classic GPWS style)
            float freq1 = 1200f;
            float freq2 = 800f;
            float switchTime = 0.15f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float cyclePos = t % (switchTime * 2);
                float freq = cyclePos < switchTime ? freq1 : freq2;

                // Add slight frequency wobble for urgency
                freq += Mathf.Sin(t * 30f) * 20f;

                float envelope = 1f;
                if (t < 0.02f) envelope = t / 0.02f;
                if (t > duration - 0.05f) envelope = (duration - t) / 0.05f;

                data[i] = Mathf.Sin(2 * Mathf.PI * freq * t) * 0.7f * envelope;

                // Add harmonics for more piercing sound
                data[i] += Mathf.Sin(2 * Mathf.PI * freq * 2 * t) * 0.2f * envelope;
            }

            AudioClip clip = AudioClip.Create("WarningTone", samples, 1, sampleRate, false);
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
