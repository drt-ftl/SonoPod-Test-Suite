using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeakerButton : MonoBehaviour
{
    public ControlPad controlPad;
    public SonoLoopManager.SpeakerPosition position;
    bool isOnThis = false;
    bool hovered = false;
    bool clicking = false;
    bool clickable = false;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Renderer>().material.color = controlPad.speakerOff[controlPad.RingIndex];
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.Free
            || TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.FreeFieldLocalization
            || TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT)
            clickable = true;
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT)
        {
            //if (TestManager.instance.HINT_Bg_List.Contains(position))
                GetComponent<Renderer>().material.color = controlPad.speakerOn[0];
            if (position == SonoLoopManager.SpeakerPosition.CEN)
                GetComponent<Renderer>().material.color = controlPad.speakerClick[1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT && controlPad.RingIndex == 0) return;
        try
        {
            if (TestManager.instance == null) return;
            if (TestManager.instance.SpeakerManagers == null || TestManager.instance.SpeakerManagers.Count < 2) return;
            var _this = controlPad.RingIndex == 0 ? 0 : 1;
            var _that = _this == 0 ? 1 : 0;
            isOnThis = TestManager.instance.SpeakerManagers[_this].Speakers[position].isOn;
            var isOnThat = TestManager.instance.SpeakerManagers[_that].Speakers[position].isOn;
            if (position == TestManager.instance.SpeakerManagers[controlPad.RingIndex].CurrentPosition)
            {
                GetComponent<Renderer>().material.color = controlPad.speakerClick[controlPad.RingIndex];
            }
            else if (isOnThis)
            {
                var c = controlPad.speakerOn[_this];
                if (!hovered && !clicking)
                    GetComponent<Renderer>().material.color = c;
            }
            else if (isOnThat)
            {
                var c = controlPad.speakerOn[_that];
                GetComponent<Renderer>().material.color = c;
            }
            else GetComponent<Renderer>().material.color = controlPad.speakerOff[_this];
        }
        catch { }

    }
    private void OnMouseEnter()
    {
        if (!clickable || !isOnThis || SonoLoopManager.instance == null) return;
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT && controlPad.RingIndex == 0) return;
        hovered = true;
        GetComponent<Renderer>().material.color = controlPad.speakerHover[controlPad.RingIndex];
    }
    private void OnMouseExit()
    {
        hovered = false;
        clicking = false;
        if (!clickable || !isOnThis || SonoLoopManager.instance == null) return;
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT && controlPad.RingIndex == 0) return;
        if (TestManager.instance.SpeakerManagers[controlPad.RingIndex].CurrentPosition != position)
            GetComponent<Renderer>().material.color = controlPad.speakerOn[controlPad.RingIndex];
        else
            GetComponent<Renderer>().material.color = controlPad.speakerClick[controlPad.RingIndex];

    }
    private void OnMouseDown()
    {
        clicking = true;
        if (!clickable) return;
        if (!isOnThis || SonoLoopManager.instance == null) return;
        var ringIndex = controlPad.RingIndex;
        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT && ringIndex == 0) return;
        
        GetComponent<Renderer>().material.color = controlPad.speakerClick[ringIndex];
        var oldPosition = TestManager.instance.SpeakerManagers[ringIndex].CurrentPosition;
        TestManager.instance.SpeakerManagers[ringIndex].CurrentPosition = position;

        TestManager.instance.SpeakerManagers[ringIndex].Speakers[oldPosition].audioSource.volume = 0;
        TestManager.instance.SpeakerManagers[ringIndex].Speakers[position].audioSource.volume = TestManager.instance.SpeakerManagers[ringIndex].SpeakerRingVolume;

    }
    private void OnMouseUp()
    {
        clicking = false;
    }
}
