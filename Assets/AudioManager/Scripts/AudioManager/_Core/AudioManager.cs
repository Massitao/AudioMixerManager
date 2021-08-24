using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Massitao.Pattern.Singleton;
using Massitao.Pattern.ObjectPool;
using Massitao.TransformFollowerTool;

namespace Massitao.AudioManager
{
    [Serializable] public class Mixer
    {
        public string name;
        public AudioMixer audioMixer;
        public MixerType audioMixerType;

        [Space(10)]

        public List<MixerGroup> audioMixerGroups;

        [Space(10)]

        public List<MixerExposedParameter> audioMixerExposedParameters;
    }  
    [Serializable] public class MixerGroup
    {
        public string name;

        [HideInInspector] public AudioMixer audioMixerOwner;

        public AudioMixerGroup audioGroup;
        public MixerGroupType audioMixerGroupType;
    } 
    [Serializable] public class MixerExposedParameter
    {
        public string name;

        [HideInInspector] public AudioMixer audioMixerOwner;

        public string exposedParameter;
        public MixerExposedParametersType exposedParameterType;
    }

    public class AudioManager : Singleton<AudioManager>
    {
        [Header("Audio Mixers")]
        [SerializeField] private List<Mixer> mixerList = new List<Mixer>();

        private Dictionary<MixerType, Mixer> mixersDictionary;
        private Dictionary<MixerGroupType, MixerGroup> mixerGroupsDictionary;
        private Dictionary<MixerExposedParametersType, MixerExposedParameter> mixerExposedParametersDictionary;

        [Header("Music AudioSource")]
        [SerializeField] private MixerGroupType musicMixerGroup = 0;
        [SerializeField] private AudioSource musicAudioSource;
        private Coroutine musicEndCheckerCoroutine;

        [Header("Audio Pool")]
        [SerializeField] private ObjectPool audioPooler;

        [Header("Fade Coroutines")]
        private Dictionary<MixerExposedParametersType, Coroutine> fadeCoroutinesDictionary;


        #region MonoBehaviour Methods
        protected override void Awake()
        {
            base.Awake();

            InitializeMixerDictionaries();
            GetMusicAudioSource();
            GetAudioPooler();
            SetFadeCoroutines();
        }

        private void InitializeMixerDictionaries()
        {
            mixersDictionary = new Dictionary<MixerType, Mixer>();
            mixerGroupsDictionary = new Dictionary<MixerGroupType, MixerGroup>();
            mixerExposedParametersDictionary = new Dictionary<MixerExposedParametersType, MixerExposedParameter>();

            Mixer mixer;
            MixerType mixerType;
            MixerGroup mixerGroup;
            MixerGroupType MixerGroupType;
            MixerExposedParameter mixerExposedParameter;
            MixerExposedParametersType mixerExposedParametersType;

            // Mixer Loop
            for (int i = 0; i < mixerList.Count; i++)
            {
                mixer = mixerList[i];
                mixerType = mixerList[i].audioMixerType;
                mixersDictionary.Add(mixerType, mixer);

                // Mixer Group Loop
                for (int j = 0; j < mixer.audioMixerGroups.Count; j++)
                {
                    mixerGroup = mixer.audioMixerGroups[j];
                    mixerGroup.audioMixerOwner = mixer.audioMixer;
                    MixerGroupType = mixerGroup.audioMixerGroupType;
                    mixerGroupsDictionary.Add(MixerGroupType, mixer.audioMixerGroups[j]);
                }

                // Mixer Exposed Parameters Type Loop
                for (int k = 0; k < mixer.audioMixerExposedParameters.Count; k++)
                {
                    mixerExposedParameter = mixer.audioMixerExposedParameters[k];
                    mixerExposedParameter.audioMixerOwner = mixer.audioMixer;
                    mixerExposedParametersType = mixer.audioMixerExposedParameters[k].exposedParameterType;
                    mixerExposedParametersDictionary.Add(mixerExposedParametersType, mixerExposedParameter);
                }
            }
        }
        private void GetMusicAudioSource()
        {
            if (musicAudioSource == null)
            {
                if (TryGetComponent(out AudioSource thisMusicAudioSource))
                {
                    musicAudioSource = thisMusicAudioSource;
                }
                else
                {
                    musicAudioSource = gameObject.AddComponent<AudioSource>();
                }
            }

            musicAudioSource.outputAudioMixerGroup = GetAudioMixerGroup(musicMixerGroup);

            if (musicAudioSource.outputAudioMixerGroup == null)
            {
                Debug.LogWarning($"AudioManager doesn't have a Music Mixer Group assigned!", gameObject);
            }
        }
        private void GetAudioPooler()
        {
            if (audioPooler == null)
            {
                if (TryGetComponent(out ObjectPool attachedPool))
                {
                    audioPooler = attachedPool;
                }
                else
                {
                    Debug.LogError($"No Audio Pooler was found! Disabling {gameObject.name}", gameObject);
                    gameObject.SetActive(false);

                    return;
                }
            }

            audioPooler.InitializePool();
        }
        private void SetFadeCoroutines()
        {
            fadeCoroutinesDictionary = new Dictionary<MixerExposedParametersType, Coroutine>();

            foreach (MixerExposedParametersType parameters in System.Enum.GetValues(typeof(MixerExposedParametersType)))
            {
                fadeCoroutinesDictionary.Add(parameters, null);
            }
        }
        #endregion


