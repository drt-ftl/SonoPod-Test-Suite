using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance;
    public GameObject IP_WindowObject;
    public Transform IP_Parent;
    public TMP_Text commStatusText;
    public TMP_Text versionText;
    public void Awake()
    {
        instance = this;
        if (SonoLoopManager.instance == null)
            SceneManager.LoadScene("Start");
        Communicator.instance.InitializeCommunicator();
        versionText.text = SonoLoopManager.instance.VERSION_NAME;
    }
    public void Do_HearingThreshold_PT()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.HearingThreshold_PT;
        SceneManager.LoadScene("Test");
    }
    public void Do_HearingThreshold_WT()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.HearingThreshold_WT;
        SceneManager.LoadScene("Test");
    }
    public void Do_PulsedWarble()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.PulsedWarble;
        SceneManager.LoadScene("Test");
    }
    public void Do_SpeechReceptionThreshold()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold;
        SceneManager.LoadScene("Test");
    }
    public void Do_QuickSIN()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.QuickSIN;
        SceneManager.LoadScene("Test");
    }
    public void Do_Free()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.Free;
        SceneManager.LoadScene("Test");
    }
    public void Do_FreeFieldLocalization()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.FreeFieldLocalization;
        SceneManager.LoadScene("Test");
    }
    public void Do_HINT()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.HINT;
        SceneManager.LoadScene("Test");
    }
    public void Do_Calibration()
    {
        SonoLoopManager.instance.TestType = SonoLoopManager.SonoLoopTestType.Calibration;
        SceneManager.LoadScene("Test");
    }
    public void Exit()
    {
        Application.Quit();
    }
    public void SetIP()
    {
        IP_Parent.gameObject.SetActive(true);
        IP_Parent.GetComponentInChildren<IP_Window>().Begin();
    }
}
