using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
public class Communicator : MonoBehaviour
{
    public static Communicator instance;
    public string ipStr { get; set; }
    public string baseUrl
    {
        get { return "http://" + ipStr + ":8000"; }
    }
    const string Calibrate = "/cal/mic_spl/";
    const string GetCurrentAndMax = "/api/mic_spl/";
    const string GetCurrentMode = "/api/calibration/mode/";
    const string SetCurrentCalibrationModePink = "/api/calibration/mode/Pink%20Noise";
    const string SetCurrentCalibrationModePure = "/api/calibration/mode/Pure%20Tone";
    const string SetCurrentCalibrationModeWarble = "/api/calibration/mode/Warble%20Tone";
    const string GetCurrentUsbVolumeState = "/api/volume/usb_control_enabled/";
    const string EnableWindowsAudio = "/api/volume/usb_control_enabled/true/";
    const string DisableWindowsAudio = "/api/volume/usb_control_enabled/false/";
    const string GetCurrentMuteValue = "/api/volume/mute/";
    const string SetMuteOn = "/api/volume/mute/true/";
    const string SetMuteOff = "/api/volume/mute/false/";
    const string GetCurrentVolume = "/api/volume/value/";
    const  string SetVolume_AttnValue = "/api/volume/value/*";
    Dictionary<SonoLoopManager.CalibrationType, List<float>> currentDbList = new Dictionary<SonoLoopManager.CalibrationType, List<float>>();
    Dictionary<SonoLoopManager.CalibrationType, int> currentVol;
    int iterations = 0;
    public List<string> MessageRecord;
    public GameObject IP_Window;
    // Sets the current volume value. An HTTP error 409 occurs if usb_control_enabled is true. The value is an attenuation (attn) value, such that:
    // attn(dB) = (0xFF - value) * 0.375
    // E.G: 
    // 0xFF = 0 dB
    // 0xFE = -0.375 dB
    // 0xFD = -0.375 * (0xFF - 0xFD) = -0.75 dB
    // ...
    // 0x00 = -0.375 * (0xFF - 0x00) = -95.625 dB

    // Now, remember that these are dB but NOT dB SPL.So, set the volume to a reasonable level (probably not max...) and measure the SPL with the mic in the center of the loop.Then, 
    // find the difference in dB between the measured and target values.Apply this offset using the attn conversion above.
    // E.G.:
    // Known:
    // Test Level: 0xF0
    // Target SPL: 80 dB
    // Measured SPL: 90 dB
    // Calculated:
    // dB diff = (Target - Measured) = (90 - 80) = -10dB
    // value diff = -10dB / 0.375 = 26
    // Corrected Level = 0xF0 - 26 = 0xD6
    //  Conversion from a Windows volume level is a bit more difficult, but not too bad.I don't think this calculation will be necessary but are included for reference (but have not been veriified). Note that windows only uses a part of the above range, otherwise the volume control would be too sensitive. There's also some rounding involved, so these might not be exactly correct.
    //100% Windows volume = 0xFF or 0 dB attn)
    //50% Windows volume = 0xA5 or(0xFF - 0xA5) * 0.375 = -33.75dB
    //0% Windows volume = 0x4B or(0xFF - 0x4B) * 0.375 = -67.5dB

