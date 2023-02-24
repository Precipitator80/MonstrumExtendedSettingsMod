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
using System.Collections.Generic;

public class VirtualAudioSource_ClosestListenerOnly : VirtualAudioSource
{

    /// <summary>
    /// If true, this overrides panLevel settings (sets to 1)
    /// </summary>
    public bool force3DSoundOnStartup = true;

    /// <summary>
    /// If set to true, the clip will only play for the listener it started playing for. 
    /// If false, when another listener becomes closer midway through the clip, the sound will switch to be relative to the new closest listener
    /// </summary>
    public bool lockPlayingClipToListener = false;

    public VirtualAudioSource_ClosestListenerOnly(AudioSource passedOriginalAudioSource, AudioSource passedMySource)
    {
        originalAudioSource = passedOriginalAudioSource;
        mySource = passedMySource;
    }

    protected override void OnEnable()
    {
        if (force3DSoundOnStartup)
        {
            if (mySource != null)
            {
                mySource.spatialBlend = 1;
            }
            else
            {
                Debug.LogWarning("No AudioSource assigned to this virtual source! An AudioSource must be assigned for in order to function properly!");
            }
        }
        base.OnEnable();
    }

    public override IEnumerator PlayAudioSource(float delay = 0f)
    {
        isCoroutinePlaying = true;


        if (mySource == null)
        {
            Debug.LogWarning("No AudioSource assigned to this virtual source! An AudioSource must be assigned for in order to function properly!");
        }
        else if (!mySource.gameObject.activeInHierarchy || !mySource.enabled)
        {
            Debug.LogWarning("Cannot play a disabled AudioSource");
        }
        else
        {
            do
            {
                VirtualAudioListener closestListener = GetClosestListener();
                if (closestListener != null)
                {
                    mySource.PlayDelayed(delay);
                }


                if (lockPlayingClipToListener)
                {
                    while (mySource != null && mySource.isPlaying)
                    {
                        //update audio source by keeping the relative positions / orientations between the closest virtual player and this virtual source the same as between the actual source and true listener
                        if (closestListener != null)
                        {
                            mySource.transform.position = Quaternion.Inverse(closestListener.transform.rotation) * (this.transform.position - closestListener.transform.position) + VirtualAudioListener.sceneAudioListener.transform.position;
                        }
                        else
                        {
                            mySource.Stop();
                        }
                        yield return null;
                    }
                }
                else
                {
                    while (mySource != null && mySource.isPlaying)
                    {
                        closestListener = GetClosestListener();
                        if (closestListener != null)
                        {
                            mySource.transform.position = Quaternion.Inverse(closestListener.transform.rotation) * (this.transform.position - closestListener.transform.position) + VirtualAudioListener.sceneAudioListener.transform.position;
                        }
                        else
                        {
                            mySource.Stop();
                        }
                        yield return null;
                    }
                }
                if (mySource != null && !mySource.isPlaying)
                {
                    //This is for a curious case where audio has been requested to play, but Unity doesn't register isPlaying as true.
                    //It seems to happen in larger / slower scenes when the game is tabbed out of, then tabbed back into
                    yield return null;
                }
                delay = 0; //set delay to 0 for looping purposes: only allow a delay on the original play, not on every loop cycle.
            } while (loopCoroutine && !pauseAudio && VirtualAudioListener.allListeners.Count > 0 && mySource != null && mySource.gameObject.activeInHierarchy && mySource.enabled);
        }

        isCoroutinePlaying = false;

    }

    /// <summary>
    /// Returns the closest VirtualAudioListener, or null if there isn't one.
    /// </summary>
    public VirtualAudioListener GetClosestListener()
    {
        List<VirtualAudioListener> listeners = VirtualAudioListener.allListeners;
        VirtualAudioListener returnListener = null;
        float minSqrDist = Mathf.Infinity;
        for (int i = 0; i < listeners.Count; ++i)
        {
            if ((transform.position - listeners[i].transform.position).sqrMagnitude < minSqrDist)
            {
                minSqrDist = (transform.position - listeners[i].transform.position).sqrMagnitude;
                returnListener = listeners[i];
            }
        }
        return returnListener;
    }

    public void Update()
    {
        if (this.isPlaying && !mySource.isPlaying)
        {
            this.Stop();
        }
        /*
        else if (!this.isPlaying && mySource.isPlaying)
        {
            this.Play();
        }
        */
    }


    public virtual float volume
    {
        get
        {
            return mySource.volume;
        }
        set
        {
            mySource.volume = value;
        }
    }
}
