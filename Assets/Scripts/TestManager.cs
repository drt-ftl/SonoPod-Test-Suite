using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Manages the overall test.
/// Gets Inputs from Buttons, Keypad
/// Owns Events for playing clips.
/// Owns all Speaker Managers (0 is Primary, 1 is Secondary, 2 is Background)
/// Owns the Current Clip
/// </summary>
public class TestManager : MonoBehaviour
{
    public List<List<AudioClip>> Clips;

    public event EventHandler PlayPauseClip_Handler;
    public List<SpeakerManager> SpeakerManagers { get; internal set; }
    public TMP_Text titleText;
    public TMP_Text instructionsText;
    public static TestManager instance;
    public AudioClip warningTone;
    public TMP_Dropdown primaryDropdown;
    public TMP_Dropdown secondaryDropdown;
    public GameObject freePanel;
    public GameObject CalibrationPanel;
    public List<GameObject> nonCaliObjects;
    public Button proceedButton;
    public Button returnButton;
    public TMP_Text inProgressText;
    public TMP_Text versionText;
    public bool isPaused { get; set; }
    public bool CtrlDown { get; set; }
    public SonoLoopManager.SonoLoopTestType TestType { get;  set; }
    public List<int> CurrentClipIndex { get; internal set; }
    public SonoLoopManager.CalibrationType FreeCaliTypePrimary { get; internal set; }
    public SonoLoopManager.CalibrationType FreeCaliTypeSecondary { get; internal set; }
    public SonoLoopManager.SpeakerPosition CurrentHintPosition { get; set; }
    public List<SonoLoopManager.SpeakerPosition> FrontSpeaker_List 
        { get { return new List<SonoLoopManager.SpeakerPosition>(1) { SonoLoopManager.SpeakerPosition.CEN }; } }
    public List<SonoLoopManager.SpeakerPosition> RearSpeaker_List
        { get { return new List<SonoLoopManager.SpeakerPosition>(1) { SonoLoopManager.SpeakerPosition.SW }; } }
    public List<SonoLoopManager.SpeakerPosition> LeftSpeaker_List
    { get { return new List<SonoLoopManager.SpeakerPosition>(1) { SonoLoopManager.SpeakerPosition.SL }; } }
    public List<SonoLoopManager.SpeakerPosition> RightSpeaker_List
    { get { return new List<SonoLoopManager.SpeakerPosition>(1) { SonoLoopManager.SpeakerPosition.SR }; } }
    public List<SonoLoopManager.SpeakerPosition> LeftRightSpeaker_List
    { get { return new List<SonoLoopManager.SpeakerPosition>(2) { SonoLoopManager.SpeakerPosition.SL, SonoLoopManager.SpeakerPosition.SR }; } }
    public List<SonoLoopManager.SpeakerPosition> HINT_Bg_List
    { get { return new List<SonoLoopManager.SpeakerPosition>(3) { SonoLoopManager.SpeakerPosition.CEN, SonoLoopManager.SpeakerPosition.SL, SonoLoopManager.SpeakerPosition.SR }; } }
    public List<SonoLoopManager.SpeakerPosition> AllSpeaker_List
    { 
        get 
        {
            var speakerList = new List<SonoLoopManager.SpeakerPosition>();
            for (int i = 0; i < 8; i++)
                speakerList.Add(SonoLoopManager.instance.getPositionFromIndex(i));
            return speakerList; 
        } 
    }
    public List<SonoLoopManager.SpeakerPosition> HINTSpeaker_List (SonoLoopManager.SpeakerPosition pos)
    {
            var speakerList = new List<SonoLoopManager.SpeakerPosition>();
            speakerList.Add(pos);
            return speakerList;
    }
    void Awake()
    {
        if (SonoLoopManager.instance == null)
        {
            SceneManager.LoadScene("Start");
            return;
        }        
        instance = this; 
        FreeCaliTypePrimary = SonoLoopManager.CalibrationType.WarbleTone;
        FreeCaliTypeSecondary = SonoLoopManager.CalibrationType.WarbleTone;
        isPaused = true;
        TestType = SonoLoopManager.instance.TestType;
        var info = Instructions.GetInfo(TestType);
        titleText.text = info[0];
        instructionsText.text = info[1];
        Clips = new List<List<AudioClip>>();
        freePanel.SetActive(false);
        var noiseClips = new List<AudioClip>();
        versionText.text = SonoLoopManager.instance.VERSION_NAME;
        switch (SonoLoopManager.instance.TestType)
        {
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
                Clips.Add(SonoLoopManager.instance.PureToneClips);
                Clips.Add(SonoLoopManager.instance.WarbleClips);
                break;
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
                Clips.Add(SonoLoopManager.instance.WarbleClips);
                Clips.Add(SonoLoopManager.instance.PureToneClips);
                break;
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
                Clips.Add(SonoLoopManager.instance.TwoSyllableClips);
                Clips.Add(SonoLoopManager.instance.TwoSyllableClips);
                break;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                Clips.Add(SonoLoopManager.instance.QuickSINClips_Primary);
                var pClipsQ = SonoLoopManager.instance.QuickSINClips_Primary;
                var nClipsQ = SonoLoopManager.instance.QuickSINClips_Noise;
                for (int i = 0; i < pClipsQ.Count; i++)
                {
                    noiseClips.Add(nClipsQ[i % nClipsQ.Count]);
                }
                Clips.Add(noiseClips);
                break;
            case SonoLoopManager.SonoLoopTestType.FreeFieldLocalization:
                Clips.Add(SonoLoopManager.instance.PureToneClips);
                Clips.Add(SonoLoopManager.instance.WarbleClips);
                break;
            case SonoLoopManager.SonoLoopTestType.Free:
                Clips.Add(SonoLoopManager.instance.SentenceClips);
                Clips.Add(SonoLoopManager.instance.SentenceClips);
                freePanel.SetActive(true);
                break;
            case SonoLoopManager.SonoLoopTestType.HINT:
                Clips.Add(SonoLoopManager.instance.HINTClips_Primary);
                var pClips = SonoLoopManager.instance.HINTClips_Primary;
                var nClips = SonoLoopManager.instance.HINTClips_Noise;
                for (int i = 0; i <  pClips.Count; i++)
                {
                    noiseClips.Add(nClips[i % nClips.Count]);
                }
                Clips.Add(noiseClips);
                break;

            default:
                Clips.Add(SonoLoopManager.instance.SentenceClips);
                Clips.Add(SonoLoopManager.instance.SentenceClips);
                break;
        }
        SpeakerManagers = new List<SpeakerManager>();

        var PrimarySpeakerManager = new GameObject("PrimarySpeakerManager");
        PrimarySpeakerManager.AddComponent<AudioSource>();
        PrimarySpeakerManager.AddComponent<SpeakerManager>();
        PrimarySpeakerManager.transform.parent = transform;
        SpeakerManagers.Add(PrimarySpeakerManager.GetComponent<SpeakerManager>());

        var SecondarySpeakerManager = new GameObject("SecondarySpeakerManager");
        SecondarySpeakerManager.AddComponent<AudioSource>();
        SecondarySpeakerManager.AddComponent<SpeakerManager>();
        SecondarySpeakerManager.transform.parent = transform;
        SpeakerManagers.Add(SecondarySpeakerManager.GetComponent<SpeakerManager>());

        for (int i = 0; i < SpeakerManagers.Count; i++)
        {
            SpeakerManagers[i].InitializeSpeakerManager(i);
        }

        if (TestType == SonoLoopManager.SonoLoopTestType.Free)
        {
            ChangeSoundArray(0);
            ChangeSoundArray(1);
        }

        //#if UNITY_EDITOR
        //#else
        Communicator.instance.SetAttenuation(TestType);
        //#endif

        if (TestType == SonoLoopManager.SonoLoopTestType.Calibration)
            OpenCalibrationPanel();
    }
    void Update()
    {
        if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            && (TestType == SonoLoopManager.SonoLoopTestType.QuickSIN 
            || TestType == SonoLoopManager.SonoLoopTestType.HINT
            || TestType == SonoLoopManager.SonoLoopTestType.Free))
            CtrlDown = true;
        else CtrlDown = false;

