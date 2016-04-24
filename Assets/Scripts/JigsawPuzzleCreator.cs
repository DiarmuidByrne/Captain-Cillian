using UnityEngine;
using PuzzleMaker;
using System.Collections;

/// <summary>
/// Creates puzzle pieces for puzzle and assign them their images according to their position
/// in puzzle
/// </summary>
public class JigsawPuzzleCreator : MonoBehaviour
{

#region "Variables"

    /// <summary>
    /// User provided puzzle image
    /// </summary>
    public Texture2D PuzzleImage;
    /// <summary>
    /// If set to true JigsawPuzzleCreator will load selected file to create new instance of puzzle maker.
    /// File is loaded from streamming assets folder of the project
    /// </summary>
    [HideInInspector]
    public bool UseFilePath = false;
    /// <summary>
    /// Used by editor to store selected .pm file path
    /// </summary>
    [HideInInspector]
    public string PMFilePath = "";
    /// <summary>
    /// Used by editor to store index of current selected file
    /// </summary>
    [HideInInspector]
    public int _selectedFileIndex = 0;
    /// <summary>
    /// Joints mask images use to create puzzle pieces
    /// </summary>
    public Texture2D[] JointMaskImage;

    /// <summary>
    /// No of pieces in row / total columns  in this puzzle
    /// </summary>
    [Range(3, 10)]
    public int PiecesInRow = 3;
    /// <summary>
    /// No of pieces in col / total rows  in this puzzle
    /// </summary>
    [Range(3, 10)]
    public int PiecesInCol = 3;

    /// <summary>
    /// If set to true user is displayed with a complete background puzzle image for help
    /// </summary>
    public bool DisplayCompletedImage = true;


    /// <summary>
    /// Holds created PuzzlePieceMaker instance
    /// </summary>
    protected PuzzlePieceMaker _puzzlePieceMaker;
    /// <summary>
    /// Holds created puzzle pieces instance
    /// </summary>
    protected GameObject[] _puzzlePieces;     

    /// <summary>
    /// Holds instance for actual image mesh to be displayed on puzzle completion
    /// </summary>
    protected GameObject _actualImageGameObject;

    /// <summary>
    /// Holds main instance of puzzle piece gameobject, from which pieces are instantiated
    /// </summary>
    protected GameObject _puzzlePieceInst;

    /// <summary>
    /// Holds camera transform
    /// </summary>
    protected Transform parentCamTransform;
    /// <summary>
    /// Holds camera component from prefab
    /// </summary>
    protected Camera parentCam;


    /// <summary>
    /// Used to check whether file loading is completed so that other operations can continue
    /// </summary>
    protected bool _isFileLoadingComplete = false;

#endregion



    /// <summary>
    /// Returns piece width in world units
    /// </summary>
    public float PieceWidthInWorld
    {
        get { return 1f; }
    }

    /// <summary>
    /// Returns piece height in world units
    /// </summary>
    public float PieceHeightInWorld
    {
        get { return ((float)PuzzleImage.height / (float)PuzzleImage.width); }
    }



