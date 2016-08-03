using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;

namespace Com.Utility
{
    public class ScreenCapture
    {
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateDC(
            string lpszDriver, // 驱动名称
            string lpszDevice, // 设备名称
            string lpszOutput, // 无用，可以设定位"NULL"
            IntPtr lpInitData // 任意的打印机数据
            );

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt(
            IntPtr hdcDest, //目标设备的句柄
            int nXDest, // 目标对象的左上角的X坐标
            int nYDest, // 目标对象的左上角的Y坐标
            int nWidth, // 目标对象的矩形的宽度
            int nHeight, // 目标对象的矩形的长度
            IntPtr hdcSrc, // 源设备的句柄
            int nXSrc, // 源对象的左上角的X坐标
            int nYSrc, // 源对象的左上角的Y坐标
            Int32 dwRop // 光栅的操作值
            );

        #region 变量
        private const int SourceCopy = 13369376;
        private const int CaptureBlt = 1073741824;
        private IntPtr _screenDC;
        private Graphics _graphicsScreen;
        private Graphics _graphicsBitmap;
        private Bitmap _myBitmap;
        #endregion

        #region 构造
        public ScreenCapture()
        {
            _screenDC = CreateDC("Display", null, null, (IntPtr)null);
            _graphicsScreen = Graphics.FromHdc(_screenDC);

        }
        #endregion

