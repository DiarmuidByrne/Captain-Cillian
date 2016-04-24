using UnityEngine;
using System.IO;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

public class HelperMethods{

    /// <summary>
    /// Converts a texture2d image to two dimensional jagged color array
    /// </summary>
    /// <param name="Image">Texture2d image to be converted</param>
    /// <returns></returns>
    public static Color[][] Texture2DToColorArr(Texture2D Image)
    {
        Color[][] Temp = new Color[Image.width][];

        if (Image != null)
        {
            for (int XTrav = 0; XTrav < Image.width; XTrav++)
            {
                Temp[XTrav] = new Color[Image.height];

                for (int YTrav = 0; YTrav < Image.height; YTrav++)
                {
                    Temp[XTrav][YTrav] = Image.GetPixel(XTrav, YTrav);
                }
            }

            return Temp;
        }

        return null;
    }


    /// <summary>
    /// Converts two dimensional jagged color array to texture2d image
    /// </summary>
    /// <param name="ImgClrArr">Jagged color array to be converted</param>
    /// <returns></returns>
    public static Texture2D ColorArrToTexture2D(Color[][] ImgClrArr)
    {
        int ImageWidth = ImgClrArr.GetLength(0);
        int ImageHeight = ImgClrArr[0].Length;

        Texture2D ResultedTexture = new Texture2D(ImageWidth, ImageHeight);
        for (int XTrav = 0; XTrav < ImageWidth; XTrav++)
        {
            for (int YTav = 0; YTav < ImageHeight; YTav++)
            {
                ResultedTexture.SetPixel(XTrav, YTav, ImgClrArr[XTrav][YTav]);
            }
        }
        ResultedTexture.Apply();

        return ResultedTexture;
    }


    /// <summary>
    /// Rotates an image to provided angle value
    /// </summary>
    /// <param name="Image">Image to rotate</param>
    /// <param name="angle">Angle value between 0 and 360</param>
    /// <returns></returns>
    public static Texture2D rotateImage(Texture2D Image, int angle)
    {
        int x;
        int y;
        int i;
        int j;

        float phi = Mathf.Deg2Rad * angle;

        float sn = Mathf.Sin(phi);
        float cs = Mathf.Cos(phi);

        int isn = (int)sn;
        int ics = (int)cs;

        Color32[] arr = Image.GetPixels32();

        Texture2D texture = Object.Instantiate(Image) as Texture2D;
        Color32[] arr2 = texture.GetPixels32();

        int W = texture.width;
        int H = texture.height;
        int xc = W / 2 - 1;
        int yc = H / 2;

        Color32 TransparentColor = new Color32(255, 255, 255, 0);

        for (j = 0; j < H; j++)
        {
            for (i = 0; i < W; i++)
            {
                arr2[j * W + i] = TransparentColor;

                x = ics * (i - xc) + isn * (j - yc) + xc;
                y = -isn * (i - xc) + ics * (j - yc) + yc;

                if ((x > -1) && (x < W) && (y > -1) && (y < H))
                {
                    arr2[j * W + i] = arr[y * W + x];
                }
            }
        }

        texture.SetPixels32(arr2);
        texture.Apply();

        return texture;

    }


    /// <summary>
    /// Scales provided image
    /// </summary>
    /// <param name="Image">  </param>
    /// <param name="Scale"> Scale amount 0 and above </param>
    /// <returns>Return new scaled image</returns>
    public static Texture2D scaleImage(Texture2D Image, float Scale)
    {
        Texture2D Temp = Object.Instantiate(Image) as Texture2D;

        float NewWidth = Image.width * Scale;
        float NewHeight = Image.height * Scale;

        TextureScale.Bilinear(Temp, (int)NewWidth, (int)NewHeight);

        return Temp;
    }


    public static Texture2D resizeImage(Texture2D Image, int Width, int Height)
    {
        Texture2D Temp = Object.Instantiate(Image) as Texture2D;

        TextureScale.Bilinear(Temp, (int)Width, (int)Height);

        return Temp;
    }


    public static void saveTexture2D(Texture2D Texture, string Path)
    {
        System.IO.File.WriteAllBytes(Path, Texture.EncodeToPNG());
    }

    /// <summary>
    /// Creates color array image with color[width][height] dimension
    /// </summary>
    /// <param name="InitializationColor">Initialization color for image</param>
    /// <param name="ImageWidth">Width of resulted color array image</param>
    /// <param name="ImageHeight">Height of resulted color array image</param>
    /// <returns></returns>
    public static Color[][] CreateColoredImage(Color InitializationColor, int ImageWidth, int ImageHeight)
    {
        Color[][] ResultedImage = new Color[ImageWidth][];

        for (int XTrav = 0; XTrav < ImageWidth; XTrav++)
        {
            ResultedImage[XTrav] = new Color[ImageHeight];
            for (int YTrav = 0; YTrav < ImageHeight; YTrav++)
                ResultedImage[XTrav][YTrav] = InitializationColor;
        }

        return ResultedImage;

    }


