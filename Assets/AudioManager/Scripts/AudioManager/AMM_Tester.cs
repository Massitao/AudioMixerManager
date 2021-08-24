using UnityEngine;
using Massitao.AudioManager;

public class AMM_Tester : MonoBehaviour
{
    [Header("Music Tracks")]
    public AudioClipConfiguration introMusic;
    public AudioClipConfiguration nextTrackMusic;

    [Header("Mixer Group Changes")]
    public MixerExposedParametersType parameterToModify;
    public float newParameterValue;


    // Start is called before the first frame update
    void Start()
    {
        AudioManager.Instance?.PlayMusic(introMusic, () => AudioManager.Instance?.PlayMusic(nextTrackMusic, true));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.Instance?.SetExposedParameterValue(parameterToModify, newParameterValue);
        }
    }
}