    /// <summary>
    /// Generates jigsaw puzzle and initialize pieces with data according to user selected options
    /// </summary>
    /// <returns>Coroutine</returns>
    public virtual IEnumerator Start()
    {
        if (!UseFilePath)
        {
            _puzzlePieceMaker = new PuzzlePieceMaker(PuzzleImage, JointMaskImage, PiecesInRow, PiecesInCol);

            //Enable below code and provide a file path to save data created by _puzzlemaker class using provided image
            //_PuzzleMaker.SaveData("File Path here e.g c:\\Images\\Temp.pm");
        }
        else
        {

            if (PuzzlePieceMaker.IsPMFileSupportedPlatform())
            {
                try
                {
                    _puzzlePieceMaker = new PuzzlePieceMaker(PMFilePath);
                    PiecesInCol = _puzzlePieceMaker.NoOfPiecesInCol;
                    PiecesInRow = _puzzlePieceMaker.NoOfPiecesInRow;
                    _isFileLoadingComplete = true;
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Please check if you have provided correct file path : " + e.Message);
                    Destroy(gameObject);
                    yield break;
                }

            }
            else if (Application.platform == RuntimePlatform.WebGLPlayer ||
                        Application.platform == RuntimePlatform.WindowsWebPlayer)
            {
                WebPmFileLoader Temp = GetComponent<WebPmFileLoader>();

                if (Temp != null)
                {
                    yield return StartCoroutine(Temp.LoadFileToStream(PMFilePath));

                    if (Temp.PMFileStream != null)
                    {
                        _puzzlePieceMaker = new PuzzlePieceMaker(Temp.PMFileStream);
                        PiecesInCol = _puzzlePieceMaker.NoOfPiecesInCol;
                        PiecesInRow = _puzzlePieceMaker.NoOfPiecesInRow;
                        _isFileLoadingComplete = true;
                    }
                    else
                    {
                        Debug.LogError("File loading failed");
                        yield break;
                    }
                }
                else
                {
                    Debug.LogError("Prefab not set correctly");
                }
            }


        }

        
        yield return new WaitForEndOfFrame();

        
        //Get main instance of puzzle piece
        _puzzlePieceInst = gameObject.transform.FindChild("Piece").gameObject;

        //Set puzzle image to Puzzle piece instance
        _puzzlePieceInst.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.PuzzleImage;

        //Seperate from cam for cam resizing to adjust whole puzzle in cam view
        parentCamTransform = gameObject.transform.parent;
        parentCam = parentCamTransform.GetComponent<Camera>();
        gameObject.transform.parent = null;


        //Set background
        if (DisplayCompletedImage)
        {
            GetComponent<Renderer>().enabled = true;
            GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.CreatedBackgroundImage;
        }

        //Resize gameobject to match size for pieces
        transform.localScale = new Vector3((float)PiecesInRow, (PiecesInCol * ((float)PuzzleImage.height / (float)PuzzleImage.width)), 1f);


        //Translate to zero
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.Euler(Vector3.zero);

        
        //Initialize all pieces and joints resize them and place them on screen randomly
        _puzzlePieces = new GameObject[PiecesInRow * PiecesInCol];

        
        //Scale piece according to puzzle
        float PieceScaleX = 1;
        float PieceScaleY = 1 * ((float)_puzzlePieceMaker.PuzzleImageHeight / (float)_puzzlePieceMaker.PuzzleImageWidth);
        _puzzlePieceInst.transform.localScale = new Vector3(PieceScaleX, PieceScaleY, 1);


        //Create joints instances for each side from _puzzle piece
        GameObject _leftJointInst = Object.Instantiate(_puzzlePieceInst) as GameObject;
        Component[] AllCompoenents = _leftJointInst.GetComponents<Component>();
        foreach (Component item in AllCompoenents)
            if (!(item is Renderer || item is Transform || item is MeshFilter))
                Destroy(item);

        yield return new WaitForEndOfFrame();

        GameObject _rightJointInst = Object.Instantiate(_leftJointInst) as GameObject;
        GameObject _topJointInst = Object.Instantiate(_leftJointInst) as GameObject;
        GameObject _bottomJointInst = Object.Instantiate(_leftJointInst) as GameObject;


        //Initialize joints instances
        _leftJointInst.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.LeftJointsMaskImage;
        _rightJointInst.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.RightJointsMaskImage;
        _topJointInst.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.TopJointsMaskImage;
        _bottomJointInst.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.BotJointsMaskImage;

        //Name joints
        _leftJointInst.name = "LeftJoint";
        _rightJointInst.name = "RightJoint";
        _topJointInst.name = "TopJoint";
        _bottomJointInst.name = "BottomJoint";

        //Position joints
        float JointWidth = _leftJointInst.transform.localScale.x;
        float JointHeight = _leftJointInst.transform.localScale.y;
        

        //Make joints child of piece
        _leftJointInst.transform.parent = _puzzlePieceInst.transform;
        _leftJointInst.transform.localScale = Vector3.one;
        _leftJointInst.SetActive(true);

        _rightJointInst.transform.parent = _puzzlePieceInst.transform;
        _rightJointInst.transform.localScale = Vector3.one;
        _rightJointInst.SetActive(true);

        _topJointInst.transform.parent = _puzzlePieceInst.transform;
        _topJointInst.transform.localScale = Vector3.one;
        _topJointInst.SetActive(true);

        _bottomJointInst.transform.parent = _puzzlePieceInst.transform;
        _bottomJointInst.transform.localScale = Vector3.one;
        _bottomJointInst.SetActive(true);


        //Position joints
        _leftJointInst.transform.localPosition = new Vector3(-1, 0, 0);
        _rightJointInst.transform.localPosition = new Vector3(1, 0, 0);
        _topJointInst.transform.localPosition = new Vector3(0, 1, 0);
        _bottomJointInst.transform.localPosition = new Vector3(0, -1, 0);
        

        //Initialize pieces
        for (int RowTrav = 0; RowTrav < PiecesInCol; RowTrav++)
        {
            for (int ColTrav = 0; ColTrav < PiecesInRow; ColTrav++)
            {
                GameObject Temp = Object.Instantiate(_puzzlePieceInst) as GameObject;
                
                
                //Name for this piece
                Temp.name = "Piece" + ((RowTrav * PiecesInRow) + ColTrav).ToString();
                
                SetPuzzlePieceUV(Temp, ColTrav, RowTrav, PiecesInCol, PiecesInRow);

                //Enable piece
                Temp.SetActive(true);

                SPieceInfo ThisPieceData = _puzzlePieceMaker._CreatedPiecesData[RowTrav,ColTrav];
                bool TempOut = false;

                bool HaveTopJoint = ThisPieceData.HaveJoint(EJointPosition.Top);
                if (HaveTopJoint) 
                    HaveTopJoint = ThisPieceData.GetJoint(EJointPosition.Top, out TempOut).JointType != EJointType.Female;

                bool HaveBotJoint = ThisPieceData.HaveJoint(EJointPosition.Bottom);
                if (HaveBotJoint)
                    HaveBotJoint = ThisPieceData.GetJoint(EJointPosition.Bottom, out TempOut).JointType != EJointType.Female;

                bool HaveLeftJoint = ThisPieceData.HaveJoint(EJointPosition.Left);
                if (HaveLeftJoint)
                    HaveLeftJoint = ThisPieceData.GetJoint(EJointPosition.Left, out TempOut).JointType != EJointType.Female;

                bool HaveRightJoint = ThisPieceData.HaveJoint(EJointPosition.Right);
                if (HaveRightJoint)
                    HaveRightJoint = ThisPieceData.GetJoint(EJointPosition.Right, out TempOut).JointType != EJointType.Female;



                //Set joints uvs
                Transform LeftJoint = Temp.transform.FindChild("LeftJoint");
                if (HaveLeftJoint && ColTrav > 0)
                        SetPuzzlePieceUV(LeftJoint.gameObject, ColTrav - 1, RowTrav, PiecesInCol, PiecesInRow);
                else
                    LeftJoint.gameObject.SetActive(false);


                Transform RightJoint = Temp.transform.FindChild("RightJoint");
                if (HaveRightJoint && ColTrav < PiecesInCol - 1)
                    SetPuzzlePieceUV(RightJoint.gameObject, ColTrav + 1, RowTrav, PiecesInCol, PiecesInRow);
                else
                    RightJoint.gameObject.SetActive(false);


                Transform TopJoint = Temp.transform.FindChild("TopJoint");
                if ( HaveTopJoint && RowTrav < PiecesInRow - 1)
                    SetPuzzlePieceUV(TopJoint.gameObject, ColTrav, RowTrav + 1, PiecesInCol, PiecesInRow);
                else
                    TopJoint.gameObject.SetActive(false);


                Transform BotJoint = Temp.transform.FindChild("BottomJoint");
                if ( HaveBotJoint && RowTrav >0)
                    SetPuzzlePieceUV(BotJoint.gameObject, ColTrav, RowTrav - 1, PiecesInCol, PiecesInRow);
                else
                    BotJoint.gameObject.SetActive(false);


                _puzzlePieces[(RowTrav * PiecesInRow) + ColTrav] = Temp;
            }
        }

    }


