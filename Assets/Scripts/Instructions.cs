using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Instructions
{
    static string start = "\n<u><b>Instructions</u></b>\n\n";
    static string end = "\n\n\n\n\n\n";
    public static string[] GetInfo(SonoLoopManager.SonoLoopTestType testType)
    {
        var info = new string[2];
        switch(testType)
        {
            case SonoLoopManager.SonoLoopTestType.FreeFieldLocalization:
                info = FreeFieldLocalization();
                break;
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_PT:
                info = HearingThreshold_PT();
                break;
            case SonoLoopManager.SonoLoopTestType.HearingThreshold_WT:
                info = HearingThreshold_WT();
                break;
            case SonoLoopManager.SonoLoopTestType.Free:
                info = FREE();
                break;
            case SonoLoopManager.SonoLoopTestType.QuickSIN:
                info = QuickSIN();
                break;
            case SonoLoopManager.SonoLoopTestType.SpeechReceptionThreshold:
                info = SpeechReceptionThreshold();
                break;
            case SonoLoopManager.SonoLoopTestType.HINT:
                info = HINT();
                break;
            case SonoLoopManager.SonoLoopTestType.Calibration:
                info = Calibration();
                break;
            default:
                break;
        }
        info[1] += end;
        return info;
    }
    public static string[] Calibration()
    {
        var info = new string[2];
        info[0] = "Calibration";
        info[1] = "";

        return info;
    }
    public static string[] HearingThreshold_PT()
    {
        var info = new string[2];
        info[0] = "Hearing Threshold (PT)";
        info[1] = start + "Click the Play button to play a 3 second pure tone.\n";
        info[1] += "Use the arrow buttons to select tone.\n";
        info[1] += "Use the +/- buttons to adjust volume.\n";

        return info;
    }
    public static string[] HearingThreshold_WT()
    {
        var info = new string[2];
        info[0] = "Hearing Threshold (WT)";
        info[1] = start + "Click the Play button to play a 3 second warble tone.\n";
        info[1] += "Use the arrow buttons to select tone.\n";
        info[1] += "Use the +/- buttons to adjust volume.\n";

        return info;
    }
    public static string[] SpeechReceptionThreshold()
    {
        var info = new string[2];
        info[0] = "Speech Reception Threshold";
        info[1] = start + "Click the Play button to play a two syllable word.\n";
        info[1] += "Use the arrow buttons to select word.\n";
        info[1] += "Use the +/- buttons to adjust volume.\n";

        return info;
    }
    public static string[] QuickSIN()
    {
        var info = new string[2];
        info[0] = "QuickSIN";
        info[1] = start + "Click the Play button to play a series of sentences, with muffled conversation in the background.\n";
        info[1] += "Use the Play/Pause buttn to stop and start between sentences.\n";
        info[1] += "Use the arrow buttons to select sentence series.\n";
        info[1] += "Use the +/- buttons to adjust volume of sentences.\n";
        info[1] += "Pressing Ctrl allows for adjustment of background volume.\n";

        return info;
    }
    public static string[] FreeFieldLocalization()
    {
        var info = new string[2];
        info[0] = "Free-Field Localization";
        info[1] = start + "Provide the test subject with an image of the Sonoloop speaker mapping.\n";
        info[1] += "Click any Speaker Button to select.\n";
        info[1] += "Click the Play to play a 3 second pure tone from the selected speaker.\n";
        info[1] += "Ask the subject to point to the speaker on the image.\n";
        info[1] += "Use the arrow buttons to select tone.\n";
        info[1] += "Use the +/- buttons to adjust volume.\n";

        return info;
    }
    public static string[] FREE()
    {
        var info = new string[2];
        info[0] = "Free Roam";
        info[1] = start + "Use the dropdown menu to select the sound bank.\n";
        info[1] += "Use the arrow buttons to select tone.\n";
        info[1] += "Use the +/- buttons to adjust volume.\n";

        return info;
    }
    public static string[] HINT()
    {
        var info = new string[2];
        info[0] = "HINT";
        info[1] = start + "Click the Play button to play a series of sentences, with noise in the background.\n";
        info[1] += "Press Ctrl and click one of the available speaker buttons to select the noise source speaker. Sentences are always from the front speaker.\n";
        info[1] += "Use the arrow buttons to select sentence series.\n";
        info[1] += "Use the +/- buttons to adjust volume of sentences.\n";
        info[1] += "Pressing Ctrl allows for adjustment of background volume.\n";

        return info;
    }
}
