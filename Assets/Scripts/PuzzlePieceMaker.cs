using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;


namespace PuzzleMaker
{

    /// <summary>
    /// Main class responsible for Loading, Saving and Creating of Puzzle data
    /// </summary>
    public class PuzzlePieceMaker
    {

        /// <summary>
        /// Holds pieces information for created puzzle
        /// </summary>
        public SPieceInfo[,] _CreatedPiecesData = null;
        

        private Texture2D[] _jointMask = null;

        private int _secretVersionNo = 6640331; //Use to identify correct version file
        

#region "Properties"

        private int _noOfPiecesInRow = 0;
        /// <summary>
        /// Returns total number of pieces in a row can also be called Total no of columns in puzzle.
        /// </summary>
        public int NoOfPiecesInRow
        {
            get { return _noOfPiecesInRow; }
        }


        private int _noOfPiecesInCol = 0;
        /// <summary>
        /// Returns total number of pieces in a column can also be called Total no of rows in puzzle.
        /// </summary>
        public int NoOfPiecesInCol
        {
            get { return _noOfPiecesInCol; }
        }


        private int _pieceWidthWithoutJoint = 0;
        /// <summary>
        /// Returns width of piece image
        /// </summary>
        public int PieceWidthWithoutJoint
        {
            get { return _pieceWidthWithoutJoint; }
        }


        private int _pieceHeightWithoutJoint = 0;
        /// <summary>
        /// Returns height of piece image
        /// </summary>
        public int PieceHeightWithoutJoint
        {
            get { return _pieceHeightWithoutJoint; }
        }


        private Texture2D _image = null;
        /// <summary>
        /// Returns copy of Puzzle Image with female joints drawn by Puzzle Maker.
        /// Use by pieces in puzzle
        /// </summary>
        public Texture2D PuzzleImage
        {
            //Return a copy of actual image
            get { return Object.Instantiate(_image) as Texture2D; }
        }

        private Texture2D _origionalImage = null;
        /// <summary>
        /// Returns copy of origional puzzle image provided by user
        /// </summary>
        public Texture2D PuzzleOrigionalImage
        {
            //Return a copy of actual image
            get { return Object.Instantiate(_origionalImage) as Texture2D; }
        }

        /// <summary>
        /// Returns create puzzle image for pieces width
        /// </summary>
        public int PuzzleImageWidth
        {
            get { return _image.width; }
        }

        /// <summary>
        /// Returns create puzzle image for pieces height
        /// </summary>
        public int PuzzleImageHeight
        {
            get { return _image.height; }
        }

        /// <summary>
        /// Returns copy joints masks images provided by user for creation of pieces
        /// </summary>
        public Texture2D[] JointMasks
        {
            get
            {
                Texture2D[] Temp = new Texture2D[_jointMask.Length];
                
                //Make copy of array to return so that noone can make changes inside
                for (int i = 0; i < Temp.Length; i++)
                    Temp[i] = Object.Instantiate(_jointMask[i]) as Texture2D;

                return Temp;
            }
        }

        private Texture2D _leftJointsMaskImage = null;
        /// <summary>
        /// Returns copy of LeftJointImage created by puzzle maker for joints of pieces
        /// </summary>
        public Texture2D LeftJointsMaskImage
        {
            get{return Object.Instantiate(_leftJointsMaskImage) as Texture2D;}
        }

        private Texture2D _rightJointsMaskImage = null;
        /// <summary>
        /// Returns copy of RightJointImage created by puzzle maker for joints of pieces
        /// </summary>
        public Texture2D RightJointsMaskImage
        {
            get{return Object.Instantiate(_rightJointsMaskImage) as Texture2D;}
        }

        private Texture2D _topJointsMaskImage = null;
        /// <summary>
        /// Returns copy of TopJointImage created by puzzle maker for joints of pieces
        /// </summary>
        public Texture2D TopJointsMaskImage
        {
            get{return Object.Instantiate(_topJointsMaskImage) as Texture2D;}
        }

        private Texture2D _botJointsMaskImage = null;
        /// <summary>
        /// Returns copy of BottomJointImage created by puzzle maker for joints of pieces
        /// </summary>
        public Texture2D BotJointsMaskImage
        {
            get{return Object.Instantiate(_botJointsMaskImage) as Texture2D;}
        }


        private Texture2D _createdBackgroundImage = null;
        /// <summary>
        /// Returns copy of grayscale background image created from origional image
        /// </summary>
        public Texture2D CreatedBackgroundImage
        {
            get
            {
                return Object.Instantiate(_createdBackgroundImage) as Texture2D;
            }
        }

#endregion


#region "Constructors"

        /// <summary>
        /// Creates PuzzlePieceMaker class from file at provides FilePath
        /// </summary>
        /// <param name="FilePath">Path to load .pm file from</param>
        public PuzzlePieceMaker(string FilePath)
        {
            LoadData(FilePath);
        }

        /// <summary>
        /// Creates PuzzlePieceMaker class from filestream
        /// </summary>
        /// <param name="PMFileStream">FileStream to load puzzle data from</param>
        public PuzzlePieceMaker(Stream PMFileStream)
        {
            LoadStreamData(PMFileStream);
        }

