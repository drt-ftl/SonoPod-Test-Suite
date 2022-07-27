using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IP_Window : MonoBehaviour
{
    public TMP_InputField IP_Input;
    public TMP_Text IP_Output;
    private void Start()
    {
        Begin();
    }
    public void Begin()
    {
        IP_Input.text = Communicator.instance.ipStr;
        IP_Output.text = Communicator.instance.baseUrl;
    }
    public void OnValueChange()
    {
        IP_Output.text = "http://" + IP_Input.text + ":8000";
    }

    public void SetValue()
    {
        Communicator.instance.ipStr = IP_Input.text;
        DestroyIt();
    }
    public void Cancel()
    {
        DestroyIt();
    }
    void DestroyIt()
    {
        transform.parent.gameObject.SetActive(false);
    }
}
