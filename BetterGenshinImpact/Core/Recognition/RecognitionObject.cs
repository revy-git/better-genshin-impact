﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using Rect = System.Windows.Rect;

namespace BetterGenshinImpact.Core.Recognition;

/// <summary>
/// 识别对象
/// </summary>
public class RecognitionObject
{
    public RecognitionType RecognitionType { get; set; }

    public Rect RegionOfInterest { get; set; }

    public string? Name { get; set; }

    #region 模板匹配

    /// <summary>
    /// 模板匹配的对象
    /// </summary>
    public Mat TemplateImageMat { get; set; }
    /// <summary>
    /// 模板匹配阈值。可选，默认 0.8 。
    /// </summary>
    public double Threshold { get; set; } = 0.8;

    /// <summary>
    /// 模板匹配算法。可选，默认 CCoeffNormed 。
    /// https://docs.opencv.org/4.x/df/dfb/group__imgproc__object.html
    /// </summary>
    public TemplateMatchModes TemplateMatchMode { get; set; } = TemplateMatchModes.CCoeffNormed;

    /// <summary>
    /// 匹配模板遮罩，指定图片中的某种色彩不需要匹配
    /// 使用时，需要将模板图片的背景色设置为纯黑色，即 #000000
    /// </summary>
    public bool UseMask { get; set; } = false;

    /// <summary>
    /// 不需要匹配的颜色，默认绿幕
    /// </summary>
    public Color MaskColor { get; set; } = Color.FromArgb(0, 255, 0);


    #endregion

    #region 颜色匹配

    /// <summary>
    /// 颜色匹配方式。即 cv::ColorConversionCodes。可选，默认 4 (RGB)。
    /// 常用值：4 (RGB, 3 通道), 40 (HSV, 3 通道), 6 (GRAY, 1 通道)。
    /// https://docs.opencv.org/4.x/d8/d01/group__imgproc__color__conversions.html
    /// </summary>
    public ColorConversionCodes ColorConversionCode { get; set; } = ColorConversionCodes.BGR2RGB;

    public Color LowerColor { get; set; }
    public Color UpperColor { get; set; }
    /// <summary>
    /// 符合的点的数量要求。可选，默认 1
    /// </summary>
    public int MatchCount { get; set; } = 1;

    #endregion

    #region 普通OCR文字识别

    /// <summary>
    /// OCR 引擎。可选，默认 Media.Ocr。
    /// </summary>
    public string? OcrEngine { get; set; }

    /// <summary>
    /// 部分文字识别结果不准确，进行替换。可选。
    /// </summary>
    public Dictionary<string, string[]> ReplaceDictionary { get; set; } = new();

    /// <summary>
    /// 包含匹配
    /// </summary>
    public List<string> ContainMatchText { get; set; } = new();

    /// <summary>
    /// 正则匹配
    /// </summary>
    public List<string> RegexMatchText { get; set; } = new();

    #endregion


    #region 模型OCR识别

    /// <summary>
    /// 是否使用模型文字识别
    /// </summary>
    public bool UseModelTextRecognition { get; set; } = false;

    /// <summary>
    /// 圣遗物 Yas
    /// 拾取 Yap
    /// </summary>
    public string? ModelTextRecognitionType { get; set; }

    #endregion
}