        /// <summary>
        /// Creates puzzle from provided data.
        /// </summary>
        /// <param name="Image">Main image for puzzle</param>
        /// <param name="JointMaskImage">Mask images for joints to be used to create joints for pieces in this puzzle</param>
        /// <param name="NoOfPiecesInRow">Total no of pieces in row of puzzle / total columns</param>
        /// <param name="NoOfPiecesInCol">Total no of pieces in Col of puzzle / total rows</param>
        public PuzzlePieceMaker(Texture2D Image, Texture2D[] JointMaskImage, int NoOfPiecesInRow, int NoOfPiecesInCol)
        {

#region "Arguments error checking"

            if (Image == null)
            {
                Debug.LogError("Error creating puzzle piece maker , Image cannot be null");
                return;
            }
            else if (NoOfPiecesInRow < 2 || NoOfPiecesInCol < 2)
            {
                Debug.LogError("Error creating puzzle piece maker , NoOfPiecesInRow or NoOfPiecesInCol cannot be less then 2");
                return;
            }

#endregion


            _image = Image;
            _origionalImage = Image;

            _jointMask = JointMaskImage;

            _noOfPiecesInRow = NoOfPiecesInRow;
            _noOfPiecesInCol = NoOfPiecesInCol;


            Texture2D _CreatedImageMask;

            _CreatedPiecesData = GenerateJigsawPieces(Image, JointMaskImage, out _CreatedImageMask,  NoOfPiecesInRow, NoOfPiecesInCol);


            _createdBackgroundImage = PuzzleImgToBackgroundImage(Image, _CreatedImageMask, _noOfPiecesInRow, NoOfPiecesInCol);
        }

        /// <summary>
        /// Returns whether file loading is supported on this platform.
        /// </summary>
        /// <returns>Returns true if current platform supports Puzzle maker PM file system</returns>
        public static bool IsPMFileSupportedPlatform()
        {
            if (Application.platform == RuntimePlatform.Android ||
                Application.platform == RuntimePlatform.WindowsEditor ||
                Application.platform == RuntimePlatform.WindowsPlayer ||
                Application.platform == RuntimePlatform.OSXEditor ||
                Application.platform == RuntimePlatform.OSXPlayer ||
                Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return true;
            }

            return false;
        }

#endregion


        
        /// <summary>
        /// Generate puzzle pieces from image.
        /// </summary>
        /// <param name="NoOfPiecesInRow">Total no of pieces in row of puzzle / total columns</param>
        /// <param name="NoOfPiecesInCol">Total no of pieces in Col of puzzle / total rows</param>
        /// <param name="Image">Main image for puzzle</param>
        /// <param name="JointMaskImage">Mask images for joints to be used to create joints for pieces in this puzzle</param>
        /// <param name="CreatedImageMask">Unknown</param>
        /// <returns>Returns generated pieces metadata</returns>
        private SPieceInfo[,] GenerateJigsawPieces(Texture2D Image, Texture2D[] JointMaskImage, out Texture2D CreatedImageMask, int NoOfPiecesInRow, int NoOfPiecesInCol)
        {

#region "Argument Error Checking"

            if (NoOfPiecesInRow < 2)
            {
                throw new System.ArgumentOutOfRangeException("NoOfPiecesInRow", "Argument should be greater then 1");
            }
            else if (NoOfPiecesInCol < 2)
            {
                throw new System.ArgumentOutOfRangeException("NoOfPiecesInCol", "Argument should be greater then 1");
            }
            else if (Image == null)
            {
                throw new System.ArgumentNullException("No texture2d assigned to this class");
            }

#endregion

            

            _noOfPiecesInRow = NoOfPiecesInRow;
            _noOfPiecesInCol = NoOfPiecesInCol;

            _origionalImage = Image;

            Color[][] _PuzzleImageTopJointMask = HelperMethods.Texture2DToColorArr(Image);
            Color[][] _PuzzleImageBotJointMask = HelperMethods.Texture2DToColorArr(Image);
            Color[][] _PuzzleImageLeftJointMask = HelperMethods.Texture2DToColorArr(Image);
            Color[][] _PuzzleImageRightJointMask = HelperMethods.Texture2DToColorArr(Image);
            Color[][] _PuzzleImage = HelperMethods.Texture2DToColorArr(Image);


            SPieceInfo[,] PiecesInformation = null;


            PiecesInformation = DrawCustomPieceJointsMask(ref _PuzzleImageTopJointMask, ref _PuzzleImageBotJointMask,
                    ref _PuzzleImageLeftJointMask, ref _PuzzleImageRightJointMask,
                    JointMaskImage,  out _pieceWidthWithoutJoint,
                    out _pieceHeightWithoutJoint,Image.width, Image.height, NoOfPiecesInCol, NoOfPiecesInRow);
            


            CreatedImageMask = HelperMethods.ColorArrToTexture2D(_PuzzleImageTopJointMask);

            //Create mask image for each side joints
            JointsMaskToJointsImage(ref _PuzzleImage, ref _PuzzleImageTopJointMask,ref  _PuzzleImageBotJointMask,
                ref _PuzzleImageLeftJointMask, ref _PuzzleImageRightJointMask,
                PieceWidthWithoutJoint, PieceHeightWithoutJoint, Image.width, Image.height);

            _topJointsMaskImage = HelperMethods.ColorArrToTexture2D(_PuzzleImageTopJointMask);
            _botJointsMaskImage = HelperMethods.ColorArrToTexture2D(_PuzzleImageBotJointMask);
            _leftJointsMaskImage = HelperMethods.ColorArrToTexture2D(_PuzzleImageLeftJointMask);
            _rightJointsMaskImage = HelperMethods.ColorArrToTexture2D(_PuzzleImageRightJointMask);

            _image = HelperMethods.ColorArrToTexture2D(_PuzzleImage);
            

            //Return data for puzzle pieces
            SPieceInfo[,] ResultData = new SPieceInfo[NoOfPiecesInCol, NoOfPiecesInRow];
            for (int i = 0; i < NoOfPiecesInCol; i++)
                for (int j = 0; j < NoOfPiecesInRow; j++)
                    ResultData[i, j] = PiecesInformation[i, j];

            return ResultData;

        }


