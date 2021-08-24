using System.Collections.Generic;
using UnityEngine;

namespace Massitao.AudioManager
{
    [CreateAssetMenu(fileName = "AudioClip Group", menuName = "Audio Manager/AudioClip Group")]
    public class AudioClipContainer : ScriptableObject
    {
        [Header("Group of AudioClips")]
        [SerializeField] private List<AudioClipConfiguration> audioClipContainer = new List<AudioClipConfiguration>();

        [Space(5)]

        [SerializeField] private bool randomRepeatPreviousClip = false;

        private List<AudioClipConfiguration> audioTempSelector = new List<AudioClipConfiguration>();
        private AudioClipConfiguration audioPreviouslyPlayed = null;


        #region Container Methods
        [ContextMenu("Reset Container")]
        private void ResetContainer()
        {
            audioClipContainer.Clear();
            randomRepeatPreviousClip = false;

            audioTempSelector.Clear();
            audioPreviouslyPlayed = null;
        }

        public AudioClipConfiguration SelectAudioClipFromGroup()
        {
            if (audioClipContainer.Count == 0)
            {
                return null;
            }

            if (audioClipContainer.Count == 1)
            {
                return audioClipContainer[0];
            }
            else
            {
                return SelectRandomAudioClipFromGroup();
            }
        }
        private AudioClipConfiguration SelectRandomAudioClipFromGroup()
        {
            audioTempSelector.Clear();
            audioTempSelector.AddRange(audioClipContainer);

            if (audioPreviouslyPlayed != null && !randomRepeatPreviousClip)
            {
                audioTempSelector.Remove(audioPreviouslyPlayed);
            }

            audioPreviouslyPlayed = audioTempSelector[Random.Range(0, audioTempSelector.Count)];
            return audioPreviouslyPlayed;
        }
        #endregion
    }
}