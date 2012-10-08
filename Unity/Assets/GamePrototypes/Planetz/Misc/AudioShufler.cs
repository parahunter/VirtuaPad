using UnityEngine;
using System.Collections;

public class AudioShufler : MonoBehaviour 
{
	public AudioClip[] clips;
	
	public float minPitch = 0.8f;
	public float maxPitch = 1.2f;
		
	public bool playOnAwake = false;
	
	void Awake()
	{
		if(playOnAwake)
			Play ();
	}
	
	public void Play()
	{
		audio.clip = clips[Random.Range(0, clips.Length - 1)];
		audio.pitch = Random.Range(minPitch,maxPitch);
		audio.Play();
	}
}
