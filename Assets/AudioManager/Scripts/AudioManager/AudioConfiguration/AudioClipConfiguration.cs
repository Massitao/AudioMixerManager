using UnityEngine;

namespace Massitao.AudioManager
{
    [CreateAssetMenu(fileName = "AudioClip Configuration", menuName = "Audio Manager/AudioClip Configuration")]
    public class AudioClipConfiguration : ScriptableObject
    {
        [Header("Audio Clip Configuration")]
        #region Audio Clip
        [Tooltip("Sets clip to play.")]
        [SerializeField] private AudioClip clip;

        public AudioClip GetClip()
        {
            return clip;
        }
        #endregion

        #region Mixer Group Type
        [Tooltip("Sets clip sound type. Necessary to assign Audio Mixer Group (if it exists).")]
        [SerializeField] private MixerGroupType clipGroupType;

        public MixerGroupType GetMixerGroupType()
        {
            return clipGroupType;
        }
        #endregion

        [Space(5)]

        #region AudioSource Configuration
        [Tooltip("Sets the AudioSource Configuration the clip should have.")]
        [SerializeField] private AudioSourceConfiguration clipAudioSourceConfiguration;

        public AudioSourceConfiguration GetAudioSourceConfiguration()
        {
            return clipAudioSourceConfiguration;
        }
        #endregion


        #region Editor Configuration Methods
        [ContextMenu("Reset Configuration")]
        private void ResetConfiguration()
        {
            clip = null;
            clipGroupType = 0;
            clipAudioSourceConfiguration = null;

            Debug.Log("Audio Curves have been reseted. Actual curves are empty and you are free to start over again. Select another Object and click again on this Object.");
        }

        [ContextMenu("Reset Curves")]
        private void ResetAudioSourceCurves()
        {
            clipAudioSourceConfiguration.ResetAudioSourceCurves();
        }
        #endregion
    }
}