        /// <summary>
        /// Generates actual joint image for each side joints image from actual puzzle image
        /// </summary>
        /// <param name="Image">Outputs generated puzzle image with female joints for puzzle pieces</param>
        /// <param name="TopJointMaskImage">Outputs generated top joints image for pieces</param>
        /// <param name="BotJointMaskImage">Outputs generated bottom joints image for pieces</param>
        /// <param name="LeftJointMaskImage">Outputs generated left joints image for pieces</param>
        /// <param name="RightJointMaskImage">Outputs generated right joints image for pieces</param>
        /// <param name="PieceHeightWithoutJoint">Output piece image height</param>
        /// <param name="PieceWidthWithoutJoint">Output piece image width</param>
        /// <param name="PuzzleImgHeight">Height of user provided puzzle image</param>
        /// <param name="PuzzleImgWidth">Width of user provided puzzle image</param>
        private void JointsMaskToJointsImage(ref Color[][] Image, ref Color[][] TopJointMaskImage, ref Color[][] BotJointMaskImage,
            ref Color[][] LeftJointMaskImage, ref Color[][] RightJointMaskImage, int PieceWidthWithoutJoint, int PieceHeightWithoutJoint,
            int PuzzleImgWidth, int PuzzleImgHeight)
        {

            Color TransparentColor = new Color(0,0,0,0);
            
            for (int XTrav = 0; XTrav < PuzzleImgWidth; XTrav++)
            {
                for (int YTrav = 0; YTrav < PuzzleImgHeight; YTrav++)
                {
                    bool IsChangeHappened = false;

                    if (TopJointMaskImage[XTrav][YTrav] == Color.black)
                    {
                        TopJointMaskImage[XTrav][YTrav] = Image[XTrav][YTrav];
                        IsChangeHappened = true;
                    }

                    if (BotJointMaskImage[XTrav][YTrav] == Color.black){
                        BotJointMaskImage[XTrav][YTrav] = Image[XTrav][YTrav];
                        IsChangeHappened = true;
                    }

                    if (LeftJointMaskImage[XTrav][YTrav] == Color.black){
                        LeftJointMaskImage[XTrav][YTrav] = Image[XTrav][YTrav];
                        IsChangeHappened = true;
                    }

                    if (RightJointMaskImage[XTrav][YTrav] == Color.black){
                        RightJointMaskImage[XTrav][YTrav] = Image[XTrav][YTrav];
                        IsChangeHappened = true;
                    }


                    if ( IsChangeHappened )
                        Image[XTrav][YTrav] = TransparentColor;

                }
            }
            
        }


