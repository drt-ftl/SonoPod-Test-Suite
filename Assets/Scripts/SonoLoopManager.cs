using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SonoLoopManager : MonoBehaviour
{
    public List<AudioClip> WarbleClips;
    public List<AudioClip> PulsedWarbleClips;
    public List<AudioClip> PureToneClips;
    public List<AudioClip> SentenceClips;
    public List<AudioClip> TwoSyllableClips;
    public List<AudioClip> QuickSINClips_Primary;
    public List<AudioClip> QuickSINClips_Noise;
    public List<AudioClip> HINTClips_Primary;
    public List<AudioClip> HINTClips_Noise;
    public List<AudioClip> CrowdClips;
    public List<AudioClip> FREE_BG_Clips;
    public AudioClip crowdNoise;
    public AudioClip PN_Test;
    public AudioClip PT_Test;
    public AudioClip WT_Test;
    public AudioClip HINT_Test;
    public AudioClip QuickSIN_Test;
    public AudioClip completeClip;
    public AudioClip warningClip;
    public TMP_Text title;
    public TMP_Text details;
    public string VERSION_NAME { get; internal set; }
    public string startIp { get; internal set; }
    public enum CalibrationType { PinkNoise, PureTone, WarbleTone, HINT, QuickSIN, Complete,Warning };
    public Dictionary<CalibrationType, AudioClip> calibrationClips = new Dictionary<CalibrationType, AudioClip>();
    public Dictionary<CalibrationType, int> AttenuationValues = new Dictionary<CalibrationType, int>();

    public static SonoLoopManager instance;
    public enum SonoLoopTestType { HearingThreshold_PT, HearingThreshold_WT, PulsedWarble, SpeechReceptionThreshold, QuickSIN, FreeFieldLocalization, Free, HINT, Calibration }
    public enum SpeakerPosition { FL, FR, SL, SR, CEN, SW, SBL, SBR, NONE }
    //public enum SpeakerRole { Primary, Secondary, Background, None}
    public SonoLoopTestType TestType { get; internal set; }
    public string settingsFilename { get; internal set; }


    void Start()
    {
        instance = this;
        var dirName = System.IO.Path.GetDirectoryName(Application.dataPath);
        var d = dirName.Split(System.IO.Path.DirectorySeparatorChar);
        dirName = dirName.Replace("_Data", "");
        VERSION_NAME = d[d.Length - 1];
        if (VERSION_NAME.Contains("_"))
            VERSION_NAME = VERSION_NAME.Split('_')[1];

        DontDestroyOnLoad(this.gameObject);
        startIp = "172.23.23.2";
        AttenuationValues = new Dictionary<CalibrationType, int>();
        AttenuationValues.Add(CalibrationType.PinkNoise, 210);
        AttenuationValues.Add(CalibrationType.HINT, 250);
        AttenuationValues.Add(CalibrationType.QuickSIN, 223);
        AttenuationValues.Add(CalibrationType.PureTone, 180);
        AttenuationValues.Add(CalibrationType.WarbleTone, 180);

        calibrationClips = new Dictionary<CalibrationType, AudioClip>();
        calibrationClips.Add(CalibrationType.PinkNoise, SonoLoopManager.instance.PN_Test);
        calibrationClips.Add(CalibrationType.HINT, SonoLoopManager.instance.HINT_Test);
        calibrationClips.Add(CalibrationType.QuickSIN, SonoLoopManager.instance.QuickSIN_Test);
        calibrationClips.Add(CalibrationType.PureTone, SonoLoopManager.instance.PT_Test);
        calibrationClips.Add(CalibrationType.WarbleTone, SonoLoopManager.instance.WT_Test);
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        calibrationClips.Add(CalibrationType.Complete, SonoLoopManager.instance.completeClip);
        calibrationClips.Add(CalibrationType.Warning, SonoLoopManager.instance.warningClip);
        settingsFilename = Application.dataPath + "/sonoLoopSettings.txt";
        CheckForSettingsFile();
        TestType = SonoLoopTestType.QuickSIN;
        StartCoroutine(Communicator.instance.SetupFromSettings());
        
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
    }

    public void CheckForSettingsFile()
    {
        if (!System.IO.File.Exists(settingsFilename))
        {
            details.text = "NO SETTINGS FILE";
            return;
        }
        var sr = new System.IO.StreamReader(settingsFilename);
        var line = sr.ReadLine();
        var vals = line.Split(',');
        if (vals.Length < 5) return;
        details.text = vals.Length.ToString() + " values";
        float pn, h, qs, pt, wt;
        if (float.TryParse(vals[0], out pn))
            AttenuationValues[CalibrationType.PinkNoise] = (int)pn;
        if (float.TryParse(vals[1], out h))
            AttenuationValues[CalibrationType.HINT] = (int)h;
        if (float.TryParse(vals[2], out qs))
            AttenuationValues[CalibrationType.QuickSIN] = (int)qs;
        if (float.TryParse(vals[3], out pt))
            AttenuationValues[CalibrationType.PureTone] = (int)pt;
        if (float.TryParse(vals[4], out wt))
            AttenuationValues[CalibrationType.WarbleTone] = (int)wt;
        if (vals.Length > 5)
            startIp = vals[5];
        details.text = "SETTINGS LINE: " + line;

    }
    public float TrySetVolume(float currentLinear, float stepDB)
    {
        var vol_dB = LinearToDecibel(currentLinear) + stepDB;
        var vol_linear = DecibelToLinear(vol_dB);
        if (vol_linear < 0) vol_linear = 0;
        if (vol_linear > 1.0f) vol_linear = 1;
        return vol_linear;
    }
    public float LinearToDecibel(float linear)
    {
        float dB;
        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;
        return dB;
    }
    public float DecibelToLinear(float dB)
    {
        float linear = Mathf.Pow(10.0f, dB / 20.0f);
        return linear;
    }
    public float Linear_To_dBSPL(float linear)
    {
        //      1.0 Linear =   80dB SPL
        //      0.0 Linear = -144dB SPL

        linear *= 10000;
        float dB;
        if (linear != 0)
            dB = 20.0f * Mathf.Log10(linear);
        else
            dB = -144.0f;
        return dB;
    }
    public float dBSPL_To_Linear(float dB)
    {
        dB -= 80f;
        float linear = Mathf.Pow(10.0f, dB / 20.0f);
        return linear;
    }

    public SpeakerPosition getPositionFromText(string text)
    {
        var pos = SpeakerPosition.NONE;
        if (text == "FL") pos = SpeakerPosition.FL;
        if (text == "FR") pos = SpeakerPosition.FR;
        if (text == "CEN") pos = SpeakerPosition.CEN;
        if (text == "SW") pos = SpeakerPosition.SW;
        if (text == "SBL") pos = SpeakerPosition.SBL;
        if (text == "SBR") pos = SpeakerPosition.SBR;
        if (text == "SL") pos = SpeakerPosition.SL;
        if (text == "SR") pos = SpeakerPosition.SR;
        return pos;
    }
    public string getTextFromPosition(SpeakerPosition pos)
    {
        var text = "";
        if (pos == SpeakerPosition.FL) text = "FL";
        if (pos == SpeakerPosition.FR) text = "FR";
        if (pos == SpeakerPosition.CEN) text = "CEN";
        if (pos == SpeakerPosition.SW) text = "SW";
        if (pos == SpeakerPosition.SBL) text = "SBL";
        if (pos == SpeakerPosition.SBR) text = "SBR";
        if (pos == SpeakerPosition.SL) text = "SL";
        if (pos == SpeakerPosition.SR) text = "SR";
        return text;
    }
    public SpeakerPosition getPositionFromIndex(int _index)
    {
        var pos = SpeakerPosition.NONE;
        if (_index == 0) pos = SpeakerPosition.FL;
        if (_index == 1) pos = SpeakerPosition.FR;
        if (_index == 2) pos = SpeakerPosition.CEN;
        if (_index == 3) pos = SpeakerPosition.SW;
        if (_index == 4) pos = SpeakerPosition.SBL;
        if (_index == 5) pos = SpeakerPosition.SBR;
        if (_index == 6) pos = SpeakerPosition.SL;
        if (_index == 7) pos = SpeakerPosition.SR;
        return pos;
    }
    public int getIndexFromPosition(SpeakerPosition pos)
    {
        if (pos == SpeakerPosition.FL) return 0;
        if (pos == SpeakerPosition.FR) return 1;
        if (pos == SpeakerPosition.CEN) return 2;
        if (pos == SpeakerPosition.SW) return 3;
        if (pos == SpeakerPosition.SBL) return 4;
        if (pos == SpeakerPosition.SBR) return 5;
        if (pos == SpeakerPosition.SL) return 6;
        if (pos == SpeakerPosition.SR) return 7;
        return -1;
    }
    public float getRadiansFromPosition(SpeakerPosition pos)
    {
        float val = 0;
        if (pos == SpeakerPosition.FL) val = -Mathf.PI / 4f;
        if (pos == SpeakerPosition.FR) val = Mathf.PI / 4f;
        if (pos == SpeakerPosition.CEN) val = 0;
        if (pos == SpeakerPosition.SW) val = Mathf.PI;
        if (pos == SpeakerPosition.SBL) val = - 3 *Mathf.PI / 4f;
        if (pos == SpeakerPosition.SBR) val = 3 * Mathf.PI / 4f;
        if (pos == SpeakerPosition.SL) val = -Mathf.PI / 2f;
        if (pos == SpeakerPosition.SR) val = Mathf.PI / 2f;
        return val;
    }
    public void Exit()
    {
        Application.Quit();
    }
}
