using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ControlPad : MonoBehaviour
{
    public static ControlPad instance;
    public int RingIndex;           // < 0 means variable
    public TMP_Text display; 
    public Color color;
    public Color hoverColor;
    public Color clickColor;
    public List<Color> speakerOff;
    public List<Color> speakerOn;
    public List<Color> speakerHover;
    public List<Color> speakerClick;
    public Sprite playSprite;
    public Sprite pauseSprite;
    public List<SonoLoopButton> buttons;
    public SpriteRenderer border;
    public Color primaryBorderColor;
    public Color secondaryBorderColor;

    void Start()
    {
        instance = this;
        border.material.color = primaryBorderColor;
        if (RingIndex < 0) RingIndex = 0;
        if (RingIndex > 2) RingIndex = 2;
        TestManager.instance.PlayPauseClip_Handler += Instance_PlayPauseClip;
    }

    private void Instance_PlayPauseClip(object sender, System.EventArgs e)
    {
        PlayPause();
    }
    public void PlayPause()
    {
        if (TestManager.instance.isPaused)
            GetComponent<SpriteRenderer>().sprite = playSprite;
        else
            GetComponent<SpriteRenderer>().sprite = pauseSprite;
    }

    void Update()
    {
        if (TestManager.instance == null) return;
        float vol0 = TestManager.instance.SpeakerManagers[0].SpeakerRingVolume;
        vol0 = SonoLoopManager.instance.Linear_To_dBSPL(vol0);
        var speakerRing0 = TestManager.instance.SpeakerManagers[0];
        var txt = 
          "Channel: Primary\n"
        + "Clip:    " + speakerRing0?.Clips[speakerRing0.CurrentClipNumber].name + "\n"
        + "Level:   " + vol0.ToString("f1").ToString() + " dB SPL";
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT
            || TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.QuickSIN
            || TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.Free)
        {
            float vol1 = TestManager.instance.SpeakerManagers[1].SpeakerRingVolume;
            vol1 = SonoLoopManager.instance.Linear_To_dBSPL(vol1);

            var speakerRing1 = TestManager.instance.SpeakerManagers[1];

            txt += "\n\n\n"
            + "Channel: Secondary\n"
            + "Clip:    " + speakerRing1?.Clips[speakerRing1.CurrentClipNumber].name + "\n"
            + "Level:   " + vol1.ToString("f1").ToString() + " dB SPL";
        }
        display.text = txt;
        if (TestManager.instance.CtrlDown) RingIndex = 1;
        else RingIndex = 0;

        switch(RingIndex)
        {
            case 1:
                border.color = secondaryBorderColor;
                break;
            default:
                border.color = primaryBorderColor;
                break;
               
        }
    }
}