        /// <summary>
        /// Draws joint mask image for each side of joints for every piece which is used to later 
        /// generate puzzle image for pieces
        /// </summary>
        /// <param name="TopJointMaskImage">Output mask image for top joints</param>
        /// <param name="BotJointMaskImage">Output mask image for bottom joints</param>
        /// <param name="LeftJointMaskImage">Output mask image for left joints</param>
        /// <param name="RightJointMaskImage">Output mask image for right joints</param>
        /// <param name="JointMaskImages">User provided joint mask images to be used</param>
        /// <param name="PieceHeightWithoutJoint">Output piece image height</param>
        /// <param name="PieceWidthWithoutJoint">Output piece image width</param>
        /// <param name="PuzzleImgHeight">Height of user provided puzzle image</param>
        /// <param name="PuzzleImgWidth">Width of user provided puzzle image</param>
        /// <param name="Cols">Total columns in puzzle</param>
        /// <param name="Rows">Total rows in puzzle</param>
        /// <returns>Returns created pieces metadata created during masks creation</returns>
        private SPieceInfo[,] DrawCustomPieceJointsMask(ref Color[][] TopJointMaskImage, ref Color[][] BotJointMaskImage,
            ref Color[][] LeftJointMaskImage, ref Color[][] RightJointMaskImage, Texture2D[] JointMaskImages,
                out int PieceWidthWithoutJoint, out int PieceHeightWithoutJoint, int PuzzleImgWidth, int PuzzleImgHeight, int Rows = 5, int Cols = 5)
        {

            int[] JointMaskWidth = new int[JointMaskImages.Length];
            int[] JointMaskHeight = new int[JointMaskImages.Length];

            //Create direction wise mask images
            Texture2D[] LeftJointMask = new Texture2D[JointMaskImages.Length];
            Texture2D[] RightJointMask = new Texture2D[JointMaskImages.Length];
            Texture2D[] TopJointMask = new Texture2D[JointMaskImages.Length];
            Texture2D[] BottomJointMask = new Texture2D[JointMaskImages.Length];

            SPieceInfo[,] ResultPiecesData = new SPieceInfo[Rows, Cols];


            //Initialize pieces data
            for (int i = 0; i < Rows; i++)
                for (int j = 0; j < Cols; j++)
                    ResultPiecesData[i, j] = new SPieceInfo((i * Cols) + j);

            int PieceHeight = PuzzleImgHeight / Rows;
            int PieceWidth = PuzzleImgWidth / Cols;

            PieceWidthWithoutJoint = PieceWidth;
            PieceHeightWithoutJoint = PieceHeight;



            for (int ArrayTav = 0; ArrayTav < JointMaskImages.Length; ArrayTav++)
            {
                LeftJointMask[ArrayTav] = JointMaskImages[ArrayTav];
                BottomJointMask[ArrayTav] = HelperMethods.rotateImage(JointMaskImages[ArrayTav], 90);
                RightJointMask[ArrayTav] = HelperMethods.rotateImage(BottomJointMask[ArrayTav], 90);
                TopJointMask[ArrayTav] = HelperMethods.rotateImage(JointMaskImages[ArrayTav], 270);
                                

                #region "Resize Joint mask images for drawing inside mask image And calculate joints width and height"

                //Resize Joint mask images for drawing inside mask image
                //  Image will be resized according to piece width
                int MaskImageWidth = (int)(PieceWidth * 0.3f);
                int MaskImageHeight = (int)((float)MaskImageWidth / ((float)JointMaskImages[ArrayTav].width / (float)JointMaskImages[ArrayTav].height));

                LeftJointMask[ArrayTav] = HelperMethods.resizeImage(LeftJointMask[ArrayTav], MaskImageWidth, MaskImageHeight);
                RightJointMask[ArrayTav] = HelperMethods.resizeImage(RightJointMask[ArrayTav], MaskImageWidth, MaskImageHeight);
                TopJointMask[ArrayTav] = HelperMethods.resizeImage(TopJointMask[ArrayTav], MaskImageWidth, MaskImageHeight);
                BottomJointMask[ArrayTav] = HelperMethods.resizeImage(BottomJointMask[ArrayTav], MaskImageWidth, MaskImageHeight);


                //Calculate joints width and heights
                CalculateCustomJointDimensions(LeftJointMask[ArrayTav], out JointMaskWidth[ArrayTav], out JointMaskHeight[ArrayTav]);



                #endregion

            }




            #region "Argument Error Checking"

            //Joint mask image width and height should be same
            //Joint mask image should have only black and white pixels inside it

            if (JointMaskImages[0].width != JointMaskImages[0].height)
            {
                Debug.LogError("JointMaskImage width and height should be same");
                return null;
            }
            else
            {
                bool ErrorFound = false;  //If Non-Black or Non-White pixel found

                //Check for pixel colors in joint mask image
                for (int rowtrav = 0; rowtrav < JointMaskImages[0].height && !ErrorFound; rowtrav++)
                {
                    for (int coltrav = 0; coltrav < JointMaskImages[0].width && !ErrorFound; coltrav++)
                    {
                        Color PixelColor = JointMaskImages[0].GetPixel(coltrav, rowtrav);

                        if (PixelColor != Color.white || PixelColor != Color.black)
                        {
                            ErrorFound = true;

                            //Debug.LogError("Only white and black pixels are allowed in JointMaskImage");

                            //return null;
                        }
                    }
                }


            }

            #endregion

            TopJointMaskImage = new Color[PuzzleImgWidth][];
            BotJointMaskImage = new Color[PuzzleImgWidth][];
            LeftJointMaskImage = new Color[PuzzleImgWidth][];
            RightJointMaskImage = new Color[PuzzleImgWidth][];

            //Clear Instantiated mask image
            for (int i = 0; i < PuzzleImgWidth; i++)
            {
                TopJointMaskImage[i] = new Color[PuzzleImgHeight];
                BotJointMaskImage[i] = new Color[PuzzleImgHeight];
                LeftJointMaskImage[i] = new Color[PuzzleImgHeight];
                RightJointMaskImage[i] = new Color[PuzzleImgHeight];
            }


            //Generate random joint info And Draw joints

            Random.seed = System.DateTime.Now.Second;
            Color PieceColor = Color.black;


            for (int RowTrav = 0; RowTrav < Rows; RowTrav++)
            {

                for (int ColTrav = 0; ColTrav < Cols; ColTrav++)
                {
                    int PieceX = ColTrav * PieceWidth;
                    int PieceY = RowTrav * PieceHeight;

                    //Generate Random joint info and Draw Joints From Mask Image

                    #region "Draw right joints according to piece joint information"

                    if (ColTrav < Cols - 1)
                    {
                        int SelectedRandomJoint = Random.Range(1, JointMaskImages.Length) - 1;

                        //Create random joint information
                        int RndVal = (int)(Random.Range(1f, 18f) >= 10 ? 1 : 0);
                        ResultPiecesData[RowTrav, ColTrav].AddJoint(new SJointInfo((EJointType)RndVal, EJointPosition.Right,
                                        JointMaskWidth[SelectedRandomJoint], JointMaskHeight[SelectedRandomJoint]));


                        int JointX = PieceX + PieceWidth;
                        int JointY = PieceY + (PieceHeight / 2) - (RightJointMask[SelectedRandomJoint].height / 2);

                        bool Result = false;
                        SJointInfo RightJointInfo = ResultPiecesData[RowTrav, ColTrav].GetJoint(EJointPosition.Right, out Result);

                        if (!Result)
                        {
                            Debug.LogError("Logical error in draw joints from mask image Right Joints");
                        }
                        else
                        {
                            if (RightJointInfo.JointType == EJointType.Male)
                                drawJoint(ref RightJointMaskImage, RightJointMask[SelectedRandomJoint], PieceColor, JointX, JointY);
                        }

                    }
                    #endregion

                    #region"Draw left joints according to piece joint information"

                    if (ColTrav > 0)
                    {
                        int SelectedRandomJoint = Random.Range(1, JointMaskImages.Length) - 1;

                        //Create random joint information
                        bool Result = false;

                        SJointInfo PreviousRightJoint = ResultPiecesData[RowTrav, ColTrav - 1].GetJoint(EJointPosition.Right, out Result);

                        if (Result == false)
                        {
                            Debug.LogError("Logical error in joints information left joint");
                        }
                        else
                        {
                            SJointInfo CalcLeftJoint = new SJointInfo(PreviousRightJoint.JointType == EJointType.Female ?
                                        EJointType.Male : EJointType.Female, EJointPosition.Left,
                                        JointMaskWidth[SelectedRandomJoint], JointMaskHeight[SelectedRandomJoint]);
                            ResultPiecesData[RowTrav, ColTrav].AddJoint(CalcLeftJoint);
                        }


                        int JointX = PieceX - LeftJointMask[SelectedRandomJoint].width;
                        int JointY = PieceY + (PieceHeight / 2) - (LeftJointMask[SelectedRandomJoint].height / 2);

                        Result = false;
                        SJointInfo LeftJointInfo = ResultPiecesData[RowTrav, ColTrav].GetJoint(EJointPosition.Left, out Result);

                        if (!Result)
                        {
                            Debug.LogError("Logical error in draw joints from mask image Left Joints");
                        }
                        else
                        {
                            if (LeftJointInfo.JointType == EJointType.Male)
                                drawJoint(ref LeftJointMaskImage, LeftJointMask[SelectedRandomJoint], PieceColor, JointX, JointY);
                        }
                    }

                    #endregion

                    #region"Draw Top joints according to piece joint information"

                    if (RowTrav < Rows - 1)
                    {
                        int SelectedRandomJoint = Random.Range(1, JointMaskImages.Length) - 1;

                        //Create random joint information
                        int RndVal = (int)(Random.Range(1f, 17f) >= 10 ? 1 : 0);
                        ResultPiecesData[RowTrav, ColTrav].AddJoint(new SJointInfo((EJointType)RndVal, EJointPosition.Top,
                                        JointMaskWidth[SelectedRandomJoint], JointMaskHeight[SelectedRandomJoint]));

                        int JointX = PieceX + (PieceWidth / 2) - (TopJointMask[SelectedRandomJoint].width / 2);
                        int JointY = PieceY + PieceHeight;

                        bool Result = false;
                        SJointInfo TopJointInfo = ResultPiecesData[RowTrav, ColTrav].GetJoint(EJointPosition.Top, out Result);

                        if (!Result)
                        {
                            Debug.LogError("Logical error in draw joints from mask image Top Joints");
                        }
                        else
                        {
                            if (TopJointInfo.JointType == EJointType.Male)
                                drawJoint(ref TopJointMaskImage, TopJointMask[SelectedRandomJoint], PieceColor, JointX, JointY);
                        }

                    }

                    #endregion

                    #region"Draw Bottom joints according to piece joint information"

                    if (RowTrav > 0)
                    {
                        int SelectedRandomJoint = Random.Range(1, JointMaskImages.Length) - 1;

                        //Create random joint information
                        bool Result = false;

                        SJointInfo PreviousPieceTopJoint = ResultPiecesData[RowTrav - 1, ColTrav].GetJoint(EJointPosition.Top, out Result);

                        if (Result == false)
                        {
                            Debug.LogError("Logical error in joints information Bottom joint");
                        }
                        else
                        {
                            SJointInfo CalcBottomJoint = new SJointInfo(PreviousPieceTopJoint.JointType == EJointType.Female ?
                                        EJointType.Male : EJointType.Female, EJointPosition.Bottom,
                                        JointMaskWidth[SelectedRandomJoint], JointMaskHeight[SelectedRandomJoint]);

                            ResultPiecesData[RowTrav, ColTrav].AddJoint(CalcBottomJoint);
                        }


                        int JointX = PieceX + (PieceWidth / 2) - (BottomJointMask[SelectedRandomJoint].width / 2);
                        int JointY = PieceY - BottomJointMask[SelectedRandomJoint].height;

                        Result = false;
                        SJointInfo BottomJointInfo = ResultPiecesData[RowTrav, ColTrav].GetJoint(EJointPosition.Bottom, out Result);

                        if (!Result)
                        {
                            Debug.LogError("Logical error in draw joints from mask image Top Joints");
                        }
                        else
                        {
                            if (BottomJointInfo.JointType == EJointType.Male)
                                drawJoint(ref BotJointMaskImage, BottomJointMask[SelectedRandomJoint], PieceColor, JointX, JointY);
                        }

                    }

                    #endregion


                }

            }



            return ResultPiecesData;
        }

