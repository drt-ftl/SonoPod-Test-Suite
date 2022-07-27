using UnityEngine;

public class SonoLoopButton : MonoBehaviour
{
    public enum ButtonType { Play, Volume, Clip }
    public ButtonType buttonType;
    public float magnitude;
    ControlPad controlPad;
    private void Start()
    {
        controlPad = transform.GetComponentInParent<ControlPad>();
        GetComponent<Renderer>().material.color = controlPad.color;
    }
    void Update()
    {
        
    }
    private void OnMouseEnter()
    {
        GetComponent<Renderer>().material.color = controlPad.hoverColor;
    }
    private void OnMouseExit()
    {
        GetComponent<Renderer>().material.color = controlPad.color;
    }
    private void OnMouseDown()
    {
        if (SonoLoopManager.instance == null) return;
        GetComponent<Renderer>().material.color = controlPad.clickColor;

        if (buttonType == ButtonType.Play && magnitude == 0)
        {
            TestManager.instance.TestManager_PlayPause_Event();
            return;
        }
        if (buttonType != ButtonType.Clip)
        {
            TestManager.instance.SpeakerManagers[controlPad.RingIndex].SonoLoopButtonInput(this);
            return;
        }
        TestManager.instance.SpeakerManagers[controlPad.RingIndex].StopPlaying();
        TestManager.instance.SpeakerManagers[controlPad.RingIndex].SonoLoopButtonInput(this);

        if (TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.HINT || TestManager.instance.TestType == SonoLoopManager.SonoLoopTestType.QuickSIN)
        {
            var other = 0;
            if (controlPad.RingIndex == 0) other = 1;
            TestManager.instance.SpeakerManagers[other].StopPlaying();
            TestManager.instance.SpeakerManagers[other].SonoLoopButtonInput(this);
        }
    }
}
