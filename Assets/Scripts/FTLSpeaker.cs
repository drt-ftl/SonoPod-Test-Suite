using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class FtlSpeaker : MonoBehaviour
{
    public SonoLoopManager.SpeakerPosition Position;
    public AudioSource audioSource;
    public bool isPlaying { get; internal set; }
    SpeakerManager speakerManager;
    float timeClipStarted = 0;
    float timePauseStarted = 0;
    float timeSpentPaused = 0;
    public EventHandler FTLSpeaker_PlayPause_Handler;
    public bool isOn { get; internal set; }


    public void InitializeSpeaker_Event(SpeakerManager _speakerManager)
    {
        speakerManager = _speakerManager;
        speakerManager.SpeakerManager_PlayClip_Handler += SpeakerManager_PlayClip;
        speakerManager.SpeakerManager_PlayClipFromDirection_Handler += SpeakerManager_PlayClipFromDirection;
        TestManager.instance.PlayPauseClip_Handler += TestManager_PlayPauseClip;
        speakerManager.SpeakerManager_PlayTestToneEvent_Handler += SpeakerManager_PlayTestToneEvent;
        timeClipStarted = float.MaxValue;
        isPlaying = false;

    }
    public void PlayPause()
    {
        var handler = FTLSpeaker_PlayPause_Handler;
        if (handler != null)
            handler(typeof(SpeakerManager), EventArgs.Empty);
    }
    private void SpeakerManager_PlayTestToneEvent(object sender, System.EventArgs e)
    {
        if (!isOn) return;
        if (audioSource == null) return;
        if (audioSource.loop) return;
        audioSource.volume = 1.0f;
        audioSource.Play();
        timeClipStarted = Time.realtimeSinceStartup;
    }

    private void Update()
    {
        if (Time.realtimeSinceStartup - timeClipStarted >   audioSource.clip.length +  timeSpentPaused)
        {
            timeSpentPaused = 0;
            if (isPlaying)
            {
                TestManager.instance.isPaused = true;
                ControlPad.instance.PlayPause();
            }
            isPlaying = false;
        }
        var limit = 9f;
        switch (TestManager.instance.TestType)
        {
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
            case SonoLoopManager.SonoLoopTestType.PulsedWarble:
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
            case SonoLoopManager.SonoLoopTestType.Calibration:
                limit = 5f;
                break;
            case SonoLoopManager.SonoLoopTestType.HINT:
                limit = 6f;
                break;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                limit = 9f;
                break;
            case SonoLoopManager.SonoLoopTestType.Free:
                limit = 5f;
                if (speakerManager.RingIndex == 1)
                    limit = 1000;
                break;
            default:
                break;
        }
        if (Time.realtimeSinceStartup - timeClipStarted > limit)
        {
            timeClipStarted = float.MaxValue;

            if (!audioSource.loop)
            {
                if (!speakerManager.speakerManagerSource.loop)
                {
                    audioSource.Stop();
                    isPlaying = false;
                }
            }
            if (TestManager.instance != null)
                TestManager.instance.isPaused = true;
            if (ControlPad.instance != null)
                ControlPad.instance.PlayPause();
        }
        
    }
    public void SpeakerSwitch (bool _isOn)
    {
        isOn = _isOn;
    }
    private void SpeakerManager_PlayClip(object sender, System.EventArgs e)
    {
        if (!isOn) return;
        if (audioSource == null) return;
        if (audioSource.loop) return;
        audioSource.Play();
        timeClipStarted = Time.realtimeSinceStartup;
        timeSpentPaused = 0;
        isPlaying = true;
    }
    private void SpeakerManager_PlayClipFromDirection(object sender, System.EventArgs e)
    {
        if (speakerManager.CurrentPosition != Position)
            return;
        if (audioSource == null) return;
        if (audioSource.loop) return;
        audioSource.Play();
        timeClipStarted = Time.realtimeSinceStartup;
        timeSpentPaused = 0;
        isPlaying = true;
    }
    private void TestManager_PlayPauseClip(object sender, System.EventArgs e)
    {
        if (audioSource.loop) return;
        if (audioSource == null) return;
        if (!isOn)   return;
        if (speakerManager.CurrentPosition != Position) return; //audioSource.volume = 0; 
        StartCoroutine(WaitAndPlay());
    }
    IEnumerator WaitAndPlay()
    {
        var sec = 0f;
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.QuickSIN && speakerManager.RingIndex == 0)
            sec = 2f;
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT && speakerManager.RingIndex == 0)
            sec = 1f;
        yield return new WaitForSeconds(sec);
        if (!audioSource.isPlaying)
        {
            audioSource.Play();
            timeClipStarted = Time.realtimeSinceStartup - sec;
            timeSpentPaused = 0;
            isPlaying = true;
        }
        if (TestManager.instance.isPaused)
        {
            if (!(TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.Free && speakerManager.RingIndex == 1))
            {
                audioSource.Pause();
            }
            timePauseStarted = Time.realtimeSinceStartup;
        }
        else
        {
            audioSource.UnPause();
            timeSpentPaused = Time.realtimeSinceStartup - timePauseStarted;
        }
        yield return new WaitForEndOfFrame();
    }

}