        #region Play Music Methods
        public void PlayMusic(AudioClipContainer audioClip, bool loop)
        {
            PlayMusic(audioClip.SelectAudioClipFromGroup(), loop);
        }
        public void PlayMusic(AudioClipContainer audioClip, Action OnMusicEnd)
        {
            PlayMusic(audioClip.SelectAudioClipFromGroup(), OnMusicEnd);
        }

        public void PlayMusic(AudioClipConfiguration audioClip, bool loop)
        {
            StopMusic();

            musicAudioSource.clip = audioClip.GetClip();
            musicAudioSource.loop = loop;

            musicAudioSource.Play();
        }
        public void PlayMusic(AudioClipConfiguration audioClip, Action OnMusicEnd)
        {
            StopMusic();

            musicAudioSource.clip = audioClip.GetClip();
            musicAudioSource.loop = false;

            musicAudioSource.Play();

            if (OnMusicEnd != null)
            {
                musicEndCheckerCoroutine = StartCoroutine(MusicEndChecker(OnMusicEnd));
            }
            else
            {
                Debug.LogWarning($"Delegate is empty! Nothing will happen after playing the song {audioClip.GetClip()}!", gameObject);
            }
        }

        public void PlayMusic(AudioClip audioClip, bool loop)
        {
            StopMusic();

            musicAudioSource.clip = audioClip;
            musicAudioSource.loop = loop;

            musicAudioSource.Play();
        }
        public void PlayMusic(AudioClip audioClip, Action OnMusicEnd)
        {
            StopMusic();

            musicAudioSource.clip = audioClip;
            musicAudioSource.loop = false;

            musicAudioSource.Play();

            if (OnMusicEnd != null)
            {
                musicEndCheckerCoroutine = StartCoroutine(MusicEndChecker(OnMusicEnd));
            }
            else
            {
                Debug.LogWarning($"Delegate is empty! Nothing will happen after playing the song {audioClip}!", gameObject);
            }
        }

        public void StopMusic()
        {
            musicAudioSource.Stop();
            StopMusicEndChecker();
        }

        private IEnumerator MusicEndChecker(Action OnMusicEnd)
        {
            bool started = false;

            while ((!started || !(musicAudioSource.time <= 0f && started)) && musicAudioSource.clip != null)
            {
                if (!started)
                {
                    started = musicAudioSource.time > 0f;
                }

                yield return null;
            }

            StopMusicEndChecker();
            OnMusicEnd?.Invoke();

            yield break;
        }
        private void StopMusicEndChecker()
        {
            if (musicEndCheckerCoroutine != null)
            {
                StopCoroutine(musicEndCheckerCoroutine);
                musicEndCheckerCoroutine = null;
            }
        }
        #endregion

