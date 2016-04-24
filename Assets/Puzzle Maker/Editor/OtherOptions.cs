using UnityEngine;
using UnityEditor;
using System.Collections;

public class OtherOptions : MonoBehaviour {
    
    [MenuItem("Window/PuzzleMaker/Feedback")]
    static void GotoComments()
    {
        Application.OpenURL(@"https://docs.google.com/forms/d/1PGQGMRniAr69FgcohDa548DlZbdTPUgnNVDAUjXcLiM/viewform");
    }

    [MenuItem("Window/PuzzleMaker/GetStarted")]
    static void GotoDocumentation()
    {
        Application.OpenURL(@"https://docs.google.com/document/d/1H4gtLdkiHx_Im02EnbvT1sXIuxKAQOWCpYs97nOR8qQ/edit?usp=sharing");
    }

    [MenuItem("Window/PuzzleMaker/API Reference")]
    static void GotoTechnicalDoc()
    {
        Application.OpenURL(@"http://www.googledrive.com/host/0B2ho1QaIYqPlVEVMTHZpak5aQzg/");
    }


    [MenuItem("Window/PuzzleMaker/WebPreviews")]
    static void GotoWebPreviews()
    {
        Application.OpenURL(@"http://www.googledrive.com/host/0B2ho1QaIYqPlQjlDMHg0Y3l2SVE/");
    }

}
