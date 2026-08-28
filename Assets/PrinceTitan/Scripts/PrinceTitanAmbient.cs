using UnityEngine;

namespace PrinceTitan
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class PrinceTitanAmbient : MonoBehaviour
    {
        private AudioSource source;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.volume = .12f;
            source.clip = BuildRoomTone();
        }

        private void Start()
        {
            if (PlayerPrefs.GetInt("PrinceTitan.Ambience", 1) == 1) source.Play();
        }

        public bool IsEnabled
        {
            get { return source != null && source.isPlaying; }
        }

        public void Toggle()
        {
            if (source.isPlaying)
            {
                source.Pause();
                PlayerPrefs.SetInt("PrinceTitan.Ambience", 0);
            }
            else
            {
                source.Play();
                PlayerPrefs.SetInt("PrinceTitan.Ambience", 1);
            }
        }

        private static AudioClip BuildRoomTone()
        {
            const int rate = 22050;
            const int seconds = 8;
            var frames = rate * seconds;
            var data = new float[frames * 2];
            var seed = 0x31415926u;
            var brown = 0f;
            for (var i = 0; i < frames; i++)
            {
                seed ^= seed << 13; seed ^= seed >> 17; seed ^= seed << 5;
                var white = ((seed & 0xffff) / 32767.5f) - 1f;
                brown = Mathf.Clamp(brown * .995f + white * .005f, -.25f, .25f);
                var t = (float)i / rate;
                var engines = Mathf.Sin(t * Mathf.PI * 2f * 55f) * .10f + Mathf.Sin(t * Mathf.PI * 2f * 82.5f) * .035f;
                var bellEnvelope = Mathf.Pow(Mathf.Max(0f, 1f - Mathf.Repeat(t, 8f) * 1.6f), 5f);
                var bell = Mathf.Sin(t * Mathf.PI * 2f * 440f) * bellEnvelope * .055f;
                var sample = Mathf.Clamp(engines + brown * .35f + bell, -.30f, .30f);
                data[i * 2] = sample;
                data[i * 2 + 1] = sample * .96f;
            }
            var clip = AudioClip.Create("Sala de Comando — ambiente", frames, 2, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
