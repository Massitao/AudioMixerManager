namespace Massitao.AudioManager
{
    /// <summary> 
    /// Defines the Audio Mixer Groups or sound types within the game.
    /// Needed for the AudioClipConfiguration and AudioManager scripts.
    /// </summary>

    /// Include every Audio Mixer Group your game will use.
    /// Separate your Mixer Groups into 3 Enum Items types:
    /// 1. MASTER:    Controls every group of an Audio Mixer and should be controlled by Game Logic.
    /// 2. "..."Master: Manages nested Mixer Groups refer and should controlled ONLY by Game Logic.
    /// 3. "...":       Every other group.
    /// 
    /// "..." = Name given by user.
    /// 
    /// For example:
    /// MASTER      -> Game Controlled Setting | Global Audio Group and used for Fadings and Effects
    /// MusicMaster -> Game Controlled Setting | Used for Fadings and Effects
    /// Music       -> User Controlled Setting | Used to allow User to modify in-game volume

    /// To easily identify which Mixer Group belong to an Audio Mixer, you could:
    /// 1. Leave comments as "Headers" with the Audio Mixer's name.
    /// 2. Add the Audio Mixer's name to the Mixer Group.
    /// 
    /// For example:
    /// 1.  // MasterMixer
    ///     Music,
    ///     Sounds,
    ///     ...
    ///     
    /// 2.  MasterMixer_Music
    ///     MasterMixer_Sounds
    ///           or
    ///     MasterMixerMusic
    ///     MasterMixerSounds
    ///           or    
    ///          ....

    public enum MixerGroupType
    {
        MASTER,

        MusicMaster,
        Music,

        SoundMaster,
        Sounds,
        Ambience,
        Dialogue,
        Effects,
        Footsteps,
        UI
    }
}