    /// <summary>
    /// Convert position of element in single dimensional array to Row & Col in multidimensional array
    /// </summary>
    /// <param name="ElementPosInArray">Position of element inside one dimensional array ,minimum value is 0</param>
    /// <param name="TotalRows">Total rows in multidimensional array ,minimum value is 1</param>
    /// <param name="TotalCols">Total cols in multidimensional array ,minimum value is 1</param>
    /// <param name="ElementRow">Output calculated Row position in multidimensional array ,minimum value is 0</param>
    /// <param name="ElementCol">Output calculated Col position in multidimensional array ,minimum value is 0</param>
    /// <returns>Whether calculation is successfull or not</returns>
    public static bool ArrayPosToRC(int ElementPosInArray, int TotalRows,
                                        int TotalCols, out int ElementRow, out int ElementCol)
    {
        ElementRow = -1;
        ElementCol = -1;

        if (TotalRows < 1 || TotalCols < 1 || ElementPosInArray < 0 || ElementPosInArray > TotalRows * TotalCols - 1)
            return false;

        ElementRow = (ElementPosInArray / TotalCols);

        ElementCol = ElementPosInArray % TotalCols;


        //Debug.Log("ElemtRow : " + ElementRow + " , ElementCol : " + ElementCol + " , ElementPosInArray : " + ElementPosInArray);
        return true;
    }
    
