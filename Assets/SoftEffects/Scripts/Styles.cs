using System;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
#if UNITY_EDITOR
    using UnityEditor;
#endif
using System.IO;

namespace Mkey
{
#if UNITY_EDITOR
    [Serializable]
    public class InnerShadowOptions : Options
    {
        public enum ISBlendMode { Multiplay, Darken, Normal }

        public int size = 2;
        public int sizeOld = 2;

        public float choke;
        public float chokeOld;

        public float distance;
        public float distanceOld;

        public float angle;
        public float angleOld;

        private float dSin;
        private float dCos;

        public float DSin
        {
            get { return dSin; }
        }

        public float DCos
        {
            get { return dCos; }
        }


        public ISBlendMode blendMode = ISBlendMode.Multiplay;
        public Color color = Color.black;
        public AnimationCurve contour = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve contourOld = AnimationCurve.Linear(0, 0, 1, 1);
        BoxBlurAndDilateSettings bd;
        [SerializeField]
        private float[] contourBuffer;

        public float[] ContourBuffer
        {
            get
            {
                if (!CurvesEqual(contourOld, contour) || contourBuffer == null || contourBuffer.Length == 0)
                {
                    CreateCurveBuffer(contour, ref contourBuffer);
                }
                return contourBuffer;
            }
        }

