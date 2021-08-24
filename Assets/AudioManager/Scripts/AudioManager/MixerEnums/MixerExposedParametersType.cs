namespace Massitao.AudioManager
{
    /// <summary> 
    /// Lists all Audio Mixers' Exposed Parameters.
    /// Needed for the AudioManager and other minor scripts.
    /// </summary>

    /// Include every Exposed Parameters of every Audio Mixer your game will use.
    /// Separate the different Exposed Parameters of each Audio Mixer with spaces.
    /// 
    /// To easily identify which Exposed Parameters belong to an Audio Mixer, you could:
    /// 1. Leave comments as "Headers" with the Audio Mixer's name.
    /// 2. Add the Audio Mixer's name to the Exposed Parameter.
    /// 
    /// For example:
    /// 1.  // MasterMixer
    ///     EP1,
    ///     EP2,
    ///     ...
    ///     
    /// 2.  MasterMixer_EP1
    ///     MasterMixer_EP2
    ///           or
    ///     MasterMixerEP1
    ///     MasterMixerEP2
    ///           or    
    ///          ....

    public enum MixerExposedParametersType
    {
        // MASTER
        MasterMixer_MasterPitch,
        MasterMixer_MasterVolume,


        // MUSIC MASTER
        MasterMixer_MusicMasterPitch,
        MasterMixer_MusicMasterVolume,

        // MUSIC (USER)
        MasterMixer_MusicUserVolume,


        // SOUNDS MASTER
        MasterMixer_SoundMasterPitch,
        MasterMixer_SoundMasterVolume,

        // SOUNDS (USER)
        MasterMixer_SoundUserVolume,

        // SOUNDS CATEGORIES
        MasterMixer_SoundAmbienceVolume,
        MasterMixer_SoundDialogueVolume,
        MasterMixer_SoundEffectsVolume,
        MasterMixer_SoundFootstepsVolume,
        MasterMixer_SoundUIVolume
    }
}