    /// <summary>
    /// Code taken from http://wiki.unity3d.com/index.php/TextureScale
    /// </summary>
    public class TextureScale
    {

        public class ThreadData
        {
            public int start;
            public int end;
            public ThreadData(int s, int e)
            {
                start = s;
                end = e;
            }
        }


        private static Color[] texColors;
        private static Color[] newColors;
        private static int w;
        private static float ratioX;
        private static float ratioY;
        private static int w2;
        private static int finishCount;
        private static Mutex mutex;
        //private static Object thisLock = new Object();


        public static void Point(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, false);
        }

        public static void Bilinear(Texture2D tex, int newWidth, int newHeight)
        {
            ThreadedScale(tex, newWidth, newHeight, true);
        }

        private static void ThreadedScale(Texture2D tex, int newWidth, int newHeight, bool useBilinear)
        {
            texColors = tex.GetPixels();
            newColors = new Color[newWidth * newHeight];
            if (useBilinear)
            {
                ratioX = 1.0f / ((float)newWidth / (tex.width - 1));
                ratioY = 1.0f / ((float)newHeight / (tex.height - 1));
            }
            else
            {
                ratioX = ((float)tex.width) / newWidth;
                ratioY = ((float)tex.height) / newHeight;
            }
            w = tex.width;
            w2 = newWidth;
            var cores = Mathf.Min(SystemInfo.processorCount, newHeight);
            var slice = newHeight / cores;

            finishCount = 0;
            /*if (mutex == null)
            {
                mutex = new Mutex(false);
            }*/
            if (cores > 1)
            {
                int i = 0;
                ThreadData threadData;
                for (i = 0; i < cores - 1; i++)
                {
                    threadData = new ThreadData(slice * i, slice * (i + 1));
                    ParameterizedThreadStart ts = useBilinear ? new ParameterizedThreadStart(BilinearScale) : new ParameterizedThreadStart(PointScale);
                    Thread thread = new Thread(ts);
                    thread.Start(threadData);
                }
                threadData = new ThreadData(slice * i, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
                while (finishCount < cores)
                {
                    Thread.Sleep(1);
                }
            }
            else
            {
                ThreadData threadData = new ThreadData(0, newHeight);
                if (useBilinear)
                {
                    BilinearScale(threadData);
                }
                else
                {
                    PointScale(threadData);
                }
            }

            tex.Resize(newWidth, newHeight);
            tex.SetPixels(newColors);
            tex.Apply();
        }

        public static void BilinearScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                int yFloor = (int)Mathf.Floor(y * ratioY);
                var y1 = yFloor * w;
                var y2 = (yFloor + 1) * w;
                var yw = y * w2;

                for (var x = 0; x < w2; x++)
                {
                    int xFloor = (int)Mathf.Floor(x * ratioX);
                    var xLerp = x * ratioX - xFloor;
                    newColors[yw + x] = ColorLerpUnclamped(ColorLerpUnclamped(texColors[y1 + xFloor], texColors[y1 + xFloor + 1], xLerp),
                                                           ColorLerpUnclamped(texColors[y2 + xFloor], texColors[y2 + xFloor + 1], xLerp),
                                                           y * ratioY - yFloor);
                }
            }

            //mutex.WaitOne();
            //lock (thisLock)
            //{
            finishCount++;
            //}
            //mutex.ReleaseMutex();
        }

        public static void PointScale(System.Object obj)
        {
            ThreadData threadData = (ThreadData)obj;
            for (var y = threadData.start; y < threadData.end; y++)
            {
                var thisY = (int)(ratioY * y) * w;
                var yw = y * w2;
                for (var x = 0; x < w2; x++)
                {
                    newColors[yw + x] = texColors[(int)(thisY + ratioX * x)];
                }
            }

            //mutex.WaitOne();
            //lock (thisLock)
            //{
            finishCount++;
            //}
            //mutex.ReleaseMutex();
        }

        private static Color ColorLerpUnclamped(Color c1, Color c2, float value)
        {
            return new Color(c1.r + (c2.r - c1.r) * value,
                              c1.g + (c2.g - c1.g) * value,
                              c1.b + (c2.b - c1.b) * value,
                              c1.a + (c2.a - c1.a) * value);
        }

    }
}