        public bool IsDirty
        {
            get { return (use && (size != sizeOld || choke != chokeOld)) || (use != useOld); }
            set
            {
                if (!value)
                {
                    sizeOld = size;
                    chokeOld = choke;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        /// Render AA R8 texture from RGBA a-channel texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        /// <param name="inputBufferInside"></param>
        /// <param name="rebuild"></param>
        public void RenderTextureFromAAFEDT_AR8(GPUWorker gpuWorker, Texture inputTexture, CBuffers cb, bool rebuild)
        {
            if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
            {
                gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);
            }

            bd = new BoxBlurAndDilateSettings(size, choke / 100f);
            if (SoftEffects.debuglog) Debug.Log("Inner shadow size: " + bd.getDilatePixels());
            if (SoftEffects.debuglog) Debug.Log("Box blur radius " + bd.getBoxBlurRadius());
            gpuWorker.GPURenderInnerShadowFromBuffer_R8(inputTexture, cb.CBEDTAASigned, bd.getDilatePixels(), bd.getBoxBlurRadius(), this, ref r_mainTexture);

            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered Inner Shadow R8 render texture from float buffer.");
        }

        public int BlendModeNumber
        {
            get
            {
                switch (blendMode)
                {
                    case ISBlendMode.Multiplay:
                        return 0;
                    case ISBlendMode.Darken:
                        return 1;
                    case ISBlendMode.Normal:
                        return 2;
                    default:
                        return 0;
                }
            }
        }

        public void DsinCos()
        {
            float rad = Mathf.Deg2Rad * angle;
            dCos = Mathf.Cos(rad) * distance;
            dSin = Mathf.Sin(rad) * distance;
        }

        private class BoxBlurAndDilateSettings
        {
            private int m_enlargePixels;
            private int m_dilatePixels;
            private float m_boxBlurRadius;

            public BoxBlurAndDilateSettings(int sizeInPixels, float spread)
            {
                m_enlargePixels = sizeInPixels + 2;
                m_dilatePixels = (int)(sizeInPixels * spread + 0.5f);

                int blurPixels = sizeInPixels - m_dilatePixels;

                // Photoshop fudge factor by Brian Fiete
                float fudge = 1.85f - 0.45f * Mathf.Min(1.0f, blurPixels / 10.0f);
                m_boxBlurRadius = Mathf.Max(blurPixels - fudge, 0.0f);
            }

            public int getEnlargePixels()
            {
                return m_enlargePixels;
            }

            public int getDilatePixels()
            {
                return m_dilatePixels;
            }

            public float getBoxBlurRadius()
            {
                return m_boxBlurRadius;
            }
        }
    }

    [Serializable]
    public class InnerGlowOptions : Options
    {
        public enum FillType { Color, Gradient }
        public enum Method { Precize, Soft }
        public enum Position { FromOuter, FromCenter }
        public enum IGBlendMode { Lighten, Screen, Overlay }

        public int size = 2;
        public float spread;
        public int range = 50;
        public int sizeOld = 2;
        public float spreadOld;
        public int rangeOld = 50;
        public Method method = Method.Precize;
        public Method methodOld = Method.Precize;

        public IGBlendMode blendMode = IGBlendMode.Screen;
        public FillType fillType = FillType.Color;
        public Color color = Color.white;
        public Gradient gradient;
        BoxBlurAndDilateSettings bd;
        public AnimationCurve contour = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve contourOld = AnimationCurve.Linear(0, 0, 1, 1);
        public Texture2D contourTexture;
        public Position position = Position.FromOuter;
        public Position positionOld = Position.FromOuter;
        public int gradPosition = 1; // position in gradients

        [SerializeField]
        private float[] contourBuffer;

        public float[] ContourBuffer
        {
            get
            {
                if (!CurvesEqual(contourOld, contour) || contourBuffer == null || contourBuffer.Length == 0)
                {
                    CreateCurveBuffer(contour, ref contourBuffer);
                }
                return contourBuffer;
            }
        }

        public bool IsDirty
        {
            get { return (use && (size != sizeOld || spread != spreadOld || method != methodOld || position != positionOld)) || (use != useOld); }
            set
            {
                if (!value)
                {
                    sizeOld = size;
                    spreadOld = spread;
                    methodOld = method;
                    positionOld = position;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        /// Render AA R8 texture from RGBA a-channel texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        /// <param name="inputBufferInside"></param>
        /// <param name="rebuild"></param>
        public void RenderTextureFromAAFEDT_AR8(GPUWorker gpuWorker, Texture inputTexture, CBuffers cb, bool rebuild)
        {
            if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
            {
                gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);
            }
            switch (method)
            {
                case Method.Precize:
                    gpuWorker.GPURenderInnerGlowPrecFromBuffer_R8(inputTexture, cb.CBEDTAASigned, this, ref r_mainTexture);
                    break;

                case Method.Soft: // box blur and dilate 
                    bd = new BoxBlurAndDilateSettings(size, spread / 100f);
                    gpuWorker.GPURenderInnerGlowSoftFromBuffer_R8(inputTexture, cb.CBEDTAASigned, bd.getDilatePixels(), bd.getBoxBlurRadius(), this, ref r_mainTexture);
                    break;
            }

            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered Inner Glow R8 render texture from float buffer.");
        }

        public int FillNumber
        {
            get
            {
                switch (fillType)
                {
                    case FillType.Color:
                        return 0;
                    case FillType.Gradient:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        private class BoxBlurAndDilateSettings
        {
            private int m_enlargePixels;
            private int m_dilatePixels;
            private float m_boxBlurRadius;

            public BoxBlurAndDilateSettings(int sizeInPixels, float spread)
            {
                m_enlargePixels = sizeInPixels + 2;
                m_dilatePixels = (int)(sizeInPixels * spread + 0.5f);

                int blurPixels = sizeInPixels - m_dilatePixels;

                // Photoshop fudge factor by Brian Fiete
                float fudge = 1.85f - 0.45f * Mathf.Min(1.0f, blurPixels / 10.0f);
                m_boxBlurRadius = Mathf.Max(blurPixels - fudge, 0.0f);
            }

            public int getEnlargePixels()
            {
                return m_enlargePixels;
            }

            public int getDilatePixels()
            {
                return m_dilatePixels;
            }

            public float getBoxBlurRadius()
            {
                return m_boxBlurRadius;
            }
        }

        public int BlendModeNumber
        {
            get
            {
                switch (blendMode)
                {
                    case IGBlendMode.Lighten:
                        return 0;
                    case IGBlendMode.Screen:
                        return 1;
                    case IGBlendMode.Overlay:
                        return 2;
                    default:
                        return 0;
                }
            }
        }

        public void SaveTexture()
        {
            throw new NotImplementedException();
        }

        public int PositionNumber
        {
            get
            {
                switch (position)
                {
                    case Position.FromOuter:
                        return 0;
                    case Position.FromCenter:
                        return 1;
                    default:
                        return 0;
                }
            }
        }
    }

    [Serializable]
    public class BevelOptions : Options
    {
        public enum Style { Inside, Outside }
        public enum BLightMode { Lighten, Screen, Overlay }
        public enum BShadowMode { Darken, Multiplay }
        public enum BevelTechnique { ChiselHard, ChiselSoft, Smooth }
        public RenderTexture r_shadowTexture;
        public int size = 2; // 0 - 250px
        public int sizeOld = 2;
        public int depth = 500; // 0 -1000%
        public int depthOld = 500;
        public int smoothing; // 0 - 16px
        public int smoothingOld;

        public BevelTechnique bTechnique;
        public BevelTechnique bTechniqueOld;

        public Style bStyle;
        public Style bStyleOld;

        // shading
        public int angle = 120; // -180 - 180
        private int angleOld = 120;
        public int lightAltitude = 30; // 0 - 90;
        private int lightAltitudeOld = 30;

        public BLightMode lightBlendMode = BLightMode.Screen;
        public BLightMode lightBlendModeOld = BLightMode.Screen;

        public Color lightColor = Color.white;
        public Color lightColorOld = Color.white;

        public BShadowMode shadowBlendMode = BShadowMode.Multiplay;
        public BShadowMode shadowBlendModeOld = BShadowMode.Multiplay;

        public Color shadowColor = Color.black;
        public Color shadowColorOld = Color.black;

        public AnimationCurve contour = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve contourOld = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        private float[] contourBuffer;

        public float[] ContourBuffer
        {
            get
            {
                if (!CurvesEqual(contourOld, contour) || contourBuffer == null || contourBuffer.Length == 0)
                {
                    CreateCurveBuffer(contour, ref contourBuffer);
                }
                return contourBuffer;
            }
        }

        public bool IsDirty
        {
            get
            {
                return (use && (size != sizeOld
                  || angle != angleOld || lightAltitude != lightAltitudeOld
                  || bTechnique != bTechniqueOld
                  || depthOld != depth) || smoothingOld != smoothing)
                  || !CurvesEqual(contourOld, contour)
                  || bStyleOld != bStyle
                  || (use != useOld);
            }
            set
            {
                if (!value)
                {
                    sizeOld = size;
                    angleOld = angle;
                    lightAltitudeOld = lightAltitude;
                    bTechniqueOld = bTechnique;
                    contourOld.keys = contour.keys;
                    depthOld = depth;
                    smoothingOld = smoothing;
                    bStyleOld = bStyle;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated() || r_shadowTexture == null || !r_shadowTexture.IsCreated())); }
        }

        /// <summary>
        /// Render R8 texture from AAEDT distance buffer 
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        /// <param name="inputBufferInside"></param>
        /// <param name="inputBufferOutside"></param>
        /// <param name="rebuild"></param>
        public void RenderTexture(GPUWorker gpuWorker, Texture inputTexture, CBuffers cb, bool rebuild)
        {
            switch (bTechnique)
            {
                case BevelTechnique.ChiselSoft:
                    if (!CheckBuffer(cb.CBEDTSigned) || rebuild)
                    {
                        gpuWorker.GPURenderFEDTSignedBuffer(inputTexture, ref cb.CBEDTSigned);
                    }

                    switch (bStyle)
                    {
                        case Style.Inside:
                            gpuWorker.GPURenderBevelInside_R8(inputTexture, cb.CBEDTSigned, ref r_mainTexture, ref r_shadowTexture, this);
                            break;
                        case Style.Outside:
                            gpuWorker.GPURenderBevelOutside_R8(inputTexture, cb.CBEDTSigned, ref r_mainTexture, ref r_shadowTexture, this);
                            break;
                    }
                    break;

                case BevelTechnique.ChiselHard:
                    if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
                    {
                        gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);
                    }
                    switch (bStyle)
                    {
                        case Style.Inside:
                            gpuWorker.GPURenderBevelInside_R8(inputTexture, cb.CBEDTAASigned, ref r_mainTexture, ref r_shadowTexture, this);
                            break;
                        case Style.Outside:
                            gpuWorker.GPURenderBevelOutside_R8(inputTexture, cb.CBEDTAASigned, ref r_mainTexture, ref r_shadowTexture, this);
                            break;
                            #region comment
                            //  case Style.Center:
                            //     gpuWorker.GPURenderBevelInside_R8(inputTexture, cb.distMapInlineAAF, cb.distMapOutlineAAF, ref r_mainTexture, ref r_shadowTexture, this);
                            //     break;
                            #endregion comment
                    }
                    break;

                case BevelTechnique.Smooth:
                    if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
                    {
                        gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);
                    }
                    switch (bStyle)
                    {
                        case Style.Inside:
                            gpuWorker.GPURenderBevelInside_R8(inputTexture, cb.CBEDTAASigned, ref r_mainTexture, ref r_shadowTexture, this);
                            break;
                        case Style.Outside:
                            gpuWorker.GPURenderBevelOutside_R8(inputTexture, cb.CBEDTAASigned, ref r_mainTexture, ref r_shadowTexture, this);
                            break;
                    }
                    break;
            }

            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered Bevel R8 render texture from buffer.");
        }

        public int LightModeNumber
        {
            get
            {
                switch (lightBlendMode)
                {
                    case BLightMode.Lighten:
                        return 0;
                    case BLightMode.Screen:
                        return 1;
                    case BLightMode.Overlay:
                        return 2;
                    default:
                        return 0;
                }
            }
        }

        public int ShadowModeNumber
        {
            get
            {
                switch (shadowBlendMode)
                {
                    case BShadowMode.Darken:
                        return 0;
                    case BShadowMode.Multiplay:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        public int TechniqueNumber
        {
            get
            {
                switch (bTechnique)
                {
                    case BevelTechnique.ChiselHard:
                        return 2;
                    case BevelTechnique.ChiselSoft:
                        return 1;
                    case BevelTechnique.Smooth:
                        return 3;
                    default:
                        return 2;
                }
            }
        }

        public Vector3 LightDirection
        {
            get
            {
                float lightAngle_r = Mathf.Deg2Rad * angle;
                float lightAlt_r = Mathf.Deg2Rad * lightAltitude;
                float coslAlt = Mathf.Cos(lightAlt_r);
                float lx = coslAlt * Mathf.Cos(lightAngle_r);
                float ly = coslAlt * Mathf.Sin(lightAngle_r);
                float lz = Mathf.Sin(lightAlt_r);
                return new Vector3(lx, ly, lz).normalized;
            }
        }

        public int PosNumber
        {
            get
            {
                {
                    switch (bStyle)
                    {
                        case Style.Inside:
                            return 0;
                        case Style.Outside:
                            return 1;
                        // case Style.Center:
                        //     return 2;
                        default:
                            return 0;
                    }
                }
            }
        }
    }

    [Serializable]
    public class StrokeOptions : Options
    {
        public enum Position
        {
            Inside
                , Outside
            // , Center
        }
        public enum FillType
        {
            Color
                , Gradient
            // ,Pattern
        }
        public enum GradientType { Linear, Radial, Angle, Reflected, Diamond, ShapeBurst }

        public int size = 2;
        public int sizeOld = 2;
        public Position pos = Position.Inside;
        public Position posOld = Position.Inside;
        public BlendMode blendMode = BlendMode.Normal;
        public FillType fillType = FillType.Color;
        public GradientType gradType = GradientType.Linear;
        public Color color = Color.white;
        public Texture2D pattern;
        public Gradient gradient;
        public float angle; // gradient angle
        public int gradPosition = 0; // position in gradients

        public bool IsDirty
        {
            get { return (use && (size != sizeOld || pos != posOld)) || (use != useOld); }
            set
            {
                if (!value)
                {
                    sizeOld = size;
                    posOld = pos;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        ///  Render AA R8 texture from R8 a-channel texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        /// <param name="inputBufferInside"></param>
        /// <param name="inputBufferOutside"></param>
        /// <param name="rebuild"></param>
        public void RenderTextureFromAAFEDTBuffer_AR8(GPUWorker gpuWorker, Texture inputTexture, CBuffers cb, bool rebuild)
        {
            if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
            {
                gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);
            }

            switch (pos)
            {
                case Position.Inside:
                    gpuWorker.GPURenderStrokeFromBuffer_R8(inputTexture, cb.CBEDTAASigned, true, size, ref r_mainTexture);
                    break;

                case Position.Outside:
                    gpuWorker.GPURenderStrokeFromBuffer_R8(inputTexture, cb.CBEDTAASigned, false, size, ref r_mainTexture);
                    break;

            }

            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered  Stroke AAFEDT");
        }

        public int PosNumber
        {
            get
            {
                {
                    switch (pos)
                    {
                        case Position.Inside:
                            return 0;
                        case Position.Outside:
                            return 1;
                        //  case Position.Center:
                        //      return 2;
                        default:
                            return 0;
                    }
                }
            }
        }

        public int FillNumber
        {
            get
            {
                switch (fillType)
                {
                    case FillType.Color:
                        return 0;
                    case FillType.Gradient:
                        return 1;
                    //  case FillType.Pattern:
                    //      return 2;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Return gradient type (0 - linear, 1-Radial, 2-Angle, 3-Reflected, 4-Diamond, 5-ShapeBurst )
        /// </summary>
        public int GradTypeNumber
        {
            get
            {
                switch (gradType)
                {
                    case GradientType.Linear:
                        return 0;
                    case GradientType.Radial:
                        return 1;
                    case GradientType.Angle:
                        return 2;
                    case GradientType.Reflected:
                        return 3;
                    case GradientType.Diamond:
                        return 4;
                    case GradientType.ShapeBurst:
                        return 5;
                    default:
                        return 0;
                }
            }
        }
    }

    [Serializable]
    public class OuterGlowOptions : Options
    {
        public enum FillType { Color, Gradient }
        public enum Method { Precize, Soft }

        public int size = 2;
        public float spread;
        public int range = 50;
        public int sizeOld = 2;
        public float spreadOld;
        public int rangeOld;
        public Method method = Method.Precize;
        public Method methodOld = Method.Precize;

        public BlendMode blendMode = BlendMode.Normal;
        public FillType fillType = FillType.Color;
        public Color color = Color.white;
        public Gradient gradient;
        BoxBlurAndDilateSettings bd;
        public AnimationCurve contour = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve contourOld = AnimationCurve.Linear(0, 0, 1, 1);
        public int gradPosition = 3; // position in gradients

        [SerializeField]
        private float[] contourBuffer;

        public float[] ContourBuffer
        {
            get
            {
                if (!CurvesEqual(contourOld, contour) || contourBuffer == null || contourBuffer.Length == 0)
                {
                    CreateCurveBuffer(contour, ref contourBuffer);
                }
                return contourBuffer;
            }
        }

        public bool IsDirty
        {
            get { return (use && (size != sizeOld || spread != spreadOld || method != methodOld)) || (use != useOld); }
            set
            {
                if (!value)
                {
                    sizeOld = size;
                    spreadOld = spread;
                    methodOld = method;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        /// Render AA R8 texture from RGBA a-channel texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        /// <param name="inputBufferOutside"></param>
        /// <param name="rebuild"></param>
        public void RenderTextureFromAAFEDT_AR8(GPUWorker gpuWorker, Texture inputTexture, CBuffers cb, bool rebuild)
        {
            if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
            {
                gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);
            }
            switch (method)
            {
                case Method.Precize:
                    gpuWorker.GPURenderOuterGlowPrecFromBuffer_R8(inputTexture, cb.CBEDTAASigned, size, spread, ref r_mainTexture);
                    break;

                case Method.Soft:
                    bd = new BoxBlurAndDilateSettings(size, spread / 100f);
                    gpuWorker.GPURenderOuterGlowSoftFromBuffer_R8(inputTexture, cb.CBEDTAASigned, bd.getDilatePixels(), bd.getBoxBlurRadius(), ref r_mainTexture);
                    break;
            }

            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered Outer Glow R8 render texture from float buffer.");
        }

        public int FillNumber
        {
            get
            {
                switch (fillType)
                {
                    case FillType.Color:
                        return 0;
                    case FillType.Gradient:
                        return 1;
                    default:
                        return 0;
                }
            }
        }

        private class BoxBlurAndDilateSettings
        {
            private int m_enlargePixels;
            private int m_dilatePixels;
            private float m_boxBlurRadius;

            public BoxBlurAndDilateSettings(int sizeInPixels, float spread)
            {
                m_enlargePixels = sizeInPixels + 2;
                m_dilatePixels = (int)(sizeInPixels * spread + 0.5f);

                int blurPixels = sizeInPixels - m_dilatePixels;

                // Photoshop fudge factor by Brian Fiete
                float fudge = 1.85f - 0.45f * Mathf.Min(1.0f, blurPixels / 10.0f);
                m_boxBlurRadius = Mathf.Max(blurPixels - fudge, 0.0f);
            }

            public int getEnlargePixels()
            {
                return m_enlargePixels;
            }

            public int getDilatePixels()
            {
                return m_dilatePixels;
            }

            public float getBoxBlurRadius()
            {
                return m_boxBlurRadius;
            }
        }
    }

    [Serializable]
    public class CloseShadowOptions : Options
    {
        public int blur = 2; //0 - 250px
        public Color color = Color.red;
        public int blurOld;

        public float OffsetX;
        public float OffsetY;
        public float angle = 90; // 0 - 360
        public float offset = 1; // 0-250px
        public float angleOld; // 0 - 360
        public float offsetOld; // 0-250px
        public float spread; // 0 - 100
        public float spreadOld;
        public AnimationCurve contour = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve contourOld = AnimationCurve.Linear(0, 0, 1, 1);
        //  public Texture2D contourTexture;
        public float noise;
        public float noiseOld;
        // public Texture2D noiseTexture;
        // public Texture2D noiseTextureOld;

        private float dSin;
        private float dCos;

        public float DSin
        {
            get { return dSin; }
        }

        public float DCos
        {
            get { return dCos; }
        }


        [SerializeField]
        private float[] contourBuffer;

        public float[] ContourBuffer
        {
            get
            {
                if (!CurvesEqual(contourOld, contour) || contourBuffer == null || contourBuffer.Length == 0)
                {
                    CreateCurveBuffer(contour, ref contourBuffer);
                }
                return contourBuffer;
            }
        }

        public bool IsDirty
        {
            get { return (use && (blur != blurOld || spread != spreadOld)) || (use != useOld); }
            set
            {
                if (!value)
                {
                    blurOld = blur;
                    spreadOld = spread;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        /// Render R8 texture from a-channel of input texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        public void RenderTexture_AR8(GPUWorker gpuWorker, Texture inputTexture)
        {
            RenderTexture tempTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.R8);
            gpuWorker.GPURenderBoxBlurTexture_AR8(inputTexture, ref tempTexture, blur, spread);
            gpuWorker.GPURenderBoxBlurTexture_R8(tempTexture, ref r_mainTexture, blur, spread);
            tempTexture.Release();
            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered CloseShadow from alpha to R8 render texture.");
        }

        public void DsinCos()
        {
            float rad = Mathf.Deg2Rad * angle;
            dCos = Mathf.Cos(rad) * offset;
            dSin = Mathf.Sin(rad) * offset;
        }
    }

    [Serializable]
    public class ShadowOptions : Options
    {
        public int blur; //0 - 250px
        public Color color = Color.black;
        public int blurOld;

        public float OffsetX;
        public float OffsetY;
        public float angle; // 0 - 360
        public float offset; // 0-250px
        public float angleOld; // 0 - 360
        public float offsetOld; // 0-250px
        public float spread; // 0 - 100
        public float spreadOld;
        public AnimationCurve contour = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve contourOld = AnimationCurve.Linear(0, 0, 1, 1);
        public Texture2D contourTexture;
        public float noise;
        public float noiseOld;
        public Texture2D noiseTexture;
        public Texture2D noiseTextureOld;

        public bool IsDirty
        {
            get { return (use && (use != useOld || blur != blurOld || spread != spreadOld || !CurvesEqual(contour, contourOld) || noise != noiseOld)); }
            set
            {
                if (!value)
                {
                    blurOld = blur;
                    spreadOld = spread;
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        /// Render ARGB32 from RGBA texture 
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        public void RenderTexture(GPUWorker gpuWorker, Texture inputTexture)
        {
            RenderTexture tempTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.ARGB32);
            gpuWorker.GPURenderBoxBlurTexture(inputTexture, ref tempTexture, blur, spread);
            gpuWorker.GPURenderBoxBlurTexture(tempTexture, ref r_mainTexture, blur, spread);
            tempTexture.Release();
            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Render Shadow ARGB render texture.");
        }

        /// <summary>
        /// Render R8 texture from r-channel of input texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        public void RenderTexture_R8(GPUWorker gpuWorker, Texture inputTexture)
        {
            RenderTexture tempTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.R8);
            gpuWorker.GPURenderBoxBlurTexture_R8(inputTexture, ref tempTexture, blur, spread);
            gpuWorker.GPURenderBoxBlurTexture_R8(tempTexture, ref r_mainTexture, blur, spread);
            tempTexture.Release();
            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered Shadow R8 render texture.");
        }

        /// <summary>
        /// Render R8 texture from a-channel of input texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        public void RenderTexture_AR8(GPUWorker gpuWorker, Texture inputTexture)
        {
            RenderTexture tempTexture = new RenderTexture(1, 1, 0, RenderTextureFormat.R8);
            RenderTexture tempTexture_1 = new RenderTexture(1, 1, 0, RenderTextureFormat.R8);
            gpuWorker.GPURenderBoxBlurTexture_AR8(inputTexture, ref tempTexture, blur, spread);
            gpuWorker.GPURenderBoxBlurTexture_R8(tempTexture, ref tempTexture_1, blur, spread);
            SetCombinedOptions();
            gpuWorker.GPUCombinerShadow(tempTexture_1, this);
            tempTexture.Release();
            tempTexture_1.Release();
            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered Shadow from alpha to R8 render texture.");
        }

        public void SetCombinedOptions()
        {
            if (!CurvesEqual(contour, contourOld) || contourTexture == null)
            {
                if (SoftEffects.debuglog) Debug.Log("Set Shadow material property (contour ) ");
                CreateCurveTexture(contour, ref contourTexture);
                contourOld.keys = contour.keys;
            }

            if (noise != noiseOld && noise > 0)
            {
                CreateNoiseTexture(ref noiseTexture, new int2(256, 256));
                noiseOld = noise;
            }

        }

        /// <summary>
        ///  Set new gpu rendered shadow texture to material or disable shadow
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterialPoperty(Material material)
        {
            if (use != (material.GetInt("_UseShadows") == 1))
            {
                if (SoftEffects.debuglog) Debug.Log("Set shadow material property (use: ) " + use);
                material.SetInt("_UseShadows", use ? 1 : 0);
                EditorUtility.SetDirty(material);
            }
            if (use)
            {
                if (color != material.GetColor("_ShadowColor"))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set shadow material property (color ) ");
                    material.SetColor("_ShadowColor", color);
                    EditorUtility.SetDirty(material);
                }

                if (material.GetTexture("_ShadowTex") != r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Shadow material texture ");
                    if (r_mainTexture == null) if (SoftEffects.debuglog) Debug.Log("Shadow render texture == null ");
                    material.SetTexture("_ShadowTex", r_mainTexture);
                    EditorUtility.SetDirty(material);
                }

                if (angleOld != angle || offsetOld != offset || OffsetX != material.GetFloat("_OffsetX") || OffsetY != material.GetFloat("_OffsetY"))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Shadow material property (position ) ");
                    OffsetX = -offset * Mathf.Cos(Mathf.Deg2Rad * angle);
                    OffsetY = -offset * Mathf.Sin(Mathf.Deg2Rad * angle);
                    material.SetFloat("_OffsetX", OffsetX);
                    material.SetFloat("_OffsetY", OffsetY);

                    angleOld = angle;
                    offsetOld = offset;
                    EditorUtility.SetDirty(material);
                }
            }
        }

        public void SaveTexturePanel()
        {
            SaveTexturePanel("", "newtexture");

        }

        public void SaveTexturePanel(string folder)
        {
            SaveTexturePanel(folder, "newtexture");
        }

        /// <summary>
        /// https://docs.unity3d.com/ScriptReference/EditorUtility.SaveFilePanel.html
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        public void SaveTexturePanel(string folder, string fileName)
        {
            Texture2D t = new Texture2D(1, 1);
            r_mainTexture.CreateTextureFromRender_ARGB32(ref t, false, "");

            var path = EditorUtility.SaveFilePanel(
                    "Save texture as PNG",
                    folder,
                   fileName,
                    "png");

            if (path.Length != 0)
            {
                var pngData = t.EncodeToPNG();
                if (pngData != null)
                    File.WriteAllBytes(path, pngData);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Debug.Log(AssetDatabase.GetAssetPath(t));
            // ClassExtensions.ReimportTexture(path, false);
            // AssetDatabase.SaveAssets();
            //  AssetDatabase.Refresh();
        }

        /// <summary>
        /// Save working texture directly to edit folder
        /// </summary>
        /// <param name="assetPath"></param>
        public void SaveTexture(string assetPath)
        {
            if (assetPath == null || assetPath == "")
            {
                if (SoftEffects.debuglog) Debug.Log("No saving path");
                return;
            }
            Texture2D t = new Texture2D(1, 1);
            r_mainTexture.CreateTextureFromRender_ARGB32(ref t, true, assetPath);
        }
    }

    [Serializable]
    public class FaceGradientOptions : Options
    {
        public enum GradientType { Linear, Radial, Angle, Reflected, Diamond, ShapeBurst }
        public GradientType gradType = GradientType.Linear;
        public enum GBlendMode { Normal, Lighten, Screen, Overlay, Darken, Multiplay }
        public GBlendMode gBlendMode = GBlendMode.Normal;
        public Gradient gradient;
        public float angle = 0; // gradient angle
        public float scale = 100;
        public int gradPosition = 2; // position in gradients

        public bool IsDirty
        {
            get { return (use && (use != useOld)); }
            set
            {
                if (!value)
                {
                    useOld = use;
                }
            }
        }

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }


        /// <summary>
        ///  Render AA R8 texture from R8 a-channel texture
        /// </summary>
        /// <param name="gpuWorker"></param>
        /// <param name="inputTexture"></param>
        /// <param name="inputBufferInside"></param>
        /// <param name="inputBufferOutside"></param>
        /// <param name="rebuild"></param>
        public void RenderTextureFromAAFEDTBuffer_AR8(GPUWorker gpuWorker, Texture inputTexture, CBuffers cb, bool rebuild)
        {
            if (!CheckBuffer(cb.CBEDTAASigned) || rebuild)
                gpuWorker.GPURenderAAFEDTSignedBuffer(inputTexture, ref cb.CBEDTAASigned);

            gpuWorker.Normalize_R8(cb.CBEDTAASigned, ref r_mainTexture, inputTexture.width, inputTexture.height, 0, 1, true);

            IsDirty = false;
            if (SoftEffects.debuglog) Debug.Log("Rendered  Face Gradient AAFEDT");
        }

        /// <summary>
        /// Return gradient type (0 - linear, 1-Radial, 2-Angle, 3-Reflected, 4-Diamond, 5-ShapeBurst )
        /// </summary>
        public int GradTypeNumber
        {
            get
            {
                switch (gradType)
                {
                    case GradientType.Linear:
                        return 0;
                    case GradientType.Radial:
                        return 1;
                    case GradientType.Angle:
                        return 2;
                    case GradientType.Reflected:
                        return 3;
                    case GradientType.Diamond:
                        return 4;
                    case GradientType.ShapeBurst:
                        return 5;
                    default:
                        return 0;
                }
            }
        }

        public int GradientBlendmodeNumber
        {
            get
            {
                switch (gBlendMode)
                {
                    case GBlendMode.Normal:
                        return 0;
                    case GBlendMode.Lighten:
                        return 1;
                    case GBlendMode.Screen:
                        return 2;
                    case GBlendMode.Overlay:
                        return 3;
                    case GBlendMode.Darken:
                        return 4;
                    case GBlendMode.Multiplay:
                        return 5;
                    default:
                        return 0;
                }
            }
        }
    }

    [Serializable]
    public class FaceOptions : OptionsMain
    {
        public RenderTexture r_combinedTexture;
        private int2[] inputUV;
        public int2[] outputUV;
        public int extPixels = 5;
        public float pixelsPerUnit = 1;

        // options for combinig - last updated
        public CloseShadowOptions csOptions;
        public CloseShadowOptions csOptions_1;
        public OuterGlowOptions ogOptions;
        public StrokeOptions strOptions;
        public BevelOptions bevOptions;
        public InnerGlowOptions igOptions;
        public InnerShadowOptions isOptions;
        public FaceGradientOptions fgOptions;

        // pattern options
        public enum PBlendMode { Normal, Lighten, Screen, Overlay, Darken, Multiplay }
        public PBlendMode pBlendMode = PBlendMode.Normal;
        public PBlendMode pBlendModeOld = PBlendMode.Normal;
        public Texture2D patternText;
        public Texture2D patternTextOld;
        public int pOpacity = 100;
        public int pOpacityOld = 100;
        public float pScale = 100;
        public float pScaleOld = 100;
        public bool usePattern = false;
        public bool usePatternOld = false;

        // color options
        public enum CBlendMode { Normal, Lighten, Screen, Overlay, Darken, Multiplay }
        public Color fColor = Color.white;
        public Color fColorOld = Color.white;
        public CBlendMode cBlendMode = CBlendMode.Normal;
        public CBlendMode cBlendModeOld = CBlendMode.Normal;
        public bool useColor = false;
        public bool useColorOld = false;

        public bool IsCombinedDirty = false;

        public bool NeedRebuild
        {
            get { return (use && (r_mainTexture == null || !r_mainTexture.IsCreated())); }
        }

        /// <summary>
        /// Set shader  _MainEditTex field, and set material dirty
        /// </summary>
        /// <param name="material"></param>
        public void SetMaterialPoperty(Material material)
        {

            if (r_combinedTexture != null && r_combinedTexture.IsCreated())
            {
                if (material.GetTexture("_MainEditTex") != r_combinedTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Face material texture ");
                    material.SetTexture("_MainEditTex", r_combinedTexture);
                    EditorUtility.SetDirty(material);
                }
            }
            else if (r_combinedTexture == null)
            {
                if (material.GetTexture("_MainEditTex") != mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Face material texture ");
                    material.SetTexture("_MainEditTex", mainTexture);
                    EditorUtility.SetDirty(material);
                }
            }

        }

        /// <summary>
        /// Create new texture for editable font with oriented symbols on GPU
        /// </summary>
        internal void RenderFontTexture(GPUWorker gWorker, Font SoftFont, CBuffers cb)
        {
            if (!SoftFont) return;

            CharacterInfo[] s = SoftFont.characterInfo;
            CharacterInfo[] ch = new CharacterInfo[s.Length];
            Texture fontTexture = SoftFont.material.mainTexture;
            int tWidth = fontTexture.width;
            int tHeight = fontTexture.height;
            float tWf = (float)tWidth;
            float tHf = (float)tHeight;

            Packer bucket = new Packer(ch.Length);
            inputUV = new int2[ch.Length * 3];
            outputUV = new int2[ch.Length * 3];

            if (SoftEffects.debuglog) Debug.Log(SoftFont.name + " - font chars length: " + s.Length);

            SymbImage imgToPacker;
            CharacterInfo chInfo;
            Data dat;
            Vector2 uv_bot_left, uv_top_right, uv_bot_right, uv_top_left;
            int2 V0, V1, V2, VE01, VE12;
            int LVE01, LVE12, p;
            VertsAsset va = new VertsAsset();

            // Mkey.Utils.Measure("Creating symbols: ", () =>  {
            for (int i = 0; i < s.Length; i++)
            {
                chInfo = s[i];

                //1) get symb uvs
                uv_bot_left = chInfo.uvBottomLeft;
                uv_top_right = chInfo.uvTopRight;
                uv_bot_right = chInfo.uvBottomRight;
                uv_top_left = chInfo.uvTopLeft; //Debug.Log(uv_bot_left + " : " + uv_bot_right + " : " + uv_top_right);

                //2) get vertex positions on texture 
                va.BL = uv_bot_left;
                va.BR = uv_bot_right;
                va.TR = uv_top_right;
                va.CalcPositions(tWf, tHf);
                V0 = va.V0;
                V1 = va.V1;
                V2 = va.V2;

                //3) save vertex positions
                p = i * 3;
                inputUV[p] = V0;  //bl
                inputUV[p + 1] = V1; //br 
                inputUV[p + 2] = V2; //tr  Debug.Log(inputUV[p] + " : " + inputUV[p + 1] + " : " + inputUV[p + 2]);

                //4) add source symbols to packer
                VE01 = V1 - V0;
                VE12 = V2 - V1;
                LVE01 = VE01.ChessLength;
                LVE12 = VE12.ChessLength;

                imgToPacker = new SymbImage(LVE01, LVE12);

                dat.sx = 0;
                dat.sy = 0;
                dat.ex = imgToPacker.size0 - 1;
                dat.ey = imgToPacker.size1 - 1;// dat.ox = ch[i].bearing;dat.oy = 0;dat.wx = ch[i].advance;dat.id = i;

                bucket.AddItem(imgToPacker, dat);

                ch[i].advance = chInfo.advance;
                ch[i].bearing = chInfo.bearing;
                ch[i].glyphHeight = chInfo.glyphHeight;
                ch[i].glyphWidth = chInfo.glyphWidth;
                ch[i].index = chInfo.index;
                ch[i].maxX = chInfo.maxX;
                ch[i].maxY = chInfo.maxY;
                ch[i].minX = chInfo.minX;
                ch[i].minY = chInfo.minY;
                ch[i].size = chInfo.size;
                ch[i].style = chInfo.style;
                /*     */

            } // end symbols creating
              //});

            //7) create new texure for EditableFont
            int image_width = fontTexture.width;
            int image_height = fontTexture.height * 2;
            SymbImage[] symbData = null;

            //  Mkey.Utils.Measure("Resolve: ", () => {
            symbData = bucket.ResolveExtendBool(ref image_width, ref image_height);
            //  });

            Data symbD;
            int oI;
            Vector2 uv;
            float i_w = image_width;
            float i_h = image_height;
            float sxdw, exdw, sydh, eydh;
            int2 tPos;

            for (int si = 0; si < symbData.Length; si++)
            {
                symbD = symbData[si].d;
                oI = symbData[si].oldOrder;
                sxdw = symbD.sx / i_w;
                exdw = (symbD.ex + 1) / i_w; // +1 : for UV correcting
                sydh = symbD.sy / i_h;
                eydh = (symbD.ey + 1) / i_h; // +1 : for UV correcting

                uv.x = sxdw; uv.y = sydh;
                ch[oI].uvBottomLeft = uv;
                uv.x = exdw; uv.y = sydh;
                ch[oI].uvBottomRight = uv;
                uv.x = sxdw; uv.y = eydh;
                ch[oI].uvTopLeft = uv;
                uv.x = exdw; uv.y = eydh;
                ch[oI].uvTopRight = uv; //Debug.Log(ch[oI].uvBottomLeft + " : " + ch[oI].uvBottomRight + " : " + ch[oI].uvTopRight);

                p = oI * 3;
                tPos.X = symbD.sx; tPos.Y = symbD.sy;
                outputUV[p] = tPos; //bl
                tPos.X = symbD.ex; tPos.Y = symbD.sy;
                outputUV[p + 1] = tPos; //br
                tPos.X = symbD.ex; tPos.Y = symbD.ey;
                outputUV[p + 2] = tPos;//tr  Debug.Log(outputUV[p] + " : " + outputUV[p+1] + " : " + outputUV[p+2]);
            }

            if (SoftEffects.debuglog) Debug.Log("image_width: " + image_width + " ; image_height: " + image_height);

            SoftFont.characterInfo = ch;

            //8) render texture 
            //Mkey.Utils.Measure("gWorker.GPUCopyTextureUV: ", () =>  {
            gWorker.GPUCopyTextureUV(inputUV, outputUV, new int2(image_width, image_height), fontTexture, ref r_mainTexture);
            //});

            //Mkey.Utils.Measure("CreateUVMapBuffer: ", () =>  {
            CreateUVMapBuffer(r_mainTexture.width, r_mainTexture.height, cb);
            //});
        }

        /// <summary>
        /// Create new texture for editable sprite 
        /// </summary>
        internal void RenderSpriteTexture(GPUWorker gWorker, Texture inputTexture, CBuffers cb)
        {
            int tWidth = inputTexture.width;
            int tHeight = inputTexture.height;

            //7) create new texure for EditableFont

            if (SoftEffects.debuglog) Debug.Log("Input sprite width: " + tWidth + " ; image_height: " + tHeight);
            int image_width = tWidth + 2 * extPixels;
            int image_height = tHeight + 2 * extPixels;
            if (SoftEffects.debuglog) Debug.Log("Output sprite width: " + image_width + " ; image_height: " + image_height);

            //8) create input  and output UVs arrays - 3 uv for each symbol for GPU rendering
            inputUV = new int2[3];
            outputUV = new int2[3];

            inputUV[0] = new int2(0, 0);
            inputUV[1] = new int2(tWidth, 0);
            inputUV[2] = new int2(tWidth, tHeight);

            outputUV[0] = new int2(extPixels, extPixels);
            outputUV[1] = new int2(tWidth + extPixels, extPixels);
            outputUV[2] = new int2(tWidth + extPixels, tHeight + extPixels);

            //9) render texture 
            gWorker.GPUCopyTextureUV(inputUV, outputUV, new int2(image_width, image_height), inputTexture, ref r_mainTexture);
            outputUV[0] = new int2(0, 0);
            outputUV[1] = new int2(image_width, 0);
            outputUV[2] = new int2(image_width, image_height);
            CreateUVMapBuffer(r_mainTexture.width, r_mainTexture.height, cb);
        }

        /// <summary>
        /// Set inner shader options for compute shader
        /// </summary>
        /// <param name="closeShadowOptions"></param>
        internal void SetCloseShadowOptions(CloseShadowOptions closeShadowOptions)
        {
            CloseShadowOptions source = closeShadowOptions;
            CloseShadowOptions target = csOptions;

            if (source.use != target.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (use: ) " + source.use);
                target.use = source.use;
                IsCombinedDirty = true;
            }

            if (target.use)
            {
                if (target.color != source.color)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (color ) ");
                    target.color = source.color;
                    IsCombinedDirty = true;
                }

                if (target.spread != source.spread)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (spread ) " + source.spread);
                    target.spread = source.spread;
                    IsCombinedDirty = true;
                }

                if (target.blur != source.blur)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (size ) " + source.blur);
                    target.blur = source.blur;
                    IsCombinedDirty = true;
                }

                if (target.angle != source.angle)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (angle ) " + source.angle);
                    target.angle = source.angle;
                    target.DsinCos();
                    IsCombinedDirty = true;
                }

                if (target.offset != source.offset)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (distance ) " + source.offset);
                    target.offset = source.offset;
                    target.DsinCos();
                    IsCombinedDirty = true;
                }

                if (!CurvesEqual(target.contour, source.contour))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (contour ) ");
                    target.contour.keys = source.contour.keys;
                    IsCombinedDirty = true;
                }

                if (target.r_mainTexture != source.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (texture ) ");
                    target.r_mainTexture = source.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Set inner shader options for compute shader
        /// </summary>
        /// <param name="closeShadowOptions"></param>
        internal void SetCloseShadowOptions_1(CloseShadowOptions closeShadowOptions)
        {
            CloseShadowOptions source = closeShadowOptions;
            CloseShadowOptions target = csOptions_1;

            if (source.use != target.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (use: ) " + source.use);
                target.use = source.use;
                IsCombinedDirty = true;
            }

            if (target.use)
            {
                if (target.color != source.color)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (color ) ");
                    target.color = source.color;
                    IsCombinedDirty = true;
                }

                if (target.spread != source.spread)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (spread ) " + source.spread);
                    target.spread = source.spread;
                    IsCombinedDirty = true;
                }

                if (target.blur != source.blur)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (size ) " + source.blur);
                    target.blur = source.blur;
                    IsCombinedDirty = true;
                }

                if (target.angle != source.angle)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (angle ) " + source.angle);
                    target.angle = source.angle;
                    target.DsinCos();
                    IsCombinedDirty = true;
                }

                if (target.offset != source.offset)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (distance ) " + source.offset);
                    target.offset = source.offset;
                    target.DsinCos();
                    IsCombinedDirty = true;
                }

                if (!CurvesEqual(target.contour, source.contour))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set CloseShadow face property (contour ) ");
                    target.contour.keys = source.contour.keys;
                    IsCombinedDirty = true;
                }

                if (target.r_mainTexture != source.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (texture ) ");
                    target.r_mainTexture = source.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Set outer glow options for compute shader
        /// </summary>
        /// <param name="outerGlowOptions"></param>
        internal void SetOuterGlowOptions(OuterGlowOptions outerGlowOptions, CBuffers cb)
        {
            ogOptions.size = outerGlowOptions.size;
            if (outerGlowOptions.use != ogOptions.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (use: ) " + outerGlowOptions.use);
                ogOptions.use = outerGlowOptions.use;
                IsCombinedDirty = true;
            }

            if (ogOptions.use)
            {
                if (ogOptions.fillType != outerGlowOptions.fillType)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (fillType ) " + outerGlowOptions.fillType);
                    ogOptions.fillType = outerGlowOptions.fillType;
                    IsCombinedDirty = true;
                }

                switch (ogOptions.fillType)
                {
                    case OuterGlowOptions.FillType.Color:
                        if (ogOptions.color != outerGlowOptions.color)
                        {
                            if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (color ) ");
                            ogOptions.color = outerGlowOptions.color;
                            IsCombinedDirty = true;
                        }
                        break;
                    case OuterGlowOptions.FillType.Gradient:
                        if (!GradientsEqual(ogOptions.gradient, outerGlowOptions.gradient) || cb.gradientsArrayOGlow == null)
                        {
                            if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (gradient ) ");
                            if (ogOptions.gradient == null) ogOptions.gradient = new Gradient();
                            if (outerGlowOptions.gradient == null) outerGlowOptions.gradient = new Gradient();
                            cb.CreateGradientArray(outerGlowOptions.gradient, outerGlowOptions.gradPosition);
                            ogOptions.gradient.SetKeys(outerGlowOptions.gradient.colorKeys, outerGlowOptions.gradient.alphaKeys);
                            ogOptions.gradient.mode = outerGlowOptions.gradient.mode;
                            IsCombinedDirty = true;
                        }
                        break;
                }

                if (ogOptions.range != outerGlowOptions.range)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (range ) " + outerGlowOptions.range);
                    ogOptions.range = outerGlowOptions.range;
                    IsCombinedDirty = true;
                }

                if (!CurvesEqual(ogOptions.contour, outerGlowOptions.contour))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (contour ) ");
                    ogOptions.contour.keys = outerGlowOptions.contour.keys;
                    IsCombinedDirty = true;
                }

                if (ogOptions.r_mainTexture != outerGlowOptions.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set OuterGlow face property (texture ) ");
                    ogOptions.r_mainTexture = outerGlowOptions.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }


        }

        /// <summary>
        /// Set stroke options for compute shader
        /// </summary>
        /// <param name="strokeOptions"></param>
        internal void SetStrokeOptions(StrokeOptions strokeOptions, CBuffers cb)
        {
            strOptions.size = strokeOptions.size;
            if (strokeOptions.use != strOptions.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set Stroke face property (use: ) " + strokeOptions.use);
                strOptions.use = strokeOptions.use;
                IsCombinedDirty = true;
            }
            if (strOptions.use)
            {
                if (strOptions.pos != strokeOptions.pos)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set stroke face property (position ) ");
                    strOptions.pos = strokeOptions.pos;
                    IsCombinedDirty = true;
                }

                if (strOptions.fillType != strokeOptions.fillType)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Stroke face property (fillType ) " + strokeOptions.fillType);
                    strOptions.fillType = strokeOptions.fillType;
                    IsCombinedDirty = true;
                }

                switch (strOptions.fillType)
                {
                    case StrokeOptions.FillType.Color:
                        if (strOptions.color != strokeOptions.color)
                        {
                            if (SoftEffects.debuglog) Debug.Log("Set stroke face property (color ) ");
                            strOptions.color = strokeOptions.color;
                            IsCombinedDirty = true;
                        }
                        break;
                    case StrokeOptions.FillType.Gradient:
                        if (!GradientsEqual(strOptions.gradient, strokeOptions.gradient) || cb.gradientsArrayStroke == null)
                        {
                            if (SoftEffects.debuglog) Debug.Log("Set Stroke face property (gradient ) ");
                            if (strOptions.gradient == null) strOptions.gradient = new Gradient();
                            if (strokeOptions.gradient == null) strokeOptions.gradient = new Gradient();
                            cb.CreateGradientArray(strokeOptions.gradient, strokeOptions.gradPosition);
                            strOptions.gradient.SetKeys(strokeOptions.gradient.colorKeys, strokeOptions.gradient.alphaKeys);
                            strOptions.gradient.mode = strokeOptions.gradient.mode;
                            IsCombinedDirty = true;
                        }
                        break;
                        /*
                                        case StrokeOptions.FillType.Pattern:
                                            if (strOptions.pattern != strokeOptions.pattern)
                                            {
                                                Debug.Log("Set Stroke face property (pattern ) ");
                                                strOptions.pattern = strokeOptions.pattern;
                                                IsCombinedDirty = true;
                                            }
                                            break;
                                            */
                }

                if (strOptions.fillType == StrokeOptions.FillType.Gradient)
                {
                    if (strOptions.gradType != strokeOptions.gradType)
                    {
                        if (SoftEffects.debuglog) Debug.Log("Set stroke face property (gradient type ) ");
                        strOptions.gradType = strokeOptions.gradType;
                        IsCombinedDirty = true;
                    }

                    if (strOptions.angle != strokeOptions.angle)
                    {
                        if (SoftEffects.debuglog) Debug.Log("Set stroke face property (angle ) ");
                        strOptions.angle = strokeOptions.angle;
                        IsCombinedDirty = true;
                    }
                }

                if (strOptions.r_mainTexture != strokeOptions.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Stroke face property texture ");
                    strOptions.r_mainTexture = strokeOptions.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Set bevel options for compute shader
        /// </summary>
        /// <param name="bevelOptions"></param>
        internal void SetBevelOptions(BevelOptions bevelOptions)
        {
            bevOptions.size = bevelOptions.size;
            if (bevelOptions.use != bevOptions.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (use: ) " + bevelOptions.use);
                bevOptions.use = bevelOptions.use;
                IsCombinedDirty = true;
            }

            if (bevOptions.use)
            {
                if (bevOptions.angle != bevelOptions.angle)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set bevel face property (angle ) " + bevelOptions.angle);
                    bevOptions.angle = bevelOptions.angle;
                    IsCombinedDirty = true;
                }

                if (bevOptions.lightAltitude != bevelOptions.lightAltitude)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (light altitude ) " + bevelOptions.lightAltitude);
                    bevOptions.lightAltitude = bevelOptions.lightAltitude;
                    IsCombinedDirty = true;
                }

                if (bevOptions.depth != bevelOptions.depth)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (depth ) " + bevelOptions.depth);
                    bevOptions.depth = bevelOptions.depth;
                    IsCombinedDirty = true;
                }

                if (bevOptions.bTechnique != bevelOptions.bTechnique)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (technique ) " + bevelOptions.bTechnique);
                    bevOptions.bTechnique = bevelOptions.bTechnique;
                    IsCombinedDirty = true;
                }

                if (bevOptions.bStyle != bevelOptions.bStyle)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (style ) " + bevelOptions.bStyle);
                    bevOptions.bStyle = bevelOptions.bStyle;
                    IsCombinedDirty = true;
                }

                if (bevOptions.lightBlendMode != bevelOptions.lightBlendMode)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (lightBlendMode ) " + bevelOptions.lightBlendMode);
                    bevOptions.lightBlendMode = bevelOptions.lightBlendMode;
                    IsCombinedDirty = true;
                }

                if (bevOptions.lightColor != bevelOptions.lightColor)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (lightColor ) " + bevelOptions.lightColor);
                    bevOptions.lightColor = bevelOptions.lightColor;
                    IsCombinedDirty = true;
                }

                if (bevOptions.shadowBlendMode != bevelOptions.shadowBlendMode)
                {
                    Debug.Log("Set Bevel face property (shadowBlendMode ) " + bevelOptions.shadowBlendMode);
                    bevOptions.shadowBlendMode = bevelOptions.shadowBlendMode;
                    IsCombinedDirty = true;
                }

                if (bevOptions.shadowColor != bevelOptions.shadowColor)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (shadowColor ) " + bevelOptions.shadowColor);
                    bevOptions.shadowColor = bevelOptions.shadowColor;
                    IsCombinedDirty = true;
                }

                if (bevOptions.smoothing != bevelOptions.smoothing)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (smoothing ) " + bevelOptions.smoothing);
                    bevOptions.smoothing = bevelOptions.smoothing;
                    IsCombinedDirty = true;
                }

                if (!CurvesEqual(bevOptions.contour, bevelOptions.contour))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set Bevel face property (contour ) ");
                    bevOptions.contour.keys = bevelOptions.contour.keys;
                    IsCombinedDirty = true;
                }