    void Awake()
    {
        instance = this; 
        MessageRecord = new List<string>();
    }
    private void Start()
    {
        ipStr = "172.23.23.2";
        if (SonoLoopManager.instance.startIp != "") ipStr = SonoLoopManager.instance.startIp;
    }
    public void InitializeCommunicator()
    {
        StartCoroutine(InitializeSequence_Coroutine());
    }
    private void OnApplicationQuit()
    {
        StartCoroutine(Communicate_Coroutine(EnableWindowsAudio, SonoLoopManager.CalibrationType.PinkNoise));
        WriteReports();
    }
    public void SetAttenuation(SonoLoopManager.SonoLoopTestType testType)
    {
        var caliType = SonoLoopManager.CalibrationType.PureTone;
        switch (testType)
        {
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
            case SonoLoopManager.SonoLoopTestType.FreeFieldLocalization:
            case SonoLoopManager.SonoLoopTestType.Free:
                caliType = SonoLoopManager.CalibrationType.PureTone;
                break;
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
                caliType = SonoLoopManager.CalibrationType.WarbleTone;
                break;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                caliType = SonoLoopManager.CalibrationType.QuickSIN;
                break;
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
                caliType = SonoLoopManager.CalibrationType.PinkNoise;
                break;
            case SonoLoopManager.SonoLoopTestType.HINT:
                caliType = SonoLoopManager.CalibrationType.HINT;
                break;
            default:
                break;
        }
        StartCoroutine(ChangeVolume_Coroutine(caliType));
    }
    public void SetFreeAttenuation(SonoLoopManager.CalibrationType type)
    {
        StartCoroutine(ChangeVolume_Coroutine(type));
    }
    public void PlaySound(SonoLoopManager.CalibrationType type)
    {
        var isWarning = false;
        if (type == SonoLoopManager.CalibrationType.Warning || type == SonoLoopManager.CalibrationType.Complete)
            isWarning = true;
        TestManager.instance.SpeakerManagers[0].PlayTestTone_Event(SonoLoopManager.instance.calibrationClips[type], isWarning);
    }
    public void StartTimedSequence()
    {
        SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.PinkNoise] = 210;
        SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.HINT] = 230;
        SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.QuickSIN] = 223;
        SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.PureTone] = 180;
        SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.WarbleTone] = 180;

        currentVol = new Dictionary<SonoLoopManager.CalibrationType, int>();
        foreach (var lvl in SonoLoopManager.instance.AttenuationValues)
            currentVol.Add(lvl.Key, lvl.Value);

        iterations = 0;
        currentDbList = new Dictionary<SonoLoopManager.CalibrationType, List<float>>();
        currentDbList.Add(SonoLoopManager.CalibrationType.PinkNoise, new List<float>());
        currentDbList.Add(SonoLoopManager.CalibrationType.PureTone, new List<float>());
        currentDbList.Add(SonoLoopManager.CalibrationType.WarbleTone, new List<float>());
        currentDbList.Add(SonoLoopManager.CalibrationType.HINT, new List<float>());
        currentDbList.Add(SonoLoopManager.CalibrationType.QuickSIN, new List<float>());
        TestManager.instance.returnButton.interactable = false;
        TestManager.instance.proceedButton.interactable = false;

        TestManager.instance.inProgressText.color = Color.red;
        TestManager.instance.inProgressText.fontSize = 60;
        TestManager.instance.inProgressText.text = "CALIBRATION IN PROGRESS";
        TestManager.instance.inProgressText.enabled = true;

        StartCoroutine(TimedSequence_Coroutine());
    }
    IEnumerator InitializeSequence_Coroutine()
    {

        StartCoroutine(Communicate_Coroutine(EnableWindowsAudio, SonoLoopManager.CalibrationType.PinkNoise));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Communicate_Coroutine(DisableWindowsAudio, SonoLoopManager.CalibrationType.PinkNoise));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Communicate_Coroutine(SetMuteOff, SonoLoopManager.CalibrationType.PinkNoise));
        yield return new WaitForSeconds(0.1f);
    }
    IEnumerator TimedSequence_Coroutine()
    {
        StartCoroutine(InitializeSequence_Coroutine());
        yield return new WaitForSeconds(0.3f);
        StartCoroutine(ChangeVolume_CoroutineDirect(185));
        yield return new WaitForSeconds(0.1f);
        StartCoroutine(Communicate_Coroutine(GetCurrentVolume, SonoLoopManager.CalibrationType.PinkNoise));
        yield return new WaitForSeconds(1f);
        StartCoroutine(CalibrationIterator());
    }
    IEnumerator CalibrationIterator()
    {
        if (iterations >= 4)
        {
            yield return new WaitForEndOfFrame();
            WriteSettingsFile();
        }
        else
        {
            var spkConfig = 0;
            var numSamples = 60;// 15;
            var pauseBetweenSetAndPlay = 0.5f;
            var pauseBetweenPlayAndGetVolume = 1.5f;
            var pauseBetweenSamples = 0.05f;
            var betweenPause = 5.0f - (pauseBetweenPlayAndGetVolume + (numSamples * pauseBetweenSamples));
            var keys = new List<SonoLoopManager.CalibrationType>(SonoLoopManager.instance.AttenuationValues.Keys);
            foreach (var caliType in keys)
            {
                Debug.Log(caliType.ToString());
                StartCoroutine(ChangeVolume_Coroutine(caliType));
                yield return new WaitForSeconds(pauseBetweenSetAndPlay);

                PlaySound(caliType);
                yield return new WaitForSeconds(pauseBetweenPlayAndGetVolume);

                for (int i = 0; i < numSamples; i++)
                {
                    StartCoroutine(GetCurrentAndMaxLevels_Coroutine(caliType, spkConfig));
                    yield return new WaitForSeconds(pauseBetweenSamples);
                }
                AverageAttnValues(caliType);
                yield return new WaitForSeconds(betweenPause + 1.0f); 
            }
            TestManager.instance.SpeakerManagers[0].StopPlaying();
            yield return new WaitForSeconds(0.1f);

            iterations++;
            StartCoroutine(CalibrationIterator()); 
            yield return new WaitForEndOfFrame();
        }
    }
    IEnumerator ChangeVolume_CoroutineDirect(int attn)
    {
        var cmd = SetVolume_AttnValue.Replace("*", attn.ToString());
        var url = baseUrl + cmd;
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
        }
    }
    IEnumerator ChangeVolume_Coroutine(SonoLoopManager.CalibrationType type)
    {
        var eqStr = SetCurrentCalibrationModePure;
        switch (type)
        {
            case SonoLoopManager.CalibrationType.PinkNoise:
            case SonoLoopManager.CalibrationType.HINT:
            case SonoLoopManager.CalibrationType.QuickSIN:
                eqStr = SetCurrentCalibrationModePink;
                break;
            case SonoLoopManager.CalibrationType.WarbleTone:
                eqStr = SetCurrentCalibrationModeWarble;
                break;
            default:
                break;
        }
        // Set the calibration mode
        StartCoroutine(Communicate_Coroutine(eqStr, type));
        yield return new WaitForSeconds(0.1f);
        var attnVals = SonoLoopManager.instance.AttenuationValues;
        var attn =  attnVals[type];
        var cmd = SetVolume_AttnValue.Replace("*", attn.ToString());
        var url = baseUrl + cmd;
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
            MessageRecord.Add("SUCCESS," + url + "," + type.ToString());
        }
        else
        {
            var jsonString = www.downloadHandler.text;
            MessageRecord.Add("FAIL," + url + "," + type.ToString() + "," + jsonString);
            yield return new WaitForEndOfFrame();
            StartCoroutine(Communicate_Coroutine(GetCurrentVolume, type));
        }
        yield return new WaitForEndOfFrame();
    }
    IEnumerator GetCurrentAndMaxLevels_Coroutine(SonoLoopManager.CalibrationType type, int spkConfig)
    {
        var url = baseUrl + GetCurrentAndMax;
        //Debug.Log(url);
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.result != UnityWebRequest.Result.Success)
        {
        }
        else
        {
            var jsonString = www.downloadHandler.text;
            MessageRecord.Add("TEST," + url + "," + type.ToString() + "," + jsonString);
            var info = JSON.Parse(jsonString);
            float current_dB_Reading;
            //if (!float.TryParse(info["max"].ToString(), out current_dB_Reading))
            if (!float.TryParse(info["current"].ToString(), out current_dB_Reading))
                current_dB_Reading = 80;
            currentDbList[type].Add(current_dB_Reading);
        }
        yield return new WaitForEndOfFrame();
    }
    public void AverageAttnValues(SonoLoopManager.CalibrationType type)
    {
        float current_dB_Reading = 0;
        var list = currentDbList[type];
        for (int i = 0; i < list.Count; i++)
        {
            current_dB_Reading += list[i];
        }
        if (list.Count > 1)
            current_dB_Reading /= list.Count;

        float dB_diff = 80.0f - current_dB_Reading;
        float val_diff = -dB_diff / 0.375f;
        var corrected = (int)(currentVol[type] - val_diff);
        if (corrected > 255) corrected = 255;
        if (corrected < 0) corrected = 0;
        SonoLoopManager.instance.AttenuationValues[type] = (int)corrected;
        currentDbList[type] = new List<float>();
    }
    IEnumerator Communicate_Coroutine(string _address, SonoLoopManager.CalibrationType _caliType)
    {
        var url = baseUrl + _address; 
        MessageRecord.Add("TRYING," + url + ", ");
        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest(); 
        if (www.result != UnityWebRequest.Result.Success)
        {
            MessageRecord.Add("FAIL," + url + "," + _caliType.ToString());
        }
        else
        {
            var jsonString = www.downloadHandler.text;
            var info = JSON.Parse(jsonString);
            if (_address == GetCurrentVolume)
            {
                int _currentVol;
                if (int.TryParse(info["value"].ToString(), out _currentVol))
                    currentVol[_caliType] = _currentVol;
            }
            MessageRecord.Add("SUCCESS," + url + "," + _caliType.ToString() + "," + jsonString);
        }
        yield return new WaitForEndOfFrame();
    }
    void WriteSettingsFile()
    {
        try
        {
            var writer = new System.IO.StreamWriter(SonoLoopManager.instance.settingsFilename);
            var txt = "";
            if (SonoLoopManager.instance.AttenuationValues != null)
            {
                var val = SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.PinkNoise];
                txt = val.ToString("f5") + ",";

                val = SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.HINT];
                txt += val.ToString("f5") + ",";

                val = SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.QuickSIN];
                txt += val.ToString("f5") + ",";

                val = SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.PureTone];
                txt += val.ToString("f5") + ",";

                val = SonoLoopManager.instance.AttenuationValues[SonoLoopManager.CalibrationType.WarbleTone];
                txt += val.ToString("f5") + ",";

                txt += ipStr;
            }
            else txt = "Communication Failure. Did not retrieve volume levels";
            writer.WriteLine(txt);
            writer.Close();
        }
        catch
        {
            MenuManager.instance.commStatusText.text = "Settings file is in use. Faied to write.";
        }
        TestManager.instance.returnButton.interactable = true;
        TestManager.instance.inProgressText.color = new Color(0.3f, 1.0f, 0.4f);
        TestManager.instance.inProgressText.fontSize = 72;
        TestManager.instance.inProgressText.text = "CALIBRATION COMPLETE";

        MessageRecord.Add("CALIBRATION COMPLETE");
        PlaySound(SonoLoopManager.CalibrationType.Complete);
    }
    void WriteReports()
    {
        try
        {
            var writer = new System.IO.StreamWriter(Application.dataPath + "/Message Record.csv");
            foreach (var row in MessageRecord)
            {
                writer.WriteLine(row);
            }
            writer.Close();
        }
        catch
        {
            MenuManager.instance.commStatusText.text = "Settings file is in use. Faied to write.";
        }
    }
    public IEnumerator SetupFromSettings()
    {
        StartCoroutine(InitializeSequence_Coroutine());
        yield return new WaitForSeconds(0.3f);

        SceneManager.LoadScene("Menu");
        yield return new WaitForEndOfFrame();
    }

}