        if (Input.GetKey(KeyCode.B))
            ToggleBackground(true);
    }
    public void TestManager_PlayPause_Event()
    {
        isPaused = !isPaused;
        var handler = PlayPauseClip_Handler;
        if (handler != null)
            handler(typeof(SpeakerManager), EventArgs.Empty);
    }
    public void ToggleBackground (bool _turnOn)
    {
        if (SpeakerManagers.Count < 3) return;
        SpeakerManagers[2].ToggleSpeakers(_turnOn);        
    }
    public void StopPlayingAll()
    {
        if (ControlPad.instance == null || SpeakerManagers == null) return;
        isPaused = true;
        ControlPad.instance.PlayPause();
        foreach (var s in SpeakerManagers)
            s.StopPlaying();
    }
    public List<SonoLoopManager.SpeakerPosition> GetSpeakersUsed(int ringIndex)
    {
        switch (TestType)
        {

            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
                if (ringIndex == 0)
                    return FrontSpeaker_List;
                break;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                return FrontSpeaker_List;
            case SonoLoopManager.SonoLoopTestType.Calibration:
                if (ringIndex == 0)
                    return FrontSpeaker_List;
                else if (ringIndex == 1)
                    return new List<SonoLoopManager.SpeakerPosition>();
                break;
            case SonoLoopManager.SonoLoopTestType.Free:
                return AllSpeaker_List;
                break;
            case SonoLoopManager.SonoLoopTestType.FreeFieldLocalization:
                if (ringIndex == 0)
                    return AllSpeaker_List;
                break;
            case SonoLoopManager.SonoLoopTestType.HINT:
                if (ringIndex == 0)
                    return FrontSpeaker_List;
                else if (ringIndex == 1)
                    //return HINT_Bg_List;
                    return AllSpeaker_List;
                break;
            default:
                break;
        }
        return new List<SonoLoopManager.SpeakerPosition>();
    }
    public void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu");
    }
    public void ChangeSoundArray(int i)
    {
        isPaused = true;
        if (ControlPad.instance != null)
            ControlPad.instance.GetComponent<SpriteRenderer>().sprite = ControlPad.instance.playSprite;
        var testTypePrimary = TestType;
        var testTypeSecondary = TestType;
        switch (primaryDropdown.value)
        {
            case 0:
                SpeakerManagers[0].Clips = SonoLoopManager.instance.PureToneClips;
                testTypePrimary = SonoLoopManager.SonoLoopTestType.HearingThreshold_PT;
                break;
            case 1:
                SpeakerManagers[0].Clips = SonoLoopManager.instance.WarbleClips;
                testTypePrimary = SonoLoopManager.SonoLoopTestType.HearingThreshold_WT;
                break;
            case 2:
                SpeakerManagers[0].Clips = SonoLoopManager.instance.SentenceClips;
                testTypePrimary = SonoLoopManager.SonoLoopTestType.QuickSIN;
                break;
            case 3:
                SpeakerManagers[0].Clips = SonoLoopManager.instance.TwoSyllableClips;
                testTypePrimary = SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold;
                break;
            case 4:
                SpeakerManagers[0].Clips = SonoLoopManager.instance.QuickSINClips_Noise;
                testTypePrimary = SonoLoopManager.SonoLoopTestType.QuickSIN;
                break;
            default:
                SpeakerManagers[0].Clips = SonoLoopManager.instance.PureToneClips;
                testTypePrimary = SonoLoopManager.SonoLoopTestType.HearingThreshold_PT;
                break;

        }
        switch (secondaryDropdown.value)
        {
            case 0:
                SpeakerManagers[1].Clips = SonoLoopManager.instance.PureToneClips;
                testTypeSecondary = SonoLoopManager.SonoLoopTestType.HearingThreshold_PT;
                break;
            case 1:
                SpeakerManagers[1].Clips = SonoLoopManager.instance.WarbleClips;
                testTypeSecondary = SonoLoopManager.SonoLoopTestType.HearingThreshold_WT;
                break;
            case 2:
                SpeakerManagers[1].Clips = SonoLoopManager.instance.SentenceClips;
                testTypeSecondary = SonoLoopManager.SonoLoopTestType.QuickSIN;
                break;
            case 3:
                SpeakerManagers[1].Clips = SonoLoopManager.instance.TwoSyllableClips;
                testTypeSecondary = SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold;
                break;
            case 4:
                SpeakerManagers[1].Clips = SonoLoopManager.instance.QuickSINClips_Noise;
                testTypeSecondary = SonoLoopManager.SonoLoopTestType.QuickSIN;
                break;
            default:
                SpeakerManagers[1].Clips = SonoLoopManager.instance.QuickSINClips_Noise;
                testTypeSecondary = SonoLoopManager.SonoLoopTestType.QuickSIN;
                break;

        }
        FreeCaliTypePrimary = GetCaliType(testTypePrimary);
        FreeCaliTypeSecondary = GetCaliType(testTypeSecondary);
        if (TestType != SonoLoopManager.SonoLoopTestType.Free)
        {
            StopPlayingAll();
            SpeakerManagers[0].ChangeClip(0);
            SpeakerManagers[1].ChangeClip(0);
        }
        else
        {
            SpeakerManagers[i].StopPlaying();
            SpeakerManagers[i].ChangeClip(0);
        }
        //#if UNITY_EDITOR
        //#else
        if (i == 0)
            Communicator.instance.SetFreeAttenuation(FreeCaliTypePrimary);
        //#endif
    }
    public void OpenCalibrationPanel()
    {
        proceedButton.interactable = false;
        CalibrationPanel.SetActive(true);
        StartCoroutine(PlayWarning_Coroutine());
        foreach (var o in nonCaliObjects)
            o.SetActive(false);
        inProgressText.enabled = false;
    }
    IEnumerator PlayWarning_Coroutine()
    {
        yield return new WaitForSeconds(0.5f);
        Communicator.instance.PlaySound(SonoLoopManager.CalibrationType.Warning);
        yield return new WaitForSeconds(5.1f);
        proceedButton.interactable = true;
        yield return new WaitForEndOfFrame();
    }
    public void CloseCalibrationPanel()
    {
        if (TestType == SonoLoopManager.SonoLoopTestType.Calibration)
        {
            SceneManager.LoadScene("Menu");
            return;
        }
        CalibrationPanel.SetActive(false);
        foreach (var o in nonCaliObjects)
            o.SetActive(true);
    }
    public void Calibrate()
    {
        Communicator.instance.StartTimedSequence();
    }
    public void Exit()
    {
        Application.Quit();
    }
    public SonoLoopManager.CalibrationType GetCaliType(SonoLoopManager.SonoLoopTestType testType)         
    {
        switch (testType)
        {
            case SonoLoopManager.SonoLoopTestType.FreeFieldLocalization:
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
                return SonoLoopManager.CalibrationType.PureTone;
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
                return SonoLoopManager.CalibrationType.WarbleTone;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                return SonoLoopManager.CalibrationType.QuickSIN;
            case SonoLoopManager.SonoLoopTestType.HINT:
                return SonoLoopManager.CalibrationType.HINT;
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
                return SonoLoopManager.CalibrationType.PinkNoise;
            default:
                return SonoLoopManager.CalibrationType.PinkNoise;
        }
    }

}
