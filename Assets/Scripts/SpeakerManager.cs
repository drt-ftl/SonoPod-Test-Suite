using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a speaker ring (Primary or Secondary)
/// </summary>
public class SpeakerManager : MonoBehaviour
{
    public Dictionary<SonoLoopManager.SpeakerPosition, FtlSpeaker> Speakers;
    public event EventHandler SpeakerManager_PlayClip_Handler;
    public event EventHandler SpeakerManager_PlayClipFromDirection_Handler;
    public event EventHandler SpeakerManager_PlayTestToneEvent_Handler;
    public AudioSource speakerManagerSource;
    public int RingIndex;
    public List<SonoLoopManager.SpeakerPosition> PositionsToUse;    
    public SonoLoopManager.SpeakerPosition CurrentPosition { get; set; }
    public List<AudioClip> Clips { get; internal set; }
    bool ClipInQueue;
    float clipStartTime;
    float currentClipDuration = 0;
    public int CurrentClipNumber { get; internal set; }
    public float SpeakerRingVolume { get; internal set; }
    public SonoLoopManager.CalibrationType FreeCaliType { get; internal set; }
    public void InitializeSpeakerManager(int ringIndex)
    {
        RingIndex = ringIndex;
        var v = 0f;
        var vol_std = SonoLoopManager.instance.dBSPL_To_Linear(30);
        var vol_qs = SonoLoopManager.instance.dBSPL_To_Linear(60);
        var vol_bg = SonoLoopManager.instance.dBSPL_To_Linear(16);
        switch (SonoLoopManager.instance.TestType)
        {
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
            case SonoLoopManager.SonoLoopTestType.PulsedWarble:
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
            case SonoLoopManager.SonoLoopTestType.FreeFieldLocalization:
                if (RingIndex == 0) v = vol_std;
                else v = 0;
                break;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                if (RingIndex == 0) v = vol_qs;
                else v = vol_bg;
                break;
            case SonoLoopManager.SonoLoopTestType.Calibration:
                if (RingIndex == 0) v = 1f;
                else v = 0;
                break;
            case SonoLoopManager.SonoLoopTestType.HINT:
                v = vol_std;
                break;
            case SonoLoopManager.SonoLoopTestType.Free:
                v = vol_std;
                break;
            default:
                v = 0.4f;
                break;
        }
        SpeakerRingVolume = v;
        ChangeVolume(0);
        speakerManagerSource = GetComponent<AudioSource>();
        Clips = TestManager.instance.Clips[ringIndex];

        speakerManagerSource.clip = Clips[0];
        Speakers = new Dictionary<SonoLoopManager.SpeakerPosition, FtlSpeaker>();
        PositionsToUse = TestManager.instance.GetSpeakersUsed(RingIndex);
        CurrentPosition = SonoLoopManager.SpeakerPosition.CEN;
        if (PositionsToUse.Count > 0) CurrentPosition = PositionsToUse[0];
        for (int i = 0; i < 8; i++)
        {
            var speakerPosition = SonoLoopManager.instance.getPositionFromIndex(i);
            var newSpeakerGO = new GameObject(speakerPosition.ToString());
            var pos = Vector3.zero;
            var angle = SonoLoopManager.instance.getRadiansFromPosition(speakerPosition);
            pos.x = Mathf.Cos(angle);
            pos.y = Mathf.Sin(angle);
            newSpeakerGO.transform.localPosition = pos * 5;
            newSpeakerGO.transform.parent = transform;
            var newSpeaker = newSpeakerGO.AddComponent<FtlSpeaker>();
            newSpeaker.Position = speakerPosition;
            newSpeaker.audioSource = newSpeaker.gameObject.AddComponent<AudioSource>();
            newSpeaker.audioSource.volume = SpeakerRingVolume;

            if (PositionsToUse.Contains(speakerPosition)) newSpeaker.SpeakerSwitch(true);
            else newSpeaker.SpeakerSwitch(false);
            Speakers.Add(speakerPosition, newSpeaker);
        }
        foreach (var speaker in Speakers.Values)
        {
            speaker.InitializeSpeaker_Event(this);
        }
        RefreshSpeakers(Clips[CurrentClipNumber]);

        CurrentPosition = SonoLoopManager.SpeakerPosition.CEN;
    }
    public void StopPlaying()
    {
        if (speakerManagerSource.loop) return;
        Debug.Log("Stopping " + RingIndex.ToString());
        foreach (var speaker in Speakers)
        {
            if (!speaker.Value.audioSource.loop)
                speaker.Value.audioSource.Stop();
        }
    }
    public void ChangeClip(int increment)
    {
        CurrentClipNumber+= increment;
        if (CurrentClipNumber < 0)
        {
            if (Clips.Count > 0)
                CurrentClipNumber = Clips.Count - 1;
            else CurrentClipNumber = 0;
        }
        if (CurrentClipNumber >= Clips.Count)
            CurrentClipNumber = 0;
        ClipInQueue = true;
        RefreshSpeakers(Clips[CurrentClipNumber]);
    }
    public void PlayOnce_Event()
    {
        if (ClipInQueue)
        {
            RefreshSpeakers(Clips[CurrentClipNumber]); 
            clipStartTime = Time.realtimeSinceStartup;
        }
        var handler = SpeakerManager_PlayClip_Handler;
        if (handler != null)
            handler(typeof(SpeakerManager), EventArgs.Empty);
    }
    public void PlayTestTone_Event(AudioClip testClip, bool isWarning)    // Just for SPL Calibration
    {
        if (RingIndex == 1)
            return;
        SpeakerRingVolume = 1.0f;
        if (isWarning)
            SpeakerRingVolume = 0.8f;
        ChangeVolume(0);
        RefreshSpeakers(testClip);
        var handler = SpeakerManager_PlayTestToneEvent_Handler;
        if (handler != null)
            handler(typeof(SpeakerManager), EventArgs.Empty);
    }

