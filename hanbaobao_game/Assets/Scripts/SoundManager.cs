/*
 * SoundManager.cs
 * 
 * Manage playing all of the Music Tracks and Sound Effects in the game,
 * and hold them all in two separate Arrays, accessible by the "Options" Screen.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance = null;  //The SoundManager Object that the rest of the Program can access.
    public AudioSource musicSource; //The AudioSource to play the selected music track.
    public AudioSource sfxSource;   //The AudioSource to play the selected sound effect.
    public AudioClip[] musicTracks; //Every music track in the game, arranged in the approximate order they'd be heard during the game.
    public AudioClip[] soundEffects;    //Every sound effect in the game, generally grouped by source type (Player, Weapon, Enemy, etc.)

	// Use this for initialization
	void Awake()
    {
        //Make the SoundManager a Singleton object.
        //if (instance == null)
        //{
            instance = this;
        /*}
        else if (instance != this)
        {
            Destroy(gameObject);
        }*/
        DontDestroyOnLoad(gameObject);
	}

    //Play the selected Music Track.
    public void PlayMusic(int index)
    {
        musicSource.clip = musicTracks[index];
        musicSource.Play();
    }

    //Play the selected Sound Effect.
    public void PlaySound(int index)
    {
        sfxSource.clip = soundEffects[index];
        sfxSource.Play();
    }
}
