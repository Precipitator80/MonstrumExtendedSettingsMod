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
using System.Collections.Generic;

public class VirtualAudioListener : MonoBehaviour {
	
	public static AudioListener sceneAudioListener;
	public static List<VirtualAudioListener> allListeners = new List<VirtualAudioListener>();
	
	/// <summary>
	/// Only used with VirtualAudioSource_PanByListenerIndex. Used to set the desired speaker ratio for this player.
	/// If p1 is on the left side and p2 is on the right,  p1 should be set to -1 and p2 should be set to 1.
	/// </summary>
	[RangeAttribute(-1,1)]
	public float pan2DForListener;
	//It might be cool to also have a variable that controls how good the hearing of this listener is: i.e. a volume multiplier
	
	void OnEnable ()
	{
		if(sceneAudioListener == null)
		{
			sceneAudioListener = (AudioListener)GameObject.FindObjectOfType(typeof(AudioListener));
		}
		allListeners.Add(this);
		
	}
	void OnDisable()
	{
		allListeners.Remove(this);
	}
}