        /// <summary>
        /// Creates completed background image to be shown to user as a helper image
        /// </summary>
        /// <param name="Image">Main puzzle image provided by user</param>
        /// <param name="ImageMask">Mask image created from main image</param>
        /// <param name="NoOfPiecesInRow">Total no of pieces in row of puzzle / total columns</param>
        /// <param name="NoOfPiecesInCol">Total no of pieces in Col of puzzle / total rows</param>
        /// <returns>Returns created helper background image</returns>
        private Texture2D PuzzleImgToBackgroundImage(Texture2D Image, Texture2D ImageMask, int NoOfPiecesInRow, int NoOfPiecesInCol)
        {
            Texture2D ImageCopy = Object.Instantiate(Image) as Texture2D;


            //Give image an effect
            Color[] texColors = ImageCopy.GetPixels();
            for (int i = 0; i < texColors.Length; i++)
            {
                float grayValue = texColors[i].grayscale;
                texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
            }
            ImageCopy.SetPixels(texColors);
            

            /*
            //Draw pieces borders
            Color BorderColor = new Color(30,30,30,100);

            for (int X = 0; X < ImageMask.width - 1; X++)
            {
                for (int Y = 0; Y < ImageMask.height - 1; Y++)
                {
                    if (ImageMask.GetPixel(X, Y) == Color.green &&
                            (ImageMask.GetPixel(X + 1, Y) == Color.red || ImageMask.GetPixel(X, Y + 1) == Color.red))
                    {
                        ImageCopy.SetPixel(X, Y, BorderColor);
                        ImageCopy.SetPixel(X+1, Y, BorderColor);
                        ImageCopy.SetPixel(X + 1, Y + 1, BorderColor);
                    }
                    else if (ImageMask.GetPixel(X, Y) == Color.red &&
                            (ImageMask.GetPixel(X + 1, Y) == Color.green || ImageMask.GetPixel(X, Y + 1) == Color.green))
                    {
                        ImageCopy.SetPixel(X, Y, BorderColor);
                        ImageCopy.SetPixel(X + 1, Y, BorderColor);
                        ImageCopy.SetPixel(X + 1, Y + 1, BorderColor);
                    }

                }
            }

            ImageCopy.Apply();
            */


            return ImageCopy;
        }
        



