using System.Collections;
using UnityEngine;
using Massitao.Pattern.ObjectPool;

namespace Massitao.AudioManager
{
    public class AudioPoolObject : MonoBehaviour
    {
        [Header("Audio Pool Object")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private ObjectPoolTrack audioPoolTrack;
        private Coroutine soundEndCheckerCoroutine;
        private string initialName;

        #region Awake
        private void Awake()
        {
            SetAudioSource();
            SetInitialName();
        }
        private void SetAudioSource()
        {
            if (audioSource == null)
            {
                if (TryGetComponent(out AudioSource thisAudioSource))
                {
                    audioSource = thisAudioSource;
                }
                else
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
        }
        private void SetInitialName()
        {
            initialName = gameObject.name;
        }
        #endregion

        #region On Enable / Disable
        private void OnDisable()
        {
            AudioDisable();
        }
        private void AudioDisable()
        {
            gameObject.name = initialName;
            audioSource.Stop();
            StopSoundEndChecker();
        }
        #endregion

        public AudioSource GetAudioSource()
        {
            return audioSource;
        }
        private bool SetAudioPoolTrack()
        {
            if (TryGetComponent(out ObjectPoolTrack tracker))
            {
                audioPoolTrack = tracker;
                return true;
            }
            else
            {
                Debug.LogError("No Tracker was found!", gameObject);
                return false;
            }
        }

        public void ForceReturnToPool()
        {
            if (audioPoolTrack == null)
            {
                if (!SetAudioPoolTrack())
                {
                    return;
                }
            }

            audioPoolTrack.ForceReturnToPool();
        }
        public void DisableAudioOnComplete()
        {
            StopSoundEndChecker();

            soundEndCheckerCoroutine = StartCoroutine(SoundEndChecker());
        }

        private IEnumerator SoundEndChecker()
        {
            bool started = false;

            while ((!started || !(audioSource.time <= 0f && started)) && audioSource.clip != null)
            {
                if (!started)
                {
                    started = audioSource.time > 0f;
                }

                yield return null;
            }

            StopSoundEndChecker();
            ForceReturnToPool();

            yield break;
        }
        private void StopSoundEndChecker()
        {
            if (soundEndCheckerCoroutine != null)
            {
                StopCoroutine(soundEndCheckerCoroutine);
                soundEndCheckerCoroutine = null;
            }
        }
    }
}