    public void PlayFromDirection_Event(SonoLoopManager.SpeakerPosition pos)
    {
        if (ClipInQueue)
        {
            RefreshSpeakers(Clips[CurrentClipNumber]);
            clipStartTime = Time.realtimeSinceStartup;
        }
        CurrentPosition = pos;
        clipStartTime = Time.realtimeSinceStartup;
        var handler = SpeakerManager_PlayClipFromDirection_Handler;
        if (handler != null)
            handler(typeof(SpeakerManager), EventArgs.Empty);
    }
    public void ChangeVolume(float magnitude)
    {
        if (Speakers == null) return;
        SpeakerRingVolume = SonoLoopManager.instance.TrySetVolume(SpeakerRingVolume, magnitude);
        foreach (var speaker in Speakers.Values)
        {
            var v = 0f;
            if (speaker.Position == CurrentPosition)
            {
                v = SpeakerRingVolume;
                if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.Free
                    && RingIndex == 1)
                {
                    var attnCurrent = SonoLoopManager.instance.AttenuationValues[TestManager.instance.FreeCaliTypePrimary];
                    var attnDesired = SonoLoopManager.instance.AttenuationValues[TestManager.instance.FreeCaliTypeSecondary];
                    var dB1 = attnCurrent * 0.375f;
                    var dB2 = attnDesired * 0.375f;
                    var linear1 = SonoLoopManager.instance.dBSPL_To_Linear(dB1);
                    var linear2 = SonoLoopManager.instance.dBSPL_To_Linear(dB2);
                    var linearDiff = linear1 - linear2;
                    linearDiff *= -1f;
                    if (linearDiff < 0) linearDiff = 0;
                    if (linearDiff > 1) linearDiff = 1;
                    v *= (1 - linearDiff);                    
                }
            }
            speaker.audioSource.volume = v;
        }
    }
    private void Update()
    {
        if (TestManager.instance == null) return;
        if (ClipInQueue && Time.realtimeSinceStartup - clipStartTime > currentClipDuration)
            RefreshSpeakers(Clips[CurrentClipNumber]);

        if (TestManager.instance.CtrlDown)
        {
            if (RingIndex != 1) return;            
        }
        else if (RingIndex != 0) return;
        var pos = SonoLoopManager.SpeakerPosition.NONE;
        if (Input.GetKeyDown(KeyCode.Keypad8) || Input.GetKeyDown(KeyCode.UpArrow))
            pos = SonoLoopManager.SpeakerPosition.CEN;
        if (Input.GetKeyDown(KeyCode.Keypad9) || Input.GetKeyDown(KeyCode.PageUp))
            pos = SonoLoopManager.SpeakerPosition.FR;
        if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.RightArrow))
            pos = SonoLoopManager.SpeakerPosition.SR;
        if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.PageDown))
            pos = SonoLoopManager.SpeakerPosition.SBR;
        if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.DownArrow))
            pos = SonoLoopManager.SpeakerPosition.SW;
        if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.End))
            pos = SonoLoopManager.SpeakerPosition.SBL;
        if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.LeftArrow))
            pos = SonoLoopManager.SpeakerPosition.SL;
        if (Input.GetKeyDown(KeyCode.Keypad7) || Input.GetKeyDown(KeyCode.Home))
            pos = SonoLoopManager.SpeakerPosition.FL;

        if (pos != SonoLoopManager.SpeakerPosition.NONE)
        {
            PlayFromDirection_Event(pos);
        }
    }
    public void RefreshSpeakers(AudioClip clip)
    {
        ClipInQueue = false;
        foreach (var speaker in Speakers.Values)
        {
            if (speaker.audioSource == null || speakerManagerSource == null) return;

            if (!speakerManagerSource.loop)
                speaker.audioSource.Stop();
            int outputChannel = SonoLoopManager.instance.getIndexFromPosition(speaker.Position);
            AudioClip newClip = AudioClip.Create(clip.name, clip.samples, 8, clip.frequency, false);
            float[] newAudioData = new float[clip.samples * 8];
            float[] originalAudioData = new float[clip.samples * clip.channels];
            if (!clip.GetData(originalAudioData, 0))
                return;
            int inputChannel = SonoLoopManager.instance.getIndexFromPosition(speaker.Position);
            int originalClipIndex = inputChannel % clip.channels;
            for (int i = outputChannel; i < newAudioData.Length; i += 8)
            {
                newAudioData[i] += originalAudioData[originalClipIndex];
                originalClipIndex += clip.channels;
            }
            if (!newClip.SetData(newAudioData, 0))
                return;
            speaker.audioSource.clip = newClip;
        }
        currentClipDuration = Speakers[0].audioSource.clip.length;
    }
    public void SonoLoopButtonInput(SonoLoopButton sonoLoopButton)
    {
        switch (sonoLoopButton.buttonType)
        {
            case SonoLoopButton.ButtonType.Play:
                PlayOnce_Event();
                break;
            case SonoLoopButton.ButtonType.Clip:
                if (sonoLoopButton.magnitude > 0) ChangeClip(1);
                else if (sonoLoopButton.magnitude < 0) ChangeClip(-1);
                else PlayOnce_Event();
                break;
            case SonoLoopButton.ButtonType.Volume:
                ChangeVolume(sonoLoopButton.magnitude);
                break;
            default:
                break;
        }
    }
}
