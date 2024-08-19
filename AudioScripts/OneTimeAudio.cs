using UnityEngine;

namespace HideAndSeek.AudioScripts
{
    public class OneTimeAudio : MonoBehaviour
    {
        public void PlayAudioClip(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3 position = new(), float spatialBend = 0, float minDistance = 1, float maxDistance = 500)
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();

            source.pitch = pitch;
            source.volume = volume;
            source.spatialBlend = spatialBend;
            transform.position = position;
            source.minDistance = minDistance;
            source.maxDistance = maxDistance;

            source.clip = clip;
            source.Play();

            Invoke(nameof(ClipDone), (clip.length/source.pitch)+1);
        }

        void ClipDone()
        {
            Destroy(gameObject);
        }
    }
}