        /// <summary>
        /// Saves puzzle information to .pm file
        /// </summary>
        /// <param name="FilePath">Path where .pm file will be saved</param>
        /// <returns>Returns true if .pm file creation is successfull false otherwise</returns>
        public bool SaveData(string FilePath)
        {
            System.IO.BinaryWriter bWriter = null;

            using (bWriter = new System.IO.BinaryWriter(new System.IO.FileStream(FilePath, System.IO.FileMode.Create)))
            {
                //Write a random number to make sure when opening file the correct file is provided
                bWriter.Write(_secretVersionNo);

                //Save basic variables data
                bWriter.Write(_noOfPiecesInRow);
                bWriter.Write(_noOfPiecesInCol);

                bWriter.Write(_pieceWidthWithoutJoint);
                bWriter.Write(_pieceHeightWithoutJoint);
                

                //Check for error in created class variables
                if (_origionalImage == null || _jointMask == null || _CreatedPiecesData == null ||
                    _createdBackgroundImage == null || _leftJointsMaskImage == null || _rightJointsMaskImage == null ||
                    _topJointsMaskImage == null || _leftJointsMaskImage == null)
                {
                    Debug.LogError("Error saving data class may into be created properly");

                    bWriter.Close();

                    if (System.IO.File.Exists(FilePath))
                        System.IO.File.Delete(FilePath);

                    return false;
                }


                //Origional puzzle image
                byte[] imageEncoded = _origionalImage.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);

                //Create puzzle image with joints
                imageEncoded = _image.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);

                //Background image
                imageEncoded = _createdBackgroundImage.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);

                //Top joint mask image
                imageEncoded = _topJointsMaskImage.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);

                //Bottom joint mask image
                imageEncoded = _botJointsMaskImage.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);

                //Left joint mask image
                imageEncoded = _leftJointsMaskImage.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);

                //Right joint mask iamge
                imageEncoded = _rightJointsMaskImage.EncodeToPNG();
                bWriter.Write(imageEncoded.Length);
                bWriter.Write(imageEncoded);


                //Save joint mask array
                bWriter.Write(_jointMask.Length);
                for (int i = 0; i < _jointMask.Length; i++)
                {
                    imageEncoded = _jointMask[i].EncodeToPNG();
                    bWriter.Write(imageEncoded.Length);
                    bWriter.Write(imageEncoded);
                }


#region "Save pieces metadata"

                for (int RowTrav = 0; RowTrav < _noOfPiecesInCol; RowTrav++)
                {
                    for (int ColTrav = 0; ColTrav < _noOfPiecesInRow; ColTrav++)
                    {
                        //Save metadata for this piece
                        bWriter.Write(_CreatedPiecesData[RowTrav, ColTrav].ID);

                        SJointInfo[] TempJI = _CreatedPiecesData[RowTrav, ColTrav].GetJoints();
                        bWriter.Write(TempJI.Length);
                        for (int i = 0; i < TempJI.Length; i++)
                        {
                            bWriter.Write((int)TempJI[i].JointType);
                            bWriter.Write(TempJI[i].JointWidth);
                            bWriter.Write(TempJI[i].JointHeight);
                            bWriter.Write((int)TempJI[i].JointPosition);
                        }
                        
                    }
                }

