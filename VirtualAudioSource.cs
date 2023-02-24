/************************************************************
 * Created in 2014 by:  LunaArgenteus
 * This software is not free. If you acquired this code without paying for it, please consider supporting me
 * by purchasing it on the Unity Asset Store to help me continue creating awesome stuff!
 * 
 * If you have any questions that are not answered by the readme, you can ask on the official support thread on Unity forums, 
 * forum.unity3d.com/threads/273344/
 * send me a private message, or can email me at LunaArgenteus@gmail.com (Please include the name of this product (Split Screen Audio) in the subject line or I may not respond),
 * but please consult the readme first!
 ************************************************************
 */
using UnityEngine;
using System.Collections;

public abstract class VirtualAudioSource : MonoBehaviour
{
    public AudioSource originalAudioSource;
    public AudioSource mySource;

    public abstract IEnumerator PlayAudioSource(float delay = 0f);

    protected virtual void OnEnable()
    {
        if (mySource != null)
        {
            mySource.enabled = true;
            if (playOnEnable)
            {
                Play();
            }
        }

    }
    protected virtual void OnDisable()
    {
        if (mySource != null)
        {
            mySource.enabled = false;
            Stop();
        }
        isCoroutinePlaying = false;
    }

    public const string PLAY_AUDIO_SOURCE_STRING = "PlayAudioSource";
    //It's a damn shame that Unity doesn't allow you to stop coroutines unless you start them by name . . .
    public void Play()
    {
        pauseAudio = false;
        StopCoroutine(PLAY_AUDIO_SOURCE_STRING);
        StartCoroutine(PLAY_AUDIO_SOURCE_STRING, 0);
    }
    public void Play(MonoBehaviour coroutineObj)
    {
        pauseAudio = false;
        coroutineObj.StopCoroutine(PLAY_AUDIO_SOURCE_STRING);
        coroutineObj.StartCoroutine(PLAY_AUDIO_SOURCE_STRING, 0);
    }

    public void PlayDelayed(float delay)
    {
        pauseAudio = false;
        StopCoroutine(PLAY_AUDIO_SOURCE_STRING);
        StartCoroutine(PLAY_AUDIO_SOURCE_STRING, delay);
    }
    public void PlayDelayed(MonoBehaviour coroutineObj, float delay)
    {
        pauseAudio = false;
        coroutineObj.StopCoroutine(PLAY_AUDIO_SOURCE_STRING);
        coroutineObj.StartCoroutine(PLAY_AUDIO_SOURCE_STRING, delay);
    }


    //having my own boolean allows me to inform players when the coroutine is done, rather than the sound itself (it should always be one frame later since coroutines are executed after updates)
    //it also allows me to set the value on a per VirtualAudioSource level, as opposed to per AudioSource level
    protected bool isCoroutinePlaying = false;
    public bool isPlaying
    {
        get
        {
            return isCoroutinePlaying;//mySource.isPlaying;
        }
    }
    /// <summary>
    /// Use this instead of PlayOnAwake to ensure that the VirtualAudioSource is set up before audio is played
    /// </summary>
    public bool playOnEnable = false;

    /// <summary>
    /// Restarts the coroutine when finished - this is useful when you want a clip to loop but also want to be able to switch between listeners between playes when lockPlayingClipToListener is true. 
    /// It's also helpful for preventing desynchronizations when looping with the multi source method and using doppler, which can change the speed of one of the sounds if only one listener moves
    /// </summary>
    public bool loopCoroutine = false;

    #region consistency methods
    //For ease of use, other AudioSource methods that aren't used / manipulated through the VirtualAudioSource are provided here
    public virtual bool bypassEffects
    {
        get
        {
            return mySource.bypassEffects;
        }
        set
        {
            mySource.bypassEffects = value;
        }
    }
    public virtual AudioClip clip
    {
        get
        {
            return mySource.clip;
        }
        set
        {
            mySource.clip = value;
        }
    }