                if (bevOptions.r_mainTexture != bevelOptions.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set bevel face property light texture ");
                    bevOptions.r_mainTexture = bevelOptions.r_mainTexture;
                    IsCombinedDirty = true;
                }

                if (bevOptions.r_shadowTexture != bevelOptions.r_shadowTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set bevel face property shadow texture ");
                    bevOptions.r_shadowTexture = bevelOptions.r_shadowTexture;
                    IsCombinedDirty = true;
                }
            }

        }

        /// <summary>
        /// Set innner glow options for compute shader
        /// </summary>
        /// <param name="innerGlowOptions"></param>
        internal void SetInnerGlowOptions(InnerGlowOptions innerGlowOptions, CBuffers cb)
        {
            igOptions.size = innerGlowOptions.size;
            if (innerGlowOptions.use != igOptions.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (use: ) " + innerGlowOptions.use);
                igOptions.use = innerGlowOptions.use;
                IsCombinedDirty = true;
            }

            if (igOptions.use)
            {
                if (igOptions.fillType != innerGlowOptions.fillType)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (fillType ) " + innerGlowOptions.fillType);
                    igOptions.fillType = innerGlowOptions.fillType;
                    IsCombinedDirty = true;
                }

                switch (igOptions.fillType)
                {
                    case InnerGlowOptions.FillType.Color:
                        if (igOptions.color != innerGlowOptions.color)
                        {
                            if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (color ) ");
                            igOptions.color = innerGlowOptions.color;
                            IsCombinedDirty = true;
                        }
                        break;
                    case InnerGlowOptions.FillType.Gradient:
                        if (!GradientsEqual(igOptions.gradient, innerGlowOptions.gradient) || cb.gradientsArrayIGlow == null)
                        {
                            if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (gradient ) ");
                            if (igOptions.gradient == null) igOptions.gradient = new Gradient();
                            if (innerGlowOptions.gradient == null) innerGlowOptions.gradient = new Gradient();
                            cb.CreateGradientArray(innerGlowOptions.gradient, innerGlowOptions.gradPosition);
                            igOptions.gradient.SetKeys(innerGlowOptions.gradient.colorKeys, innerGlowOptions.gradient.alphaKeys);
                            igOptions.gradient.mode = innerGlowOptions.gradient.mode;
                            IsCombinedDirty = true;
                        }
                        break;
                }

                if (igOptions.range != innerGlowOptions.range)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (range ) " + innerGlowOptions.range);
                    igOptions.range = innerGlowOptions.range;
                    IsCombinedDirty = true;
                }

                if (igOptions.spread != innerGlowOptions.spread)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (spread ) " + innerGlowOptions.spread);
                    igOptions.spread = innerGlowOptions.spread;
                    IsCombinedDirty = true;
                }

                if (!CurvesEqual(igOptions.contour, innerGlowOptions.contour))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (contour ) ");
                    igOptions.contour.keys = innerGlowOptions.contour.keys;
                    IsCombinedDirty = true;
                }

                if (igOptions.blendMode != innerGlowOptions.blendMode)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (blendMode ) " + innerGlowOptions.blendMode);
                    igOptions.blendMode = innerGlowOptions.blendMode;
                    IsCombinedDirty = true;
                }

                if (igOptions.position != innerGlowOptions.position)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (position ) " + innerGlowOptions.position);
                    igOptions.position = innerGlowOptions.position;
                    IsCombinedDirty = true;
                }

                if (igOptions.r_mainTexture != innerGlowOptions.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerGlow face property (texture ) ");
                    igOptions.r_mainTexture = innerGlowOptions.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Set inner shader options for compute shader
        /// </summary>
        /// <param name="innerShadowOptions"></param>
        internal void SetInnerShadowOptions(InnerShadowOptions innerShadowOptions)
        {
            if (innerShadowOptions.use != isOptions.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (use: ) " + innerShadowOptions.use);
                isOptions.use = innerShadowOptions.use;
                IsCombinedDirty = true;
            }

            if (isOptions.use)
            {
                if (isOptions.color != innerShadowOptions.color)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (color ) ");
                    isOptions.color = innerShadowOptions.color;
                    IsCombinedDirty = true;
                }

                if (isOptions.choke != innerShadowOptions.choke)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (choke ) " + innerShadowOptions.choke);
                    isOptions.choke = innerShadowOptions.choke;
                    IsCombinedDirty = true;
                }

                if (isOptions.size != innerShadowOptions.size)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (size ) " + innerShadowOptions.size);
                    isOptions.size = innerShadowOptions.size;
                    IsCombinedDirty = true;
                }

                if (isOptions.angle != innerShadowOptions.angle)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (choke ) " + innerShadowOptions.angle);
                    isOptions.angle = innerShadowOptions.angle;
                    isOptions.DsinCos();
                    IsCombinedDirty = true;
                }

                if (isOptions.distance != innerShadowOptions.distance)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (distance ) " + innerShadowOptions.distance);
                    isOptions.distance = innerShadowOptions.distance;
                    isOptions.DsinCos();
                    IsCombinedDirty = true;
                }

                if (!CurvesEqual(isOptions.contour, innerShadowOptions.contour))
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (contour ) ");
                    isOptions.contour.keys = innerShadowOptions.contour.keys;
                    IsCombinedDirty = true;
                }

                if (isOptions.blendMode != innerShadowOptions.blendMode)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (blendMode ) " + innerShadowOptions.blendMode);
                    isOptions.blendMode = innerShadowOptions.blendMode;
                    IsCombinedDirty = true;
                }

                if (isOptions.r_mainTexture != innerShadowOptions.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set InnerShadow face property (texture ) ");
                    isOptions.r_mainTexture = innerShadowOptions.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Set face Gradient options for compute shader
        /// </summary>
        /// <param name="faceGradientOptions"></param>
        internal void SetFaceGradientOptions(FaceGradientOptions faceGradientOptions, CBuffers cb)
        {
            if (fgOptions.use != faceGradientOptions.use)
            {
                if (SoftEffects.debuglog) Debug.Log("Set gradient face property (useGradient: ) " + faceGradientOptions.use);
                fgOptions.use = faceGradientOptions.use;
                if (fgOptions.gradient == null) fgOptions.gradient = new Gradient();
                IsCombinedDirty = true;
            }
            if (fgOptions.use)
            {
                if (faceGradientOptions.gradient == null) faceGradientOptions.gradient = new Gradient();
                if (fgOptions.gradient == null) fgOptions.gradient = new Gradient();
                if (!GradientsEqual(fgOptions.gradient, faceGradientOptions.gradient) || fgOptions.scale != faceGradientOptions.scale || cb.gradientsArrayFace == null)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (gradient ) ");
                    cb.CreateGradientArray(faceGradientOptions.gradient, faceGradientOptions.gradPosition, faceGradientOptions.scale * 0.01f);
                    fgOptions.gradient.SetKeys(faceGradientOptions.gradient.colorKeys, faceGradientOptions.gradient.alphaKeys);
                    fgOptions.gradient.mode = faceGradientOptions.gradient.mode;
                    IsCombinedDirty = true;
                }

                if (fgOptions.gradType != faceGradientOptions.gradType)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (gradient type ) ");
                    fgOptions.gradType = faceGradientOptions.gradType;
                    IsCombinedDirty = true;
                }

                if (fgOptions.angle != faceGradientOptions.angle)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (gradient angle ) ");
                    fgOptions.angle = faceGradientOptions.angle;
                    IsCombinedDirty = true;
                }

                if (fgOptions.scale != faceGradientOptions.scale)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (gradient scale ) ");
                    fgOptions.scale = faceGradientOptions.scale;
                    IsCombinedDirty = true;
                }

                if (fgOptions.gBlendMode != faceGradientOptions.gBlendMode)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (gradient blend mode) ");
                    fgOptions.gBlendMode = faceGradientOptions.gBlendMode;
                    IsCombinedDirty = true;
                }

                if (fgOptions.r_mainTexture != faceGradientOptions.r_mainTexture)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face gardient property  texture ");
                    fgOptions.r_mainTexture = faceGradientOptions.r_mainTexture;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Set face options for compute shader: color, pattern
        /// </summary>
        internal void SetFaceOptions()
        {
            if (useColor != useColorOld)
            {
                if (SoftEffects.debuglog) Debug.Log("Set color face property (useColor: ) " + useColor);
                useColorOld = useColor;
                IsCombinedDirty = true;
            }

            if (useColor)
            {
                if (fColor != fColorOld)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (color ) ");
                    fColorOld = fColor;
                    IsCombinedDirty = true;
                }
                if (cBlendMode != cBlendModeOld)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (color blendMode ) " + cBlendMode);
                    cBlendModeOld = cBlendMode;
                    IsCombinedDirty = true;
                }
            }

            if (usePattern != usePatternOld)
            {
                if (SoftEffects.debuglog) Debug.Log("Set face property (usePattern: ) " + usePattern);
                usePatternOld = usePattern;
                IsCombinedDirty = true;
            }

            if (usePattern)
            {
                if (pOpacity != pOpacityOld)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (pOpacity ) ");
                    pOpacityOld = pOpacity;
                    IsCombinedDirty = true;
                }

                if (pBlendMode != pBlendModeOld)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (pattern blendMode ) " + pBlendMode);
                    pBlendModeOld = pBlendMode;
                    IsCombinedDirty = true;
                }

                if (pScale != pScaleOld)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (pScale ) ");
                    pScaleOld = pScale;
                    IsCombinedDirty = true;
                }

                if (patternText != patternTextOld)
                {
                    if (SoftEffects.debuglog) Debug.Log("Set face property (pattern ) ");
                    patternTextOld = patternText;
                    IsCombinedDirty = true;
                }
            }
        }

        /// <summary>
        /// Render full styled texture for face include OuterGlow, Stroke, Bevel, InnerShadow, FaceGradient, FaceColor, FacePattern
        /// </summary>
        internal void RenderCombineTexture(GPUWorker gpuWorker, CBuffers buffers)
        {
            if (SoftEffects.debuglog) Debug.Log("Render combined texture.");
            gpuWorker.GPUCombiner(this, buffers);
            IsCombinedDirty = false;
        }

        public void SaveTexturePanel()
        {
            SaveTexturePanel("", "newtexture");

        }

        public void SaveTexturePanel(string folder)
        {
            SaveTexturePanel(folder, "newtexture");
        }

        /// <summary>
        /// https://docs.unity3d.com/ScriptReference/EditorUtility.SaveFilePanel.html
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        public void SaveTexturePanel(string folder, string fileName)
        {
            Texture2D t = new Texture2D(1, 1);
            r_combinedTexture.CreateTextureFromRender_ARGB32(ref t, false, "");

            var path = EditorUtility.SaveFilePanel(
                    "Save texture as PNG",
                    folder,
                   fileName,
                    "png");

            if (path.Length != 0)
            {
                var pngData = t.EncodeToPNG();
                if (pngData != null)
                    File.WriteAllBytes(path, pngData);
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Debug.Log(AssetDatabase.GetAssetPath(t));
            // ClassExtensions.ReimportTexture(path, false);
            // AssetDatabase.SaveAssets();
            //  AssetDatabase.Refresh();
        }

        /// <summary>
        /// Save working texture directly to edit folder
        /// </summary>
        /// <param name="assetPath"></param>
        public void SaveTexture(string assetPath)
        {
            if (assetPath == null || assetPath == "")
            {
                if (SoftEffects.debuglog) Debug.Log("No saving path");
                return;
            }
            Texture2D t = new Texture2D(1, 1);
            r_combinedTexture.CreateTextureFromRender_ARGB32(ref t, true, assetPath);
        }

        internal int ColorBlendmodeNumber
        {
            get
            {
                switch (cBlendMode)
                {
                    case CBlendMode.Normal:
                        return 0;
                    case CBlendMode.Lighten:
                        return 1;
                    case CBlendMode.Screen:
                        return 2;
                    case CBlendMode.Overlay:
                        return 3;
                    case CBlendMode.Darken:
                        return 4;
                    case CBlendMode.Multiplay:
                        return 5;
                    default:
                        return 0;
                }
            }
        }

        internal int PatternBlendmodeNumber
        {
            get
            {
                switch (pBlendMode)
                {
                    case PBlendMode.Normal:
                        return 0;
                    case PBlendMode.Lighten:
                        return 1;
                    case PBlendMode.Screen:
                        return 2;
                    case PBlendMode.Overlay:
                        return 3;
                    case PBlendMode.Darken:
                        return 4;
                    case PBlendMode.Multiplay:
                        return 5;
                    default:
                        return 0;
                }
            }
        }

        internal void CreateUVMapBuffer(int width, int height, CBuffers cb)
        {
            cb.CreateUvMap(width, height, outputUV);
        }

        internal ComputeBuffer GetUVMap(int width, int height, CBuffers cb)
        {
            if (!CheckBuffer(cb.uvMap))
            {
                CreateUVMapBuffer(width, height, cb);
            }

            return cb.uvMap;
        }

        public void ReleaseCombinedRenderTexture()
        {
            if (SoftEffects.debuglog) Debug.Log("Release combined render texture ");
            if (r_combinedTexture != null && r_combinedTexture.IsCreated())
            {
                r_combinedTexture.Release();
            }
            r_combinedTexture = null;
        }
    }

    [Serializable]
    public class Options
    {
        public bool use = false;
        public bool useOld = false;
        public static int contourBufferLength = 512;
        private float[] countBuffer = new float[contourBufferLength];
        public static int gradientWidth = 256;
        public static int gradientsCount = 4; //  0 - stroke, 1 - inner glow, 2 - grad overlay, 3 - outer glow 

        public RenderTexture r_mainTexture;

        public string OptionName
        {
            get { return MethodBase.GetCurrentMethod().DeclaringType.Name; }
        }

        public void CreateGradientTexture(Gradient g, ref Texture2D output)
        {
            if (output != null) UnityEngine.Object.DestroyImmediate(output);
            output = new Texture2D(gradientWidth, 1, TextureFormat.ARGB32, false);
            output.wrapMode = TextureWrapMode.Clamp;
            output.filterMode = FilterMode.Bilinear;
            float k = 1.0f / ((float)gradientWidth - 1.0f);

            for (int i = 0; i < gradientWidth; i++)
            {
                output.SetPixel(i, 0, g.Evaluate((float)i * k));
            }
            output.Apply();
            if (SoftEffects.debuglog) Debug.Log("Created Gradient Texture");
        }

        public void CreateGradientTexture(Gradient g, ref Texture2D output, float scale)
        {
            if (g == null) g = new Gradient();

            if (output != null) UnityEngine.Object.DestroyImmediate(output);
            output = new Texture2D(gradientWidth, 1, TextureFormat.ARGB32, false);
            output.wrapMode = TextureWrapMode.Clamp;
            output.filterMode = FilterMode.Bilinear;
            int d = (int)((float)gradientWidth * scale);
            int i0 = (gradientWidth - d) / 2;
            float k = 1.0f / ((float)d - 1.0f);

            for (int i = 0; i < gradientWidth; i++)
            {
                output.SetPixel(i, 0, g.Evaluate((float)(i - i0) * k));
            }
            output.Apply();
            if (SoftEffects.debuglog) Debug.Log("Created Gradient Texture");
        }

        public void CreateCurveTexture(AnimationCurve g, ref Texture2D output)
        {
            int width = 256;
            if (output != null) UnityEngine.Object.DestroyImmediate(output);
            output = new Texture2D(width, 1, TextureFormat.ARGB32, false);
            output.wrapMode = TextureWrapMode.Clamp;
            output.filterMode = FilterMode.Trilinear;
            if (g == null) g = AnimationCurve.Linear(0, 0, 1, 1);
            for (int i = 0; i < width; i++)
            {
                float c = g.Evaluate((float)i / ((float)width - 1.0f));
                output.SetPixel(i, 0, new Color(c, c, c, 1.0f));
            }
            output.Apply();
        }

        public void CreateCurveBuffer(AnimationCurve g, ref float[] outBuf)
        {
            float wmin1 = 1.0f / (contourBufferLength - 1.0f);
            if (g == null) g = AnimationCurve.Linear(0, 0, 1, 1);

            for (int i = 0; i < contourBufferLength; i++)
            {
                countBuffer[i] = Mathf.Clamp(g.Evaluate((float)i * wmin1), 0f, 1.0f);
            }
            outBuf = countBuffer;
        }

        public void CreateNoiseTexture(ref Texture2D output, int2 size)
        {
            int width = size.X;
            int height = size.Y;

            if (output != null) UnityEngine.Object.DestroyImmediate(output);
            output = new Texture2D(width, height, TextureFormat.ARGB32, false);
            output.wrapMode = TextureWrapMode.Clamp;
            output.filterMode = FilterMode.Bilinear;

            UnityEngine.Random.InitState(42);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    output.SetPixel(x, y, Color.white * UnityEngine.Random.value);
                }
            }
            output.Apply();
        }

        internal bool GradientsEqual(Gradient g1, Gradient g2)
        {
            if (g1 == null || g2 == null)
            {
                return true;
            }
            if (g1.mode != g2.mode)
            {
                return false;
            }
            GradientColorKey[] gck1 = g1.colorKeys;
            GradientAlphaKey[] gak1 = g1.alphaKeys;
            GradientColorKey[] gck2 = g2.colorKeys;
            GradientAlphaKey[] gak2 = g2.alphaKeys;

            if (gck1.Length != gck2.Length || gak1.Length != gak2.Length)
            {
                return false;
            }

            for (int i = 0; i < gck1.Length; i++)
            {
                if (!GradientColorKeyEqual(gck1[i], gck2[i]))
                {
                    return false;
                }
            }

            for (int i = 0; i < gak1.Length; i++)
            {
                if (!GradientAlphaKeyEqual(gak1[i], gak2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        Keyframe[] gck1;
        Keyframe[] gck2;
        internal bool CurvesEqual(AnimationCurve g1, AnimationCurve g2)
        {
            if (g1 == null || g2 == null)
            {
                return true;
            }
            gck1 = g1.keys;
            gck2 = g2.keys;

            if (gck1.Length != gck2.Length)
            {
                return false;
            }

            for (int i = 0; i < gck1.Length; i++)
            {
                if (!KeyframeEqual(gck1[i], gck2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        bool GradientColorKeyEqual(GradientColorKey ck1, GradientColorKey ck2)
        {
            if (ck1.color != ck2.color) return false;
            if (ck1.time != ck2.time) return false;
            return true;
        }

        bool GradientAlphaKeyEqual(GradientAlphaKey ak1, GradientAlphaKey ak2)
        {
            if (ak1.alpha != ak2.alpha) return false;
            if (ak1.time != ak2.time) return false;
            return true;
        }

        bool KeyframeEqual(Keyframe kf1, Keyframe kf2)
        {
            if (kf1.time != kf2.time) return false;
            if (kf1.value != kf2.value) return false;
            if (kf1.inTangent != kf2.inTangent) return false;
            if (kf1.outTangent != kf2.outTangent) return false;
            if (kf1.inWeight != kf2.inWeight) return false;
            if (kf1.outWeight != kf2.outWeight) return false;
            if (kf1.weightedMode != kf2.weightedMode) return false;
            return true;
        }

        public void ReleaseRenderTexture()
        {
            if (SoftEffects.debuglog) Debug.Log("Release render texture : " + OptionName);
            if (r_mainTexture != null && r_mainTexture.IsCreated())
            {
                r_mainTexture.Release();
            }
            r_mainTexture = null;
        }

        public bool CheckBuffer(ComputeBuffer c)
        {
            try
            {
                if (c == null || !c.IsValid()) return false;
                int count = c.count;
            }
            catch (NullReferenceException)
            {

                return false;
            }
            return true;
        }
    }

    [Serializable]
    public class OptionsMain : Options
    {
        public Texture2D mainTexture;

        /// <summary>
        /// Create Texture2D A8 from render texture R8 and save it to file. Path like - "Assets/SoftEffects/Fonts/gpu.png"
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public void CreateTextureFromRender_A8(bool save, string path)
        {
            r_mainTexture.CreateTextureFromRender_A8(ref mainTexture, save, path);
        }

        /// <summary>
        /// Create mainTexture Texture2D RFloat from render texture RFloat
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public void CreateTextureFromRender_Speed_R32()
        {
            r_mainTexture.CreateTextureFromRender_Speed_R32(ref mainTexture);
        }

        /// <summary>
        /// Create mainTexture Texture2D R16 from render texture R16
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public void CreateTextureFromRender_Speed_R16()
        {
            r_mainTexture.CreateTextureFromRender_Speed_R16(ref mainTexture);
        }

        /// <summary>
        /// Create Texture2D  from render texture and save it to file."
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public void CreateTextureFromRender_Speed_ARGB32()
        {
            r_mainTexture.CreateTextureFromRender_Speed_ARGB32(ref mainTexture);
        }

        /// <summary>
        /// Create Texture2D ARGB32 from render texture ARGB32 and save it to file. Path like - "Assets/SoftEffects/Fonts/gpu.png"
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText2D"></param>
        /// <param name="save"></param>
        /// <param name="path"></param>
        public void CreateTextureFromRender_ARGB32(bool save, string path)
        {
            r_mainTexture.CreateTextureFromRender_ARGB32(ref mainTexture, save, path);
        }

    }

#endif
}