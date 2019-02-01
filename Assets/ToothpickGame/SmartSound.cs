using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SmartSound : MonoBehaviour
{
    private AudioSource _audio;
    public new AudioSource audio
    {
        get
        {
            return _audio;
        }
    }

    public List<AudioClip> clips = new List<AudioClip>();

    public bool playRandomClip = true;

    public bool doNotInterrupt = false;

    void Reset()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null)
        {
            _audio = gameObject.AddComponent<AudioSource>();
        }
        _audio.playOnAwake = false;

    }

    void Awake()
    {
        if (_audio == null)
        {
            _audio = GetComponent<AudioSource>();
            if (_audio == null)
            {
                _audio = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    public void Play()
    {
        if (doNotInterrupt)
        {
            if (_audio.isPlaying)
            {
                return;
            }
        }

        if (playRandomClip)
        {
            if (clips.Count > 1)
            {
                _audio.clip = clips[Random.Range(0, clips.Count)];
            }
        }

        _audio.Play();
    }

    public void Stop()
    {
        if (_audio.isPlaying)
        {
            _audio.Stop();
        }
    }
}