    /// <summary>
    /// If using PanByListenerIndex as the VirtualAudioSource type, this value is unused. Use the doppler settings of the VirtualAudioSource instead!
    /// </summary>
    public virtual float dopplerLevel
    {
        get
        {
            return mySource.dopplerLevel;
        }
        set
        {
            mySource.dopplerLevel = value;
        }
    }
    public virtual bool ignoreListenerPause
    {
        get
        {
            return mySource.ignoreListenerPause;
        }
        set
        {
            mySource.ignoreListenerPause = value;
        }
    }
    public virtual bool ignoreListenerVolume
    {
        get
        {
            return mySource.ignoreListenerVolume;
        }
        set
        {
            mySource.ignoreListenerVolume = value;
        }
    }
    public virtual bool loop
    {
        get
        {
            return mySource.loop;
        }
        set
        {
            mySource.loop = value;
        }
    }

    /// <summary>
    /// If using PanByListenerIndex as the VirtualAudioSource type, this value is unused. Adjust the rolloff equation instead!
    /// </summary>
    public virtual float maxDistance
    {
        get
        {
            return mySource.maxDistance;
        }
        set
        {
            mySource.maxDistance = value;
        }
    }
    /// <summary>
    /// If using PanByListenerIndex as the VirtualAudioSource type, this value is unused. Adjust the rolloff equation instead!
    /// </summary>
    public virtual float minDistance
    {
        get
        {
            return mySource.minDistance;
        }
        set
        {
            mySource.minDistance = value;
        }
    }
    public virtual bool mute
    {
        get
        {
            return mySource.mute;
        }
        set
        {
            mySource.mute = value;
        }
    }

    /// <summary>
    /// If using PanByListenerIndex as the VirtualAudioSource type, forcibly overriding this value mid game may cause VirtualAudioSources to not sound as intended!
    /// </summary>
    public virtual float pan
    {
        get
        {
            return mySource.panStereo;
        }
        set
        {
            mySource.panStereo = value;
        }
    }

    /// <summary>
    /// If using PanByListenerIndex as the VirtualAudioSource type, forcibly overriding this value mid game may cause VirtualAudioSources to not sound as intended!
    /// </summary>
    public virtual float panLevel
    {
        get
        {
            return mySource.spatialBlend;
        }
        set
        {
            mySource.spatialBlend = value;
        }
    }

    /// <summary>
    /// If using PanByListenerIndex as the VirtualAudioSource type and doppler is enabled for it, forcibly overriding this value mid game may cause VirtualAudioSources to not sound as intended!
    /// Use pitchBaseline to set a baseline pitch instead!
    /// </summary>
    public virtual float pitch
    {
        get
        {
            return mySource.pitch;
        }
        set
        {
            mySource.pitch = value;
        }
    }
    public virtual bool playOnAwake
    {
        get
        {
            return mySource.playOnAwake;
        }
        set
        {
            mySource.playOnAwake = value;
        }
    }
    public virtual int priority
    {
        get
        {
            return mySource.priority;
        }
        set
        {
            mySource.priority = value;
        }
    }

    public virtual AudioRolloffMode rolloffMode
    {
        get
        {
            return mySource.rolloffMode;
        }
        set
        {
            mySource.rolloffMode = value;
        }
    }
    public virtual float spread
    {
        get
        {
            return mySource.spread;
        }
        set
        {
            mySource.spread = value;
        }
    }
    public virtual float time
    {
        get
        {
            return mySource.time;
        }
        set
        {
            mySource.time = value;
        }
    }
    public virtual int timeSamples
    {
        get
        {
            return mySource.timeSamples;
        }
        set
        {
            mySource.timeSamples = value;
        }
    }
    public virtual AudioVelocityUpdateMode velocityUpdateMode
    {
        get
        {
            return mySource.velocityUpdateMode;
        }
        set
        {
            mySource.velocityUpdateMode = value;
        }
    }


    public virtual void GetOutputData(float[] samples, int channel)
    {
        mySource.GetOutputData(samples, channel);
    }
    public virtual void GetSpectrumData(float[] samples, int channel, FFTWindow window)
    {
        mySource.GetSpectrumData(samples, channel, window);
    }
    protected bool pauseAudio = false;
    public virtual void Pause()
    {
        pauseAudio = true;
        mySource.Pause();
    }
    public virtual void Stop()
    {
        StopCoroutine(PLAY_AUDIO_SOURCE_STRING);
        mySource.Stop();
        isCoroutinePlaying = false; // Extra code I had to add in.
    }
    #endregion

}