        #region 公共
        /// <summary>
        /// 按照指定的区域捕捉图像并保存到文件中
        /// </summary>
        /// <param name="screenX">目标区域的X坐标</param>
        /// <param name="screenY">目标区域的Y坐标</param>
        /// <param name="screenWidth">目标区域的宽度</param>
        /// <param name="screenHeight">目标区域的高度</param>
        /// <param name="saveImagePath">图片要保存到文件路径</param>
        public void CaptrueScreenRegionToFile(int screenX, int screenY, int screenWidth, int screenHeight, string saveImagePath)
        {
            try
            {
                StartCapture(screenX, screenY, screenWidth, screenHeight);
                _myBitmap.Save(saveImagePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CaptrueError" + ex.Message);
            }
            finally
            {
                if (_myBitmap != null)
                {
                    _myBitmap.Dispose();
                    _myBitmap = null;
                }
                if (_graphicsBitmap != null)
                {
                    _graphicsBitmap.Dispose();
                    _graphicsBitmap = null;
                }
            }
        }

        /// <summary>
        /// 按照指定的区域捕捉图像并按指定的压缩质量保存到文件中（注：压缩质量只针对BMP,DIB,RLE,JPEG,JPG,JFIF,GIF,TIF,TIFF,PNG这几类格式生效）
        /// </summary>
        /// <param name="screenX">目标区域的X坐标</param>
        /// <param name="screenY">目标区域的Y坐标</param>
        /// <param name="screenWidth">目标区域的宽度</param>
        /// <param name="screenHeight">目标区域的高度</param>
        /// <param name="saveImagePath">图片要保存到文件路径</param>
        /// <param name="quality">图片的压缩质量</param>
        public void CaptrueScreenRegionToFile(int screenX, int screenY, int screenWidth, int screenHeight, string saveImagePath, int quality)
        {
            string fileExtention = Path.GetExtension(saveImagePath).Substring(1);
            if (!IsSupportCertainFileExtention(fileExtention))
            {
                CaptrueScreenRegionToFile(screenX, screenY, screenWidth, screenHeight, saveImagePath);
                return;
            }

            try
            {
                StartCapture(screenX, screenY, screenWidth, screenHeight);
                System.Drawing.Imaging.EncoderParameters ps;
                System.Drawing.Imaging.EncoderParameter p;
                ps = new System.Drawing.Imaging.EncoderParameters(1);
                p = new System.Drawing.Imaging.EncoderParameter(System.Drawing.Imaging.Encoder.Quality, quality);
                ps.Param[0] = p;
                string pictureFomatDescription;
                GetPictureFormat(fileExtention, out pictureFomatDescription);
                _myBitmap.Save(saveImagePath, GetCodecInfo(pictureFomatDescription), ps);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CaptrueError:" + ex.Message);
            }
            finally
            {
                if (_myBitmap != null)
                {
                    _myBitmap.Dispose();
                    _myBitmap = null;
                }
                if (_graphicsBitmap != null)
                {
                    _graphicsBitmap.Dispose();
                    _graphicsBitmap = null;
                }
            }
        }

        /// <summary>
        /// 按照指定的区域捕捉图像并显示在指定的控件中
        /// </summary>
        /// <param name="screenX">目标区域的X坐标</param>
        /// <param name="screenY">目标区域的Y坐标</param>
        /// <param name="screenWidth">目标区域的宽度</param>
        /// <param name="screenHeight">目标区域的高度</param>
        /// <param name="controlPtr">要显示图像的控件的句柄</param>
        public void CaptrueScreenRegionToDisplay(int screenX, int screenY, int screenWidth, int screenHeight, IntPtr controlPtr)
        {
            try
            {
                StartCapture(screenX, screenY, screenWidth, screenHeight);
                Control displayControl = Control.FromHandle(controlPtr);
                Graphics displayControlGraphic = displayControl.CreateGraphics();
                displayControlGraphic.DrawImage(_myBitmap, 0, 0, screenWidth, screenHeight);
                displayControlGraphic.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("CaptrueError:" + ex.Message);
            }
            finally
            {
                if (_myBitmap != null)
                {
                    _myBitmap.Dispose();
                    _myBitmap = null;
                }
                if (_graphicsBitmap != null)
                {
                    _graphicsBitmap.Dispose();
                    _graphicsBitmap = null;

                }
            }

        }
        #endregion

        #region 私有方法
        /// <summary>
        /// 按照指定的区域捕捉图像
        /// </summary>
        /// <param name="screenX">目标区域的X坐标</param>
        /// <param name="screenY">目标区域的Y坐标</param>
        /// <param name="screenWidth">目标区域的宽度</param>
        /// <param name="screenHeight">目标区域的高度</param>
        private void StartCapture(int screenX, int screenY, int screenWidth, int screenHeight)
        {
            _myBitmap = new Bitmap(screenWidth, screenHeight, _graphicsScreen);
            _graphicsBitmap = Graphics.FromImage(_myBitmap);
            IntPtr bitmapPtr = _graphicsBitmap.GetHdc();
            BitBlt(bitmapPtr, 0, 0, screenWidth, screenHeight, _screenDC, screenX, screenY, (SourceCopy | CaptureBlt));
            _graphicsBitmap.ReleaseHdc(bitmapPtr);
        }

        /// <summary>
        /// 保存JPG时用
        /// </summary>
        /// <param name="mimeType"></param>
        /// <returns>得到指定mimeType的ImageCodecInfo</returns>
        private static ImageCodecInfo GetCodecInfo(string mimeType)
        {
            System.Drawing.Imaging.ImageCodecInfo[] CodecInfo = ImageCodecInfo.GetImageEncoders();
            foreach (ImageCodecInfo ici in CodecInfo)
            {
                if (ici.MimeType == mimeType)
                    return ici;
            }
            return null;
        }

        /// <summary>
        /// 查看某种图片格式否支持指定压缩质量进行压缩
        /// </summary>
        /// <param name="fileExtention">图片格式</param>
        /// <returns>支持，返回true，否则，返回false</returns>
        private bool IsSupportCertainFileExtention(string fileExtention)
        {
            for (int i = 0; i < (int)PictureFileNameExtension.endFlag; i++)
            {
                if (fileExtention.ToLower() == ((PictureFileNameExtension)i).ToString())
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 根据图片文件的后缀名得到图片文件的描述类型
        /// </summary>
        /// <param name="fileExtention"></param>
        /// <param name="pictureFomatDescription"></param>
        private void GetPictureFormat(string fileExtention, out string pictureFomatDescription)
        {
            string lowerFileExtention = fileExtention.ToLower();

            if (lowerFileExtention == PictureFileNameExtension.bmp.ToString()
                || lowerFileExtention == PictureFileNameExtension.dib.ToString()
                || lowerFileExtention == PictureFileNameExtension.rle.ToString())
            {
                pictureFomatDescription = "image/" + PictureFomatDescriptionEnum.bmp.ToString();
            }
            else if (lowerFileExtention == PictureFileNameExtension.jpeg.ToString()
                || lowerFileExtention == PictureFileNameExtension.jpg.ToString()
                || lowerFileExtention == PictureFileNameExtension.jfif.ToString())
            {
                pictureFomatDescription = "image/" + PictureFomatDescriptionEnum.jpeg.ToString();
            }
            else if (lowerFileExtention == PictureFileNameExtension.gif.ToString())
            {
                pictureFomatDescription = "image/" + PictureFomatDescriptionEnum.gif.ToString();
            }
            else if (lowerFileExtention == PictureFileNameExtension.tif.ToString()
                 || lowerFileExtention == PictureFileNameExtension.tiff.ToString())
            {

                pictureFomatDescription = "image/" + PictureFomatDescriptionEnum.tiff.ToString();
            }
            else if (lowerFileExtention == PictureFileNameExtension.png.ToString())
            {
                pictureFomatDescription = "image/" + PictureFomatDescriptionEnum.png.ToString();
            }
            else
            {
                pictureFomatDescription = "image/" + PictureFomatDescriptionEnum.jpeg.ToString();
            }
        }

        private enum PictureFomatDescriptionEnum
        {
            bmp = 0,
            jpeg = 1,
            gif = 2,
            tiff = 3,
            png = 4
        }

        /// <summary>
        /// 支持指定图片压缩质量的图片的格式枚举
        /// </summary>
        private enum PictureFileNameExtension
        {
            bmp = 0,
            dib,
            rle,
            jpg,
            jpeg,
            jfif,
            gif,
            tif,
            tiff,
            png,
            endFlag
        }


        #endregion

    }
}