        #region Play Sound Methods
        #region Play Clip from AudioClip Container
        public void PlaySound(AudioClipContainer audioClipContainer)
        {
            PlaySound(audioClipContainer.SelectAudioClipFromGroup());
        }
        public void PlaySound(AudioClipContainer audioClipContainer, Transform audioSourceParent)
        {
            PlaySound(audioClipContainer.SelectAudioClipFromGroup(), audioSourceParent);
        }
        public void PlaySound(AudioClipContainer audioClipContainer, Vector3 audioSourcePosition)
        {
            PlaySound(audioClipContainer.SelectAudioClipFromGroup(), audioSourcePosition);
        }
        public void PlaySound(AudioClipContainer audioClipContainer, Transform audioSourceParent, Vector3 audioSourcePosition)
        {
            PlaySound(audioClipContainer.SelectAudioClipFromGroup(), audioSourceParent, audioSourcePosition);
        }
        #endregion

        #region Play Clip
        public void PlaySound(AudioClipConfiguration audioClip)
        {
            if (PlaySound_Checker(audioClip, out AudioPoolObject pooledAudio))
            {
                PlaySound_InitializeAudioSource(pooledAudio);
            }
        }
        public void PlaySound(AudioClipConfiguration audioClip, Transform audioSourceParent)
        {
            if (PlaySound_Checker(audioClip, out AudioPoolObject pooledAudio))
            {
                if (pooledAudio.TryGetComponent(out TransformFollower transformFollower))
                {
                    transformFollower.StartFollow(audioSourceParent);
                }
                else
                {
                    pooledAudio.gameObject.AddComponent<TransformFollower>().StartFollow(audioSourceParent);
                }

                PlaySound_InitializeAudioSource(pooledAudio);
            }
        }
        public void PlaySound(AudioClipConfiguration audioClip, Vector3 audioSourcePosition)
        {
            if (PlaySound_Checker(audioClip, out AudioPoolObject pooledAudio))
            {
                pooledAudio.GetAudioSource().transform.position = audioSourcePosition;
                PlaySound_InitializeAudioSource(pooledAudio);
            }
        }
        public void PlaySound(AudioClipConfiguration audioClip, Transform audioSourceParent, Vector3 audioSourcePosition)
        {
            if (PlaySound_Checker(audioClip, out AudioPoolObject pooledAudio))
            {
                if (pooledAudio.TryGetComponent(out TransformFollower transformFollower))
                {
                    transformFollower.StartFollow(audioSourceParent, audioSourcePosition);
                }
                else
                {
                    pooledAudio.gameObject.AddComponent<TransformFollower>().StartFollow(audioSourceParent, audioSourcePosition);
                }

                PlaySound_InitializeAudioSource(pooledAudio);
            }
        }
        #endregion

