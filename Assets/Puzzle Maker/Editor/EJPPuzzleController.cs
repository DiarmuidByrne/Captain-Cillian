using UnityEngine;
using UnityEditor;
using System.Collections;

[CustomEditor(typeof(JPPuzzleController))]
public class EJPPuzzleController : Editor {

    string[] FileNames = null;

    public void OnEnable()
    {
        if (!System.IO.Directory.Exists(Application.streamingAssetsPath))
        {
            System.IO.Directory.CreateDirectory(Application.streamingAssetsPath);
            AssetDatabase.Refresh();
        }

        FileNames = System.IO.Directory.GetFiles(Application.streamingAssetsPath + "/", "*.pm");
    }

    public override void OnInspectorGUI()
    {

        JPPuzzleController myTarget = (JPPuzzleController)target;

        EditorGUILayout.Space();

        myTarget.UseFilePath = EditorGUILayout.Toggle("Use file", myTarget.UseFilePath);

        if (myTarget.UseFilePath)
        {
            //Populate files data in combobox
            if (FileNames == null)
            {
                EditorGUILayout.HelpBox("Currently no file present, please create file using PMFileCreator from Window->PuzzleMaker->CreatePMFile", MessageType.Error);
            }
            else if ( FileNames.Length == 0 )
            {
                EditorGUILayout.HelpBox("Currently no file present, please create file using PMFileCreator from Window->PuzzleMaker->CreatePMFile", MessageType.Error);
            }
            else if ( FileNames.Length > 0 ){
                
                GUIContent[] _contentList = new GUIContent[FileNames.Length];

                for (int i = 0; i < FileNames.Length; i++)
                {
                    _contentList[i] = new GUIContent( System.IO.Path.GetFileName( FileNames[i] ) );
                }

                myTarget._selectedFileIndex = EditorGUILayout.Popup(new GUIContent( "PM File" ), myTarget._selectedFileIndex, _contentList);

                myTarget.PMFilePath = FileNames[myTarget._selectedFileIndex];
            }

            
            EditorGUILayout.Space();
            myTarget.PieceJoinSensitivity = EditorGUILayout.Slider("Pieces Join Sensitivity",
                myTarget.PieceJoinSensitivity, 0.001f, 0.2f);

            EditorGUILayout.Space();
            myTarget.DisplayCompletedImage = EditorGUILayout.Toggle("Display Completed Image", myTarget.DisplayCompletedImage);

            EditorGUILayout.Space();
            myTarget.ActualImageOnPuzzleComplete = EditorGUILayout.Toggle("Actual Image On Puzzle Complete", myTarget.ActualImageOnPuzzleComplete);

            
        }
        else
        {
            if (myTarget.JointMaskImage == null)
            {
                EditorGUILayout.HelpBox("You should provide atleast 1 joint mask image", MessageType.Error);
            }
            else if ( myTarget.JointMaskImage.Length == 0 )
            {
                EditorGUILayout.HelpBox("You should provide atleast 1 joint mask image", MessageType.Error);
            }

            base.OnInspectorGUI();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("Leave empty if you dont want to play audio", MessageType.Info);
        myTarget.PieceJoinedSound = (AudioClip)EditorGUILayout.ObjectField("Piece Joined Sound ", myTarget.PieceJoinedSound, typeof(AudioClip));
        myTarget.PiecePickupSound = (AudioClip)EditorGUILayout.ObjectField("Piece Pickup Sound ", myTarget.PiecePickupSound, typeof(AudioClip));
        myTarget.PuzzleCompletionSound = (AudioClip)EditorGUILayout.ObjectField("Puzzle Completion Sound ", myTarget.PuzzleCompletionSound, typeof(AudioClip));
        myTarget.BackgroundMusic = (AudioClip)EditorGUILayout.ObjectField("Background Music ", myTarget.BackgroundMusic, typeof(AudioClip));

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        SerializedProperty sprop = serializedObject.FindProperty("OnPuzzleComplete");
        EditorGUILayout.PropertyField(sprop);

        sprop = serializedObject.FindProperty("OnPieceJoined");
        EditorGUILayout.PropertyField(sprop);

        serializedObject.ApplyModifiedProperties();

        EditorUtility.SetDirty(target);

    }

}