#endregion

                bWriter.Close();

            }

            
            return true;
        }

        /// <summary>
        /// Loads .pm file to this instance of Puzzle Maker
        /// </summary>
        /// <param name="FilePath">FilePath of .pm file</param>
        /// <returns>Returns true if file loading is successfull false otherwise</returns>
        private bool LoadData(string FilePath)
        {
            
            System.IO.Stream stream = null;


            string Filename = System.IO.Path.GetFileName(FilePath);

            string StreammingFilePath = "";


            if (Application.platform == RuntimePlatform.Android )
            {
                StreammingFilePath = Application.streamingAssetsPath;
               
                StreammingFilePath = System.IO.Path.Combine(StreammingFilePath, Filename);

                WWW www = new WWW(StreammingFilePath);

                while (!www.isDone) ;

                if (string.IsNullOrEmpty(www.error))
                {

                    byte[] wwwLoadedData = www.bytes;
                    stream = new System.IO.MemoryStream(wwwLoadedData);

                }
                else
                {
                    Debug.LogError("www errror: " + www.error);
                    return false;
                }

            }
                            
            else if (Application.platform == RuntimePlatform.WindowsEditor ||
                        Application.platform == RuntimePlatform.WindowsPlayer ||
                        Application.platform == RuntimePlatform.OSXEditor ||
                        Application.platform == RuntimePlatform.OSXPlayer )
            {
                StreammingFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, Filename);

                stream = System.IO.File.OpenRead(StreammingFilePath);
            }

            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                StreammingFilePath = System.IO.Path.Combine(Application.streamingAssetsPath, Filename);

                stream = System.IO.File.OpenRead(StreammingFilePath);
            }

            else
            {
                Debug.LogError(Application.platform + " not supported");
            }

            Debug.Log("Loading file from " + StreammingFilePath);


            return LoadStreamData(stream);
        }

        /// <summary>
        /// Loads puzzle data from file stream of .pm file
        /// </summary>
        /// <param name="stream">Stream to load data from</param>
        /// <returns>Returns true if data loading is successfull false otherwise</returns>
        private bool LoadStreamData(System.IO.Stream stream)
        {
            System.IO.BinaryReader bReader = null;

            try
            {
                using (bReader = new System.IO.BinaryReader(stream))
                {
                    //Get secret number to check file intigrity
                    int secretNumber = bReader.ReadInt32();
                    if (secretNumber != _secretVersionNo)
                    {
                        bReader.Close();
                        Debug.LogError("Error reading file. Make sure this file is created with puzzle maker`s current version");
                        return false;
                    }

                    //Get basic variables data
                    _noOfPiecesInRow = bReader.ReadInt32();
                    _noOfPiecesInCol = bReader.ReadInt32();

                    _pieceWidthWithoutJoint = bReader.ReadInt32();
                    _pieceHeightWithoutJoint = bReader.ReadInt32();


                    //Origional puzzle image
                    int lengthImageEncoded = bReader.ReadInt32();
                    byte[] imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _origionalImage = new Texture2D(100, 100);
                    _origionalImage.LoadImage(imageEncoded);


                    //Puzzle image with joints
                    lengthImageEncoded = bReader.ReadInt32();
                    imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _image = new Texture2D(100, 100);
                    _image.LoadImage(imageEncoded);

                    //Created background image
                    lengthImageEncoded = bReader.ReadInt32();
                    imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _createdBackgroundImage = new Texture2D(100, 100);
                    _createdBackgroundImage.LoadImage(imageEncoded);
                    
                    //Top joint mask image
                    lengthImageEncoded = bReader.ReadInt32();
                    imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _topJointsMaskImage = new Texture2D(100, 100);
                    _topJointsMaskImage.LoadImage(imageEncoded);

                    //Bottom joint mask image
                    lengthImageEncoded = bReader.ReadInt32();
                    imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _botJointsMaskImage = new Texture2D(100, 100);
                    _botJointsMaskImage.LoadImage(imageEncoded);

                    //Left joint mask image
                    lengthImageEncoded = bReader.ReadInt32();
                    imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _leftJointsMaskImage = new Texture2D(100, 100);
                    _leftJointsMaskImage.LoadImage(imageEncoded);

                    //Right joint mask iamge
                    lengthImageEncoded = bReader.ReadInt32();
                    imageEncoded = bReader.ReadBytes(lengthImageEncoded);
                    _rightJointsMaskImage = new Texture2D(100, 100);
                    _rightJointsMaskImage.LoadImage(imageEncoded);


                    //Load joint mask array
                    int lengthJointMaskArr = bReader.ReadInt32();
                    _jointMask = new Texture2D[lengthJointMaskArr];
                    for (int i = 0; i < lengthJointMaskArr; i++)
                    {
                        int TempLength = bReader.ReadInt32();

                        _jointMask[i] = new Texture2D(100, 100);
                        imageEncoded = bReader.ReadBytes(TempLength);
                        _jointMask[i].LoadImage(imageEncoded);
                    }


#region "Retreive Pieces Metadata"

                    _CreatedPiecesData = new SPieceInfo[_noOfPiecesInCol, _noOfPiecesInRow];

                    for (int RowTrav = 0; RowTrav < _noOfPiecesInCol; RowTrav++)
                    {
                        for (int ColTrav = 0; ColTrav < _noOfPiecesInRow; ColTrav++)
                        {
                            int pieceID = bReader.ReadInt32();

                            //Get joints info
                            int JointInfoLength = bReader.ReadInt32();
                            SPieceInfo TempSPieceInfo = new SPieceInfo(pieceID);

                            for (int i = 0; i < JointInfoLength; i++)
                            {
                                int jointType = bReader.ReadInt32();
                                int jointWidth = bReader.ReadInt32();
                                int jointHeight = bReader.ReadInt32();
                                int jointPosition = bReader.ReadInt32();


                                TempSPieceInfo.AddJoint(new SJointInfo((EJointType)jointType, (EJointPosition)jointPosition,
                                                        jointWidth, jointHeight));
                            }
                            

                            //Insert this piece data in list
                            _CreatedPiecesData[RowTrav, ColTrav] = TempSPieceInfo;

                        }
                    }

#endregion

                    bReader.Close();

                }
            }
            catch (System.Exception ex)
            {
                throw new System.Exception("Exception in load data 2: " + ex.Message);
            }
            
            return true;
        }



        /// <summary>
        /// Calculates actual dimension of JointMask inside JointMaskImage
        /// </summary>
        /// <param name="JointMaskImage">User provided joint mask image</param>
        /// <param name="Width">Output calculated width of joint</param>
        /// <param name="Height">Output calculated height of joint</param>
        private void CalculateCustomJointDimensions(Texture2D JointMaskImage, out int Width, out int Height)
        {

            //Used to track which pixels have been added to stack while getting width and height
            Texture2D TrackPixels = Object.Instantiate(JointMaskImage) as Texture2D;

            //Make Trackpixels image white
            for (int X = 0; X < TrackPixels.width; X++)
                for (int Y = 0; Y < TrackPixels.height; Y++)
                    TrackPixels.SetPixel(X, Y, Color.white);
            TrackPixels.Apply();


            int minX = JointMaskImage.width;
            int minY = JointMaskImage.height;

            int maxX = 0;
            int maxY = 0;

            Stack<Vector2> pixelStack = new Stack<Vector2>();
            Vector2 PixelPostion = new Vector2(0,0);

            //Find pixel position with black pixel
            bool IsPixelFound = false;
            for (int RowTrav = 0; RowTrav < JointMaskImage.height && !IsPixelFound; RowTrav++)
            {
                for (int ColTrav = 0; ColTrav < JointMaskImage.width && !IsPixelFound; ColTrav++)
                {
                    if (JointMaskImage.GetPixel(ColTrav, RowTrav) == Color.black)
                    {
                        PixelPostion = new Vector2(ColTrav, RowTrav);
                        IsPixelFound = true;
                        break;
                    }
                }
            }


            pixelStack.Push(PixelPostion);



            while (pixelStack.Count > 0)
            {
                PixelPostion = pixelStack.Pop();

                int pixelPositionX = (int)PixelPostion.x;
                int pixelPositionY = (int)PixelPostion.y;

                TrackPixels.SetPixel(pixelPositionX, pixelPositionY, Color.black);


                if (maxX < pixelPositionX) maxX = pixelPositionX;
                if (maxY < pixelPositionY) maxY = pixelPositionY;

                if (minX > pixelPositionX) minX = pixelPositionX;
                if (minY > pixelPositionY) minY = pixelPositionY;


                //From center pixel spread outwards to get this piece
                if (pixelPositionX + 1 < JointMaskImage.width)
                    if (JointMaskImage.GetPixel(pixelPositionX + 1, pixelPositionY) == Color.black &&
                          TrackPixels.GetPixel(pixelPositionX + 1, pixelPositionY) != Color.black)
                        pixelStack.Push(new Vector2(PixelPostion.x + 1, PixelPostion.y));


                if (pixelPositionY + 1 < JointMaskImage.height)
                    if ( JointMaskImage.GetPixel(pixelPositionX, pixelPositionY + 1) == Color.black &&
                            TrackPixels.GetPixel(pixelPositionX, pixelPositionY + 1) != Color.black)
                        pixelStack.Push(new Vector2(PixelPostion.x, PixelPostion.y + 1));


                if (pixelPositionX - 1 > 0)
                    if (JointMaskImage.GetPixel(pixelPositionX - 1, pixelPositionY) == Color.black &&
                            TrackPixels.GetPixel(pixelPositionX - 1, pixelPositionY) != Color.black)
                        pixelStack.Push(new Vector2(PixelPostion.x - 1, PixelPostion.y));

                if (pixelPositionY - 1 > 0)
                    if (JointMaskImage.GetPixel(pixelPositionX, pixelPositionY - 1) == Color.black &&
                            TrackPixels.GetPixel(pixelPositionX, pixelPositionY - 1) != Color.black)
                        pixelStack.Push(new Vector2(PixelPostion.x, PixelPostion.y - 1));
            }


            Width = maxX - minX;
            Height = maxY - minY;

        }
        

        /// <summary>
        /// Draws using Joint Mask Image Onto Destination Image At Provided X And Y Coordinates.
        /// </summary>
        /// <param name="DestinationImage"> Image to be drawn on </param>
        /// <param name="JointMaskImage"> Image to draw </param>
        /// <param name="X">X position value should be greater then 0 and less then width - SourceImage Width of destination image</param>
        /// <param name="Y">Y position value should be greater then 0 and less then height - SourceImage Height of destination image</param>
        /// <param name="JointColor">Draw color of pixels on DestinationImage</param>
        private static void drawJoint(ref Color[][] DestinationImage, Texture2D JointMaskImage, Color JointColor, int X, int Y)
        {
            for (int i = X; i < X + JointMaskImage.width; i++)
            {
                for (int j = Y; j < Y + JointMaskImage.height; j++)
                {
                    if (JointMaskImage.GetPixel(i - X, j - Y) != Color.white)
                        DestinationImage[i][j] = JointColor;
                }
            }

        }

    }


    


}