        #region Play Sound Checkers & Configuration
        private bool PlaySound_Checker(AudioClipConfiguration audioClip, out AudioPoolObject pooledAudio)
        {
            pooledAudio = null;

            if (PlaySound_HasClip(audioClip))
            {
                pooledAudio = PlaySound_HasAudioObject();
                if (pooledAudio != null)
                {
                    if (PlaySound_SetAudioSourceConfiguration(pooledAudio.GetAudioSource(), audioClip))
                    {
                        return true;
                    }
                    else
                    {
                        pooledAudio.ForceReturnToPool();
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        private bool PlaySound_HasClip(AudioClipConfiguration audioClip)
        {
            if (audioClip.GetClip() == null)
            {
                Debug.LogWarning($"No AudioClip received!", audioClip);
                return false;
            }

            return true;
        }
        private AudioPoolObject PlaySound_HasAudioObject()
        {
            if (audioPooler.GetObjectFromPool().TryGetComponent(out AudioPoolObject pooledAudio))
            {
                return pooledAudio;
            }
            else
            {
                Debug.Log($"No Audio Object received! Is Object Pool set to not grow?", audioPooler);
                return null;
            }
        }

        private bool PlaySound_SetAudioSourceConfiguration(AudioSource audiosource, AudioClipConfiguration clipConfiguration)
        {
            AudioSourceConfiguration asConfig = clipConfiguration.GetAudioSourceConfiguration();

            #region Audio Mixer Parameters
            audiosource.outputAudioMixerGroup = GetAudioMixerGroup(clipConfiguration.GetMixerGroupType());
            if (audiosource.outputAudioMixerGroup == null)
            {
                Debug.LogWarning($"No Output Audio Mixer Group found!", clipConfiguration);
                return false;
            }

            audiosource.bypassEffects           = asConfig.GetBypassEffects();
            audiosource.bypassListenerEffects   = asConfig.GetBypassListenerEffects();
            audiosource.bypassReverbZones       = asConfig.GetBypassReverbZones();
            #endregion

            #region AudioSource Parameters
            audiosource.clip            = clipConfiguration.GetClip();

            audiosource.loop            = false;

            audiosource.priority        = asConfig.GetPriority();
            audiosource.volume          = asConfig.GetVolume();
            audiosource.pitch           = asConfig.GetPitch();
            audiosource.panStereo       = asConfig.GetStereoPan();
            audiosource.spatialBlend    = asConfig.GetSpatialBlend();
            audiosource.reverbZoneMix   = asConfig.GetReverbMix();

            #region 3D Source Parameters
            audiosource.dopplerLevel    = asConfig.GetDopplerLevel();
            audiosource.spread          = asConfig.GetSpread();
            audiosource.rolloffMode     = asConfig.GetVolumeRolloff();
            audiosource.minDistance     = asConfig.GetMinimumDistance();
            audiosource.maxDistance     = asConfig.GetMaximumDistance();

            if (!asConfig.SetAudioSourceCurves(audiosource)) Debug.LogError("No Volume Rolloff Custom Curve detected! Changing Mode to Logarithmic.", clipConfiguration);
            #endregion
            #endregion

            audiosource.gameObject.name = $"{clipConfiguration.GetMixerGroupType()}: {clipConfiguration.GetClip().name} AudioSource";

            return true;
        }
        private void PlaySound_InitializeAudioSource(AudioPoolObject audioSourceToPlay)
        {
            audioSourceToPlay.GetAudioSource().Play();
            audioSourceToPlay.DisableAudioOnComplete();
        }
        #endregion
        #endregion

        #region Duplicate Audio Clip Configuration Methods
        public AudioClipConfiguration DuplicateAudioClipConfiguration(AudioClipContainer configToDupe)
        {
            if (configToDupe == null)
            {
                Debug.LogError($"No Audio Clip Configuration to duplicate received!");
                return null;
            }

            return Instantiate(configToDupe.SelectAudioClipFromGroup());
        }
        public AudioClipConfiguration DuplicateAudioClipConfiguration(AudioClipConfiguration configToDupe)
        {
            if (configToDupe == null)
            {
                Debug.LogError($"No Audio Clip Configuration to duplicate received!");
                return null;
            }

            return Instantiate(configToDupe);
        }
        #endregion

        #region Audio Mixer Methods
        #region Get Audio Mixer
        private AudioMixer GetAudioMixer(MixerType mixerType)
        {
            mixersDictionary.TryGetValue(mixerType, out Mixer mixer);

            if (mixer == null)
            {
                Debug.LogError($"No Mixer was assigned to {mixerType} Mixer Type!");
                return null;
            }
            if (mixer.audioMixer == null)
            {
                Debug.LogError($"No Audio Mixer found in {mixer.name}!");
                return null;
            }

            return mixer.audioMixer;
        }
        private AudioMixer GetAudioMixer(MixerGroupType groupType)
        {
            AudioMixerGroup mixerGroup = GetAudioMixerGroup(groupType);

            if (mixerGroup != null)
            {
                return mixerGroup.audioMixer;
            }

            return null;
        }
        private AudioMixer GetAudioMixer(MixerExposedParametersType parameterType)
        {
            MixerExposedParameter parameter = GetMixerExposedParameter(parameterType);

            if (parameter == null)
            {
                return null;
            }

            return GetAudioMixer(parameter);
        }
        private AudioMixer GetAudioMixer(MixerExposedParameter parameter)
        {
            if (parameter.audioMixerOwner == null)
            {
                Debug.LogError($"Parameter {parameter.name} has no Mixer owner! Something went wrong!");
                return null;
            }

            return parameter.audioMixerOwner;
        }
        #endregion

        #region Get Audio Mixer Group
        private AudioMixerGroup GetAudioMixerGroup(MixerGroupType groupType)
        {
            mixerGroupsDictionary.TryGetValue(groupType, out MixerGroup mixerGroup);

            if (mixerGroup == null)
            {
                Debug.LogError($"No Mixer Group was assigned to {groupType} Group Type!");
                return null;
            }
            if (mixerGroup.audioGroup == null)
            {
                Debug.LogError($"No Output Audio Mixer Group found in {mixerGroup.name}!");
                return null;
            }

            return mixerGroup.audioGroup;
        }
        #endregion

        #region Get Mixer Exposed Parameter
        private MixerExposedParameter GetMixerExposedParameter(MixerExposedParametersType parameterType)
        {
            mixerExposedParametersDictionary.TryGetValue(parameterType, out MixerExposedParameter parameter);

            if (parameter == null)
            {
                Debug.LogError($"No Mixer Exposed Parameter was assigned to {parameterType} Mixer Parameter Type!");
                return null;
            }
            if (parameter.exposedParameter == null)
            {
                Debug.LogError($"No Exposed Parameter found in {parameter.name}!");
                return null;
            }

            return parameter;
        }
        private string GetMixerExposedParameterString(MixerExposedParametersType parameterType)
        {
            MixerExposedParameter parameter = GetMixerExposedParameter(parameterType);

            if (parameter == null)
            {
                return string.Empty;
            }

            return parameter.exposedParameter;
        }
        #endregion

        #region Get / Set Exposed Parameter Value
        public bool GetExposedParameterValue(MixerExposedParametersType parameterType, out float valueToGet)
        {
            valueToGet = 0f;

            MixerExposedParameter parameter = GetMixerExposedParameter(parameterType);
            if (parameter == null) return false;

            return GetExposedParameterValue(parameter.audioMixerOwner, parameter.exposedParameter, out valueToGet);
        }
        public bool GetExposedParameterValue(MixerExposedParametersType parameterType, out float valueToGet, out AudioMixer mixer, out string parameterString)
        {
            valueToGet = 0f;
            mixer = null;
            parameterString = string.Empty;

            MixerExposedParameter parameter = GetMixerExposedParameter(parameterType);
            if (parameter == null) return false;

            mixer = parameter.audioMixerOwner;
            parameterString = parameter.exposedParameter;

            return GetExposedParameterValue(parameter.audioMixerOwner, parameter.exposedParameter, out valueToGet);
        }
        private bool GetExposedParameterValue(AudioMixer mixer, string parameter, out float valueToGet)
        {
            valueToGet = 0f;

            return mixer.GetFloat(parameter, out valueToGet);
        }

        public bool SetExposedParameterValue(MixerExposedParametersType parameterType, float valueToSet)
        {
            MixerExposedParameter parameter = GetMixerExposedParameter(parameterType);
            if (parameter == null) return false;
          
            return SetExposedParameterValue(parameter.audioMixerOwner, parameter.exposedParameter, valueToSet);
        }
        private bool SetExposedParameterValue(AudioMixer mixer, string parameter, float valueToSet)
        {           
            return mixer.SetFloat(parameter, valueToSet);
        }
        #endregion
        #endregion

        #region Fade Methods
        #region Fade Start
        public void FadeExposedParameter(MixerExposedParametersType parameterToFade, bool abort, float valueToSet, float timeToFade, float delayBeforeFade)
        {
            if (CanFadeCheck(parameterToFade, abort))
            {
                if (fadeCoroutinesDictionary[parameterToFade] != null)
                {
                    FadeStop(parameterToFade);
                }

                fadeCoroutinesDictionary[parameterToFade] = StartCoroutine(LinearFadeBehaviour(parameterToFade, valueToSet, timeToFade, delayBeforeFade));
            }
        }
        public void FadeExposedParameter(MixerExposedParametersType parameterToFade, bool abort, float valueToSet, float timeToFade, float delayBeforeFade, Action OnFadeEnd)
        {
            if (CanFadeCheck(parameterToFade, abort))
            {
                if (fadeCoroutinesDictionary[parameterToFade] != null)
                {
                    FadeStop(parameterToFade);
                }

                fadeCoroutinesDictionary[parameterToFade] = StartCoroutine(LinearFadeBehaviour(parameterToFade, valueToSet, timeToFade, delayBeforeFade, OnFadeEnd));
            }
        }

        public void FadeExposedParameter(MixerExposedParametersType parameterToFade, bool abort, float valueToSet, float timeToFade, float delayBeforeFade, AnimationCurve customEasing)
        {
            if (CanFadeCheck(parameterToFade, abort))
            {
                if (fadeCoroutinesDictionary[parameterToFade] != null)
                {
                    FadeStop(parameterToFade);
                }

                fadeCoroutinesDictionary[parameterToFade] = StartCoroutine(CustomCurveFadeBehaviour(parameterToFade, valueToSet, timeToFade, delayBeforeFade, customEasing));
            }
        }
        public void FadeExposedParameter(MixerExposedParametersType parameterToFade, bool abort, float valueToSet, float timeToFade, float delayBeforeFade, AnimationCurve customEasing, Action OnFadeEnd)
        {
            if (CanFadeCheck(parameterToFade, abort))
            {
                if (fadeCoroutinesDictionary[parameterToFade] != null)
                {
                    FadeStop(parameterToFade);
                }

                fadeCoroutinesDictionary[parameterToFade] = StartCoroutine(CustomCurveFadeBehaviour(parameterToFade, valueToSet, timeToFade, delayBeforeFade, customEasing, OnFadeEnd));
            }
        }

        public void FadeExposedParameter(MixerExposedParametersType parameterToFade, bool abort, float valueToSet, float timeToFade, float delayBeforeFade, Func<float, float> customValueReturnMethod)
        {
            if (CanFadeCheck(parameterToFade, abort))
            {
                if (fadeCoroutinesDictionary[parameterToFade] != null)
                {
                    FadeStop(parameterToFade);
                }

                if (customValueReturnMethod == null)
                {
                    Debug.LogError($"Delegate is Empty! Can't fade {parameterToFade.ToString()}.", gameObject);
                    return;
                }

                fadeCoroutinesDictionary[parameterToFade] = StartCoroutine(CustomFormulaFadeBehaviour(parameterToFade, valueToSet, timeToFade, delayBeforeFade, customValueReturnMethod));
            }
        }
        public void FadeExposedParameter(MixerExposedParametersType parameterToFade, bool abort, float valueToSet, float timeToFade, float delayBeforeFade, Func<float, float> customValueReturnMethod, Action OnFadeEnd)
        {
            if (CanFadeCheck(parameterToFade, abort))
            {
                if (fadeCoroutinesDictionary[parameterToFade] != null)
                {
                    FadeStop(parameterToFade);
                }

                if (customValueReturnMethod == null)
                {
                    Debug.LogError($"Delegate is Empty! Can't fade {parameterToFade.ToString()}.", gameObject);
                    return;
                }

                fadeCoroutinesDictionary[parameterToFade] = StartCoroutine(CustomFormulaFadeBehaviour(parameterToFade, valueToSet, timeToFade, delayBeforeFade, customValueReturnMethod, OnFadeEnd));
            }
        }
        #endregion

        #region Fade Coroutines
        private IEnumerator LinearFadeBehaviour(MixerExposedParametersType parameterToFade, float valueToSet, float timeToFade, float delayBeforeFade)
        {
            if (GetExposedParameterValue(parameterToFade, out float valueToGet, out AudioMixer mixer, out string parameterString))
            {
                if (delayBeforeFade > Mathf.Epsilon)
                {
                    yield return new WaitForSeconds(delayBeforeFade);
                }

                float initialTime = Time.time;
                float lerpValue = valueToGet;
                float lerp = 0f;

                while (lerp < 1f)
                {
                    lerp = Mathf.Clamp01(Mathf.InverseLerp(initialTime, initialTime + timeToFade, Time.time));
                    lerpValue = Mathf.Lerp(valueToGet, valueToSet, lerp);
                    SetExposedParameterValue(mixer, parameterString, lerpValue);

                    yield return null;
                }

                lerpValue = Mathf.Lerp(valueToGet, valueToSet, 1f);
                SetExposedParameterValue(mixer, parameterString, lerpValue);
            }

            FadeStop(parameterToFade);

            yield break;
        }
        private IEnumerator LinearFadeBehaviour(MixerExposedParametersType parameterToFade, float valueToSet, float timeToFade, float delayBeforeFade, Action OnFadeEnd)
        {
            if (GetExposedParameterValue(parameterToFade, out float valueToGet, out AudioMixer mixer, out string parameterString))
            {
                if (delayBeforeFade > Mathf.Epsilon)
                {
                    yield return new WaitForSeconds(delayBeforeFade);
                }

                float initialTime = Time.time;
                float lerpValue = valueToGet;
                float lerp = 0f;

                while (lerp < 1f)
                {
                    lerp = Mathf.Clamp01(Mathf.InverseLerp(initialTime, initialTime + timeToFade, Time.time));
                    lerpValue = Mathf.Lerp(valueToGet, valueToSet, lerp);
                    SetExposedParameterValue(mixer, parameterString, lerpValue);

                    yield return null;
                }

                lerpValue = Mathf.Lerp(valueToGet, valueToSet, 1f);
                SetExposedParameterValue(mixer, parameterString, lerpValue);
            }

            FadeStop(parameterToFade);
            OnFadeEnd?.Invoke();

            yield break;
        }

        private IEnumerator CustomCurveFadeBehaviour(MixerExposedParametersType parameterToFade, float valueToSet, float timeToFade, float delayBeforeFade, AnimationCurve customEasing)
        {
            if (GetExposedParameterValue(parameterToFade, out float valueToGet, out AudioMixer mixer, out string parameterString))
            {
                if (delayBeforeFade > Mathf.Epsilon)
                {
                    yield return new WaitForSeconds(delayBeforeFade);
                }

                float initialTime = Time.time;
                float lerpValue = valueToGet;
                float lerp = 0f;

                while (lerp < 1f)
                {
                    lerp = Mathf.Clamp01(Mathf.InverseLerp(initialTime, initialTime + timeToFade, Time.time));
                    lerpValue = Mathf.Lerp(valueToGet, valueToSet, customEasing.Evaluate(lerp));
                    SetExposedParameterValue(mixer, parameterString, lerpValue);

                    yield return null;
                }

                lerpValue = Mathf.Lerp(valueToGet, valueToSet, customEasing.Evaluate(1f));
                SetExposedParameterValue(mixer, parameterString, lerpValue);
            }

            FadeStop(parameterToFade);

            yield break;
        }
        private IEnumerator CustomCurveFadeBehaviour(MixerExposedParametersType parameterToFade, float valueToSet, float timeToFade, float delayBeforeFade, AnimationCurve customEasing, Action OnFadeEnd)
        {
            if (GetExposedParameterValue(parameterToFade, out float valueToGet, out AudioMixer mixer, out string parameterString))
            {
                if (delayBeforeFade > Mathf.Epsilon)
                {
                    yield return new WaitForSeconds(delayBeforeFade);
                }

                float initialTime = Time.time;
                float lerpValue = valueToGet;
                float lerp = 0f;

                while (lerp < 1f)
                {
                    lerp = Mathf.Clamp01(Mathf.InverseLerp(initialTime, initialTime + timeToFade, Time.time));
                    lerpValue = Mathf.Lerp(valueToGet, valueToSet, customEasing.Evaluate(lerp));
                    SetExposedParameterValue(mixer, parameterString, lerpValue);

                    yield return null;
                }

                lerpValue = Mathf.Lerp(valueToGet, valueToSet, customEasing.Evaluate(1f));
                SetExposedParameterValue(mixer, parameterString, lerpValue);
            }

            FadeStop(parameterToFade);
            OnFadeEnd?.Invoke();

            yield break;
        }

        private IEnumerator CustomFormulaFadeBehaviour(MixerExposedParametersType parameterToFade, float valueToSet, float timeToFade, float delayBeforeFade, Func<float, float> customValueReturnMethod)
        {
            if (GetExposedParameterValue(parameterToFade, out float valueToGet, out AudioMixer mixer, out string parameterString))
            {
                if (delayBeforeFade > Mathf.Epsilon)
                {
                    yield return new WaitForSeconds(delayBeforeFade);
                }

                float initialTime = Time.time;
                float lerpValue = valueToGet;
                float lerp = 0f;

                while (lerp < 1f)
                {
                    lerp = Mathf.Clamp01(Mathf.InverseLerp(initialTime, initialTime + timeToFade, Time.time));
                    lerpValue = customValueReturnMethod.Invoke(lerp);
                    SetExposedParameterValue(mixer, parameterString, lerpValue);

                    yield return null;
                }

                lerpValue = customValueReturnMethod.Invoke(1f);
                SetExposedParameterValue(mixer, parameterString, lerpValue);
            }

            FadeStop(parameterToFade);

            yield break;
        }
        private IEnumerator CustomFormulaFadeBehaviour(MixerExposedParametersType parameterToFade, float valueToSet, float timeToFade, float delayBeforeFade, Func<float, float> customValueReturnMethod, Action OnFadeEnd)
        {
            if (GetExposedParameterValue(parameterToFade, out float valueToGet, out AudioMixer mixer, out string parameterString))
            {
                if (delayBeforeFade > Mathf.Epsilon)
                {
                    yield return new WaitForSeconds(delayBeforeFade);
                }

                float initialTime = Time.time;
                float lerpValue = valueToGet;
                float lerp = 0f;

                while (lerp < 1f)
                {
                    lerp = Mathf.Clamp01(Mathf.InverseLerp(initialTime, initialTime + timeToFade, Time.time));
                    lerpValue = customValueReturnMethod.Invoke(lerp);
                    SetExposedParameterValue(mixer, parameterString, lerpValue);

                    yield return null;
                }

                lerpValue = customValueReturnMethod.Invoke(1f);
                SetExposedParameterValue(mixer, parameterString, lerpValue);
            }

            FadeStop(parameterToFade);
            OnFadeEnd?.Invoke();

            yield break;
        }
        #endregion

        private void FadeStop(MixerExposedParametersType parameterToFade)
        {
            if (fadeCoroutinesDictionary[parameterToFade] != null)
            {
                StopCoroutine(fadeCoroutinesDictionary[parameterToFade]);
                fadeCoroutinesDictionary[parameterToFade] = null;
            }
        }
        private bool CanFadeCheck(MixerExposedParametersType parameterToFade, bool abort)
        {
            if (fadeCoroutinesDictionary.ContainsKey(parameterToFade))
            {
                if (fadeCoroutinesDictionary[parameterToFade] == null)
                {
                    return true;
                }
                else
                {
                    return abort;
                }
            }
            else
            {
                Debug.LogError($"Audio Manager doesn't have this key registered: {parameterToFade.ToString()}. Can't proceed with Fade.", gameObject);
                return false;
            }
        }
        #endregion
    }
}