    /// <summary>
    /// Sets pieces instance uvs according to their position in puzzle
    /// </summary>
    /// <param name="Piece">Piece instance for uv setting</param>
    /// <param name="PieceXPositioninPuzzle">X Position of piece in puzzle</param>
    /// <param name="PieceYPositionInPuzzle">Y Posiiton of piece in puzzle</param>
    /// <param name="TotalRowsInPuzzle">No of rows in puzzle / total pieces in col</param>
    /// <param name="TotalColsInPuzzle">No of cols in puzzle / total pieces in rows</param>
    public static void SetPuzzlePieceUV(GameObject Piece, int PieceXPositioninPuzzle,
        int PieceYPositionInPuzzle, int TotalRowsInPuzzle, int TotalColsInPuzzle)
    {
        float PieceWidth = 1f / (float)TotalColsInPuzzle;
        float PieceHeight = 1f / (float)TotalRowsInPuzzle;

        //Set uv for this piece
        float StartX = PieceWidth * (float)PieceXPositioninPuzzle + 0.0012f * (float)TotalColsInPuzzle;
        float StartY = PieceHeight * (float)PieceYPositionInPuzzle + 0.0012f * (float)TotalRowsInPuzzle;
        float EndX = StartX + PieceWidth - 0.0024f * (float)TotalColsInPuzzle;
        float EndY = StartY + PieceHeight - 0.0024f * (float)TotalRowsInPuzzle;

        Vector2[] UVPoints = {new Vector2(StartX, StartY),
                                      new Vector2(EndX, EndY),
                                      new Vector2(EndX, StartY),
                                      new Vector2(StartX, EndY)
                                     };
        Piece.GetComponent<MeshFilter>().mesh.uv = UVPoints;
        
    }

    /// <summary>
    /// Disables all puzzle pieces
    /// </summary>
    protected void HideAllPieces()
    {
        for (int i = 0; i < _puzzlePieces.Length; i++)
        {
            _puzzlePieces[i].SetActive(false);
        }
    }

    /// <summary>
    /// Enables all puzzle pieces
    /// </summary>
    protected void ShowAllPieces()
    {
        for (int i = 0; i < _puzzlePieces.Length; i++)
        {
            _puzzlePieces[i].SetActive(true);
        }
    }

}
