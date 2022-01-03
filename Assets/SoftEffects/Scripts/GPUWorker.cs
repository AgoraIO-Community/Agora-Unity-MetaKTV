using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEditor.SceneManagement;
#endif

namespace Mkey
{
    [Serializable]
    public class GPUWorker
    {
#if UNITY_EDITOR
        public float[] gWeights;
        public ComputeShader EShader
        {
            get
            {
                if (eShader && eShader != null) return eShader;
                else { eShader = GetComputeShader(); return eShader; }
            }
        }

        public ComputeShader eShader;
        private int kernelHandle;

        public GPUWorker(ComputeShader eShader)
        {
            this.eShader = eShader;
        }

        /// <summary>
        /// Find compute shader and set  eShader var
        /// </summary>
        internal ComputeShader GetComputeShader()
        {
            Debug.Log("find shader");
            ComputeShader cS = null;

            var guids = AssetDatabase.FindAssets("SoftEffectsCompute");
            if (guids != null && guids.Length > 0)
            {
                cS = (ComputeShader)AssetDatabase.LoadMainAssetAtPath(AssetDatabase.GUIDToAssetPath(guids[0]));
                if (cS)
                {
                    if (SoftEffects.debuglog) Debug.Log("Compute shader found: " + AssetDatabase.GUIDToAssetPath(guids[0]));
                }
                else Debug.LogError("Compute shader not found. Return.....");
            }
            return cS;
        }

        #region GAUSSIAN BLUR
        /// <summary>
        /// Render gaussian blured RGBA texture
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        /// <param name="addColor"></param>
        /// <param name="blur">Sigma (0 - 1)</param>
        /// <param name="radius">Radius in pixels</param>
        public void GPURenderGaussianBlurTexture(Texture inputText, ref RenderTexture outText, float blur, int radius)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;

            CreateGaussBlurKernel(blur, radius);
            ComputeBuffer weightsBuffer = new ComputeBuffer(radius * 2 + 1, 4);

            weightsBuffer.SetData(gWeights);

            RenderTexture bluredTextVTemp = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            bluredTextVTemp.enableRandomWrite = true;
            bluredTextVTemp.Create();

            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            outText.enableRandomWrite = true;
            outText.Create();

            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputV", bluredTextVTemp);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.Dispatch(kernelHandle, textureWidth, textureHeight / 32 + 1, 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur");
            EShader.SetTexture(kernelHandle, "gInputH", bluredTextVTemp);
            EShader.SetTexture(kernelHandle, "gOutputD", outText);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.Dispatch(kernelHandle, textureWidth / 32 + 1, textureHeight, 1);
            bluredTextVTemp.Release();

            // release buffers
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render gaussian blured R8 texture
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        /// <param name="addColor"></param>
        /// <param name="blur">Sigma (0 - 1)</param>
        /// <param name="radius">Radius in pixels</param>
        public void GPURenderGaussianBlurTexture_R8(Texture inputText, ref RenderTexture outText, float blur, int radius)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;

            CreateGaussBlurKernel(blur, radius);
            ComputeBuffer weightsBuffer = new ComputeBuffer(radius * 2 + 1, 4);

            weightsBuffer.SetData(gWeights);

            RenderTexture bluredTextVTemp = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.R8);
            bluredTextVTemp.enableRandomWrite = true;
            bluredTextVTemp.Create();

            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.R8);
            outText.enableRandomWrite = true;
            outText.Create();

            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur_R8");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputVf", bluredTextVTemp);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.Dispatch(kernelHandle, textureWidth, Mathf.CeilToInt(textureHeight / 32.0f), 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur_R8");
            EShader.SetTexture(kernelHandle, "gInputH", bluredTextVTemp);
            EShader.SetTexture(kernelHandle, "gOutputDf", outText);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 32.0f), textureHeight, 1);
            bluredTextVTemp.Release();

            // release buffers
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render gaussian blured R8 texture
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        /// <param name="addColor"></param>
        /// <param name="blur">Sigma (0 - 1)</param>
        /// <param name="radius">Radius in pixels</param>
        public void GPURenderGaussianBlurTexture250_R8(Texture inputText, ref RenderTexture outText, int radius)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;

            CreateGaussBlurKernel(radius, 250);
            //CreateBlurKernel_250(radius);
            ComputeBuffer weightsBuffer = new ComputeBuffer(501, 4);

            weightsBuffer.SetData(gWeights);

            RenderTexture bluredTextVTemp = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.R8);
            bluredTextVTemp.enableRandomWrite = true;
            bluredTextVTemp.Create();

            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.R8);
            outText.enableRandomWrite = true;
            outText.Create();

            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur250_R8");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputVf", bluredTextVTemp); //bluredTextVTemp
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.Dispatch(kernelHandle, textureWidth, Mathf.CeilToInt(textureHeight / 256.0f), 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur250_R8");
            EShader.SetTexture(kernelHandle, "gInputH", bluredTextVTemp);
            EShader.SetTexture(kernelHandle, "gOutputDf", outText);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 256.0f), textureHeight, 1);
            bluredTextVTemp.Release();

            // release buffers
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render Gaussian blured Buffer. 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderGaussianBuffer_30(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            CreateGaussBlurKernel(radius, 30);
            ComputeBuffer weightsBuffer = new ComputeBuffer(61, 4);
            weightsBuffer.SetData(gWeights);

            ComputeBuffer tempBuffer = new ComputeBuffer(TextSize.WxH, 4);
            outBuffer = new ComputeBuffer(TextSize.WxH, 4);


            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur30_BUF");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", inBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.Dispatch(kernelHandle, TextSize.Width, TextSize.H32d, 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur30_BUF");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", tempBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.Height, 1);

            tempBuffer.Release(); tempBuffer = null;
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render Gaussian blured Buffer. 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderGaussianBuffer_16(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            CreateGaussBlurKernel(radius, 16);
            ComputeBuffer weightsBuffer = new ComputeBuffer(33, 4);
            weightsBuffer.SetData(gWeights);

            ComputeBuffer tempBuffer = new ComputeBuffer(TextSize.WxH, 4);
            outBuffer = new ComputeBuffer(TextSize.WxH, 4);


            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur16_BUF");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", inBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.Dispatch(kernelHandle, TextSize.Width, TextSize.H32d, 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur16_BUF");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", tempBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.Height, 1);

            tempBuffer.Release(); tempBuffer = null;
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render Gaussian blured Buffer. 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderGaussianBuffer_8(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            CreateGaussBlurKernel(radius, 8);
            ComputeBuffer weightsBuffer = new ComputeBuffer(17, 4);
            weightsBuffer.SetData(gWeights);

            ComputeBuffer tempBuffer = new ComputeBuffer(TextSize.WxH, 4);
            outBuffer = new ComputeBuffer(TextSize.WxH, 4);


            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur8_BUF");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", inBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.Dispatch(kernelHandle, TextSize.Width, TextSize.H32d, 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur8_BUF");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", tempBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.Height, 1);

            tempBuffer.Release(); tempBuffer = null;
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render Box blured Buffer. 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderGaussianBuffer_250(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;
            int maxSize = 250;

            CreateGaussBlurKernel(radius, maxSize);
            ComputeBuffer weightsBuffer = new ComputeBuffer(maxSize * 2 + 1, 4);
            weightsBuffer.SetData(gWeights);


            ComputeBuffer tempBuffer = new ComputeBuffer(textureHeight * textureWidth, 4);
            outBuffer = new ComputeBuffer(textureHeight * textureWidth, 4);


            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur250_BUF");
            EShader.SetBuffer(kernelHandle, "fBInputA", inBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.Dispatch(kernelHandle, textureWidth, Mathf.CeilToInt(textureHeight / 32.0f), 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur250_BUF");
            EShader.SetBuffer(kernelHandle, "fBInputA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 32.0f), textureHeight, 1);

            tempBuffer.Release(); tempBuffer = null;
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        /// <summary>
        /// Render Box blured Buffer. 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderRadBlurBuffer_250(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;
            int maxSize = 250;

            CreateRadBlurKernel((int)radius, maxSize);
            ComputeBuffer weightsBuffer = new ComputeBuffer(maxSize * 2 + 1, 4);
            weightsBuffer.SetData(gWeights);


            ComputeBuffer tempBuffer = new ComputeBuffer(textureHeight * textureWidth, 4);
            outBuffer = new ComputeBuffer(textureHeight * textureWidth, 4);


            //------- vertical blur----------
            kernelHandle = EShader.FindKernel("CSVerBlur250_BUF");
            EShader.SetBuffer(kernelHandle, "fBInputA", inBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.Dispatch(kernelHandle, textureWidth, Mathf.CeilToInt(textureHeight / 32.0f), 1);

            //------- horizontal blur----------
            kernelHandle = EShader.FindKernel("CSHorBlur250_BUF");
            EShader.SetBuffer(kernelHandle, "fBInputA", tempBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.SetBuffer(kernelHandle, "wBuffer", weightsBuffer);
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 32.0f), textureHeight, 1);

            tempBuffer.Release(); tempBuffer = null;
            weightsBuffer.Release();
            weightsBuffer = null;
        }

        int KernSize;
        float sqr2si;
        float kernel_sum;
        int cpk;
        public void CreateGaussBlurKernel(float sigma, int KernRadius)
        {
            KernSize = KernRadius * 2 + 1;
            if (sigma < 0.001f) sigma = 0.001f;
            sqr2si = -1.0f / (2.0f * sigma * sigma);
            kernel_sum = 0.0f;

            if (gWeights == null || gWeights.Length != KernSize) gWeights = new float[KernSize];

            for (int c = -KernRadius; c <= 0; c++)
            {
                cpk = c + KernRadius;
                gWeights[cpk] = Mathf.Exp(((c * c)) * sqr2si);
                gWeights[KernRadius - c] = gWeights[cpk];
                kernel_sum += gWeights[cpk];
            }

            kernel_sum = 2.0f * (kernel_sum - gWeights[KernRadius]) + gWeights[KernRadius];
            if (SoftEffects.debuglog) Debug.Log("kernel_sum: " + kernel_sum);
            kernel_sum = 1.0f / kernel_sum;

            for (int c = 0; c < gWeights.Length; c++)
            {
                gWeights[c] *= kernel_sum;
            }
            //  Mkey.PrintData.BufTostring(gWeights, KernSize, 1);
        }

        public void CreateRadBlurKernel(int KernRadius, int maxRadius)
        {
            float kernel_sum = 0.0f;
            if (gWeights == null || gWeights.Length != maxRadius) gWeights = new float[maxRadius * 2 + 1];
            int center = maxRadius;

            for (int c = 0; c < maxRadius * 2 + 1; c++)
            {
                gWeights[c] = 0;
            }

            for (int c = -KernRadius; c <= 0; c++)
            {
                gWeights[c + center] = Mathf.Sqrt(KernRadius * KernRadius - c * c);
                gWeights[-c + center] = gWeights[c + center];
                kernel_sum += gWeights[c + center];
            }
            kernel_sum = 2.0f * (kernel_sum - gWeights[center]) + gWeights[center];
            if (SoftEffects.debuglog) Debug.Log("kernel_sum: " + kernel_sum);
            kernel_sum = 1.0f / kernel_sum;

            for (int c = 0; c < gWeights.Length; c++)
            {
                gWeights[c] *= kernel_sum;
            }
        }
        #endregion GAUSSIAN BLUR


        #region BOX BLUR

        float bbHalfRadius;
        float bbHalfSpread;
        /// <summary>
        /// Render Box blured  ARGB texture. 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderBoxBlurTexture(Texture inputText, ref RenderTexture outText, float radius, float spread)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.ARGB32, TextSize.Width, TextSize.Height);
            RenderTexture tempText = CreateRenderTexture(RenderTextureFormat.ARGB32, TextSize.Height, TextSize.Width, FilterMode.Point);
            bbHalfRadius = radius * 0.5f;
            string kName = "CSBoxBlurPass";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputV", tempText);
            EShader.SetFloat("fSize", bbHalfRadius);
            EShader.SetFloat("spread", spread);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            EShader.SetInt("width", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", tempText);
            EShader.SetTexture(kernelHandle, "gOutputV", outText);
            EShader.SetFloat("fSize", bbHalfRadius);
            EShader.SetFloat("spread", spread);
            EShader.Dispatch(kernelHandle, TextSize.H32d, TextSize.W32d, 1);

            if (tempText != null && tempText.IsCreated())
            {
                tempText.Release();
                tempText = null;
            }
        }

        /// <summary>
        /// Render Box blured texture. From R8 to R8, vert pass(0.5*rad), hor pass (0.5*rad) ???
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderBoxBlurTexture_R8(Texture inputText, ref RenderTexture outText, float radius, float spread)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);
            RenderTexture tempText = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Height, TextSize.Width, FilterMode.Point);
            bbHalfRadius = radius * 0.5f;

            string kName = "CSBoxBlurPass_R8"; // rotate texture 90 degr
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputVf", tempText);
            EShader.SetFloat("fSize", bbHalfRadius);
            EShader.SetFloat("spread", spread);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            EShader.SetInt("width", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", tempText);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetFloat("fSize", bbHalfRadius);
            EShader.SetFloat("spread", spread);
            EShader.Dispatch(kernelHandle, TextSize.H32d, TextSize.W32d, 1);

            if (tempText != null && tempText.IsCreated())
            {
                tempText.Release();
                tempText = null;
            }
        }

        /// <summary>
        /// Render Box blured texture. From A8 to R8, vert pass(0.5*rad), hor pass (0.5*rad) ???
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderBoxBlurTexture_AR8(Texture inputText, ref RenderTexture outText, float radius, float spread)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);
            RenderTexture tempText = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Height, TextSize.Width, FilterMode.Point);
            bbHalfRadius = radius * 0.5f;

            string kName = "CSBoxBlurPass_AR8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputVf", tempText);
            EShader.SetFloat("fSize", bbHalfRadius);
            EShader.SetFloat("spread", spread);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            kName = "CSBoxBlurPass_R8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", tempText);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetFloat("fSize", bbHalfRadius);
            EShader.SetFloat("spread", spread);
            EShader.Dispatch(kernelHandle, TextSize.H32d, TextSize.W32d, 1);

            if (tempText != null && tempText.IsCreated())
            {
                tempText.Release();
                tempText = null;
            }
        }

        /// <summary>
        /// Render Box blured Buffer. inBuffer to outBuffer; Two pass: vert pass(radius), hor pass(radius) 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderBoxBlurBuffer(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius, float spread)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            ComputeBuffer tempBuffer = new ComputeBuffer(TextSize.WxH, 4);
            outBuffer = new ComputeBuffer(TextSize.WxH, 4);
            float s = spread / 100.0f;
            float sk = (s < 0.75f) ? Mathf.Lerp(1.0f, 2.0f, s / 0.75f) : Mathf.Lerp(2.0f, 8.0f, (s - 0.75f) / 0.25f);
            int r = (int)radius;
            float aDiv = (2.0f * r + 1.0f);
            aDiv = sk / aDiv;
            float frac = radius - r;

            kernelHandle = EShader.FindKernel("CSBoxBlurPass_BUF");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", inBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", tempBuffer);
            EShader.SetFloat("fSize", aDiv);
            EShader.SetFloat("range", frac);
            EShader.SetInt("ext", r);
            EShader.SetFloat("spread", sk);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            EShader.SetInt("width", TextSize.Height); // rotate texture 90 degr
            EShader.SetInt("height", TextSize.Width);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", tempBuffer);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.SetFloat("fSize", aDiv);
            EShader.SetFloat("range", frac);
            EShader.SetInt("ext", r);
            EShader.SetFloat("spread", sk);
            EShader.Dispatch(kernelHandle, TextSize.H32d, TextSize.W32d, 1);
            tempBuffer.Release(); tempBuffer = null;
        }

        /// <summary>
        /// Render Box blured Buffer. From inBuffer to outBuffer.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderDoubleBoxBlurBuffer(Texture inputText, ComputeBuffer inBuffer, out ComputeBuffer outBuffer, float radius, float spread)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ComputeBuffer tempBuffer;
            bbHalfRadius = radius * 0.5f;
            bbHalfSpread = spread * 0.5f;

            GPURenderBoxBlurBuffer(inputText, inBuffer, out tempBuffer, bbHalfRadius, bbHalfSpread);
            GPURenderBoxBlurBuffer(inputText, tempBuffer, out outBuffer, bbHalfRadius, bbHalfSpread);
            tempBuffer.Release(); tempBuffer = null;
        }

        /// <summary>
        /// Render box blur with 2 half passes from a channel to R8  
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderDoubleBoxBlur_AR8(Texture inputText, ref RenderTexture outText, float radius, float spread, float preserve)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);
            RenderTexture tempTexture = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);
            bbHalfRadius = radius * 0.5f;

            GPURenderBoxBlurTexture_AR8(inputText, ref tempTexture, bbHalfRadius, spread);
            GPURenderBoxBlurTexture_R8(tempTexture, ref outText, bbHalfRadius, spread);

            tempTexture.Release();
        }
        #endregion BOX BLUR


        #region INVERSE
        /// <summary>
        /// Render inverse texture from input render texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderInverse(Texture inputText, ref RenderTexture outText)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;

            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.ARGB32);
            outText.enableRandomWrite = true;
            outText.Create();

            // inverse
            kernelHandle = EShader.FindKernel("CSInverse");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputV", outText);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 32.0f), Mathf.CeilToInt(textureHeight / 32.0f), 1);
        }

        /// <summary>
        /// Render inverse texture from input render texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPURenderInverse_R8(Texture inputText, ref RenderTexture outText)
        {
            int textureWidth = inputText.width;
            int textureHeight = inputText.height;

            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(textureWidth, textureHeight, 0, RenderTextureFormat.R8);
            outText.enableRandomWrite = true;
            outText.Create();

            // inverse
            kernelHandle = EShader.FindKernel("CSInverse_R8FromAlpha");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 32.0f), Mathf.CeilToInt(textureHeight / 32.0f), 1);
        }
        #endregion INVERSE


        #region FILL
        /// <summary>
        /// Fill output R8 render texture with color.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        /// <param name="color"></param>
        public void GPUFill_R8(RenderTexture inputText, ref RenderTexture outText, float color)
        {
            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(inputText.width, inputText.height, 0, RenderTextureFormat.R8);
            outText.enableRandomWrite = true;
            outText.Create();

            // fill output texture
            kernelHandle = EShader.FindKernel("CSFill_1C");
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetVector("eColor", new Vector4(color, color, color, color));
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(outText.width / 32.0f), Mathf.CeilToInt(outText.height / 32.0f), 1);
        }

        /// <summary>
        /// Fill output R8 render texture with color.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        /// <param name="color"></param>
        public void GPUFill_R8(int2 size, ref RenderTexture outText, float color)
        {
            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(size.X, size.Y, 0, RenderTextureFormat.R8);
            outText.enableRandomWrite = true;
            outText.Create();

            // fill output texture
            kernelHandle = EShader.FindKernel("CSFill_1C");
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetVector("eColor", new Vector4(color, color, color, color));
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(outText.width / 32.0f), Mathf.CeilToInt(outText.height / 32.0f), 1);
        }

        /// <summary>
        /// Fill output RGBA render texture with color.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        /// <param name="color"></param>
        public void GPUFill(int2 size, ref RenderTexture outText, Color color)
        {
            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(size.X, size.Y, 0, RenderTextureFormat.ARGB32);
            outText.enableRandomWrite = true;
            outText.Create();

            // fill output texture
            kernelHandle = EShader.FindKernel("CSFill");
            EShader.SetTexture(kernelHandle, "gOutputV", outText);
            EShader.SetVector("eColor", new Vector4(color.r, color.g, color.b, color.a));
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(outText.width / 32.0f), Mathf.CeilToInt(outText.height / 32.0f), 1);
        }
        #endregion FILL


        #region COPY
        /// <summary>
        /// Copy data from input texture RGBA with input UVs to output render texture RGBA with Output UVs;
        /// </summary>
        /// <param name="inputUV"></param>
        /// <param name="outputUV"></param>
        /// <param name="outTextSize"></param>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPUCopyTextureUV(int2[] inputUV, int2[] outputUV, int2 outTextSize, Texture inputText, ref RenderTexture outText)
        {
            if (SoftEffects.debuglog) Debug.Log("-----------Copy UV from Texture on GPU--------------------");

            //   Mkey.PrintData.Texture2DToString((Texture2D)inputText, 0);
            //---------- create compute buffers-------------------------------------
            ComputeBuffer inputVBuffer = new ComputeBuffer(inputUV.Length, 8);
            inputVBuffer.SetData(inputUV);
            ComputeBuffer inputVBuffer1 = new ComputeBuffer(inputUV.Length, 8);
            inputVBuffer1.SetData(outputUV);

            if (outText != null && outText.IsCreated()) outText.Release();
            outText = new RenderTexture(outTextSize.X, outTextSize.Y, 0, RenderTextureFormat.ARGB32);
            outText.enableRandomWrite = true;
            outText.Create();

            // fill output texture
            kernelHandle = EShader.FindKernel("CSFill");
            EShader.SetTexture(kernelHandle, "gOutputV", outText);
            EShader.SetVector("eColor", new Vector4(1, 1, 1, 0));
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(outText.width / 32.0f), Mathf.CeilToInt(outText.height / 32.0f), 1);

            // copy symbols to output texture
            kernelHandle = EShader.FindKernel("CSFullCopy");
            EShader.SetTexture(kernelHandle, "gOutputV", outText);
            EShader.SetInt("width", outTextSize.X);
            EShader.SetInt("height", outTextSize.Y);
            EShader.SetTexture(kernelHandle, "gInputV", inputText); // source text
            EShader.SetBuffer(kernelHandle, "gInputVBufferAInt", inputVBuffer); // set input vertexUV buffer
            EShader.SetBuffer(kernelHandle, "gInputVBufferBInt", inputVBuffer1); // set input vertexUV buffer

            EShader.Dispatch(kernelHandle, inputUV.Length / 3, 1, 1); // 3 uv for symbol : bottom_left, bottom_right, top_right

            // release buffers
            inputVBuffer.Release();
            inputVBuffer = null;
            inputVBuffer1.Release();
            inputVBuffer1 = null;
        }

        /// <summary>
        /// Simple render R8 texture add first Atexture r-channel to Btexture r-channel 
        /// </summary>
        /// <param name="inTex"></param>
        /// <param name="outTex"></param>
        /// <param name="r_mainTexture"></param>
        internal void AddTextures_R8(RenderTexture aTex, RenderTexture bTex, ref RenderTexture outText)
        {
            if (SoftEffects.debuglog) Debug.Log("-----------Add Texture on GPU--------------------");
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, aTex.width, aTex.height);
            TextSize.SetSize(outText.width, outText.height);
            // copy input to output texture
            kernelHandle = EShader.FindKernel("CSAdd_R8");
            EShader.SetTexture(kernelHandle, "gInputV", aTex);
            EShader.SetTexture(kernelHandle, "gInputH", bTex);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        /// <summary>
        /// Simple copy R8 texture  to R8 texture 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPUCopyR8R8(Texture inputText, ref RenderTexture outText)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            // copy input to output texture
            kernelHandle = EShader.FindKernel("CSCopy");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputV", outText);
            EShader.SetBool("boolA", true); // input texture 
            EShader.SetBool("boolB", true); // output to tetxture
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);

            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        /// <summary>
        /// Simple copy R8 texture  to R8 texture 
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPUCopyR8BUF(Texture inputText, ref ComputeBuffer outputBuffer, bool useRange, float range)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outputBuffer != null)
            {
                outputBuffer.Release();
                outputBuffer = null;
            }

            outputBuffer = new ComputeBuffer(TextSize.WxH, 4);

            // copy input to output texture
            kernelHandle = EShader.FindKernel("CSCopy");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", outputBuffer);
            EShader.SetBool("boolA", true); // input texture 
            EShader.SetBool("boolB", false); // output to texture
            EShader.SetBool("boolC", useRange); // output to texture
            EShader.SetFloat("range", range); // output to texture
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);

            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        /// <summary>
        /// Simple copy texture a-channel to render texture R8
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outText"></param>
        public void GPUCopyTextureToR8(Texture inputText, ref RenderTexture outText)
        {
            if (SoftEffects.debuglog) Debug.Log("-----------Copy Texture on GPU--------------------");
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            // copy input to output texture
            kernelHandle = EShader.FindKernel("CSCopyAR8");
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        ComputeBuffer csContourBuffer;
        ComputeBuffer csContourBuffer_1;
        ComputeBuffer isContourBuffer;
        ComputeBuffer igContourBuffer;
        ComputeBuffer ogContourBuffer;
        bool setGradient = false;
        bool setFullDTMap = false;
        public void GPUCombiner(FaceOptions fOptions, CBuffers buffers)
        {
            RenderTexture zt = new RenderTexture(2, 2, 0, RenderTextureFormat.R8);
            ComputeBuffer zb = new ComputeBuffer(1, 4);


            TextSize.SetSize(fOptions.mainTexture.width, fOptions.mainTexture.height);

            if (fOptions.r_combinedTexture != null && fOptions.r_combinedTexture.IsCreated()) fOptions.r_combinedTexture.Release();

            fOptions.r_combinedTexture = CreateRenderTexture(RenderTextureFormat.ARGB32, fOptions.mainTexture.width, fOptions.mainTexture.height, FilterMode.Bilinear);

            // set face 
            kernelHandle = EShader.FindKernel("CSCombiner");
            EShader.SetTexture(kernelHandle, "gOutputV", fOptions.r_combinedTexture);
            EShader.SetTexture(kernelHandle, "gInputV", fOptions.mainTexture); // face text
            EShader.SetBuffer(kernelHandle, "gInputVBufferA", fOptions.GetUVMap(fOptions.mainTexture.width, fOptions.mainTexture.height, buffers));

            setGradient = false;
            setFullDTMap = false;
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", zb); // dt
            EShader.SetBuffer(kernelHandle, "gInputBufferF4A", zb);   // gradients

            // set close shadow options and textures
            if (fOptions.csOptions.use)
            {
                EShader.SetBool("csUse", true);
                csContourBuffer = new ComputeBuffer(Options.contourBufferLength, 4);
                csContourBuffer.SetData(fOptions.csOptions.ContourBuffer);
                EShader.SetVector("csColor", fOptions.csOptions.color);
                EShader.SetBuffer(kernelHandle, "fBInputE", csContourBuffer);
                EShader.SetTexture(kernelHandle, "gInputX", fOptions.csOptions.r_mainTexture); // close shadow R8
                EShader.SetVector("csOptions", new Vector4(1.0f,
                                        fOptions.csOptions.DCos,
                                        fOptions.csOptions.DSin,
                                        0));
            }
            else
            {
                EShader.SetBool("csUse", false);
                EShader.SetBuffer(kernelHandle, "fBInputE", zb);
                EShader.SetTexture(kernelHandle, "gInputX", zt);

            }

            // set close shadow (1) options and textures
            if (fOptions.csOptions_1.use)
            {
                EShader.SetBool("csUse_1", true);
                csContourBuffer_1 = new ComputeBuffer(Options.contourBufferLength, 4);
                csContourBuffer_1.SetData(fOptions.csOptions_1.ContourBuffer);
                EShader.SetVector("csColor_1", fOptions.csOptions_1.color);
                EShader.SetBuffer(kernelHandle, "fBInputE1", csContourBuffer_1);
                EShader.SetTexture(kernelHandle, "gInputX1", fOptions.csOptions_1.r_mainTexture); // close shadow R8
                EShader.SetVector("csOptions_1", new Vector4(0,
                                        fOptions.csOptions_1.DCos,
                                        fOptions.csOptions_1.DSin,
                                        0));
            }
            else
            {
                EShader.SetBool("csUse_1", false);
                EShader.SetBuffer(kernelHandle, "fBInputE1", zb);
                EShader.SetTexture(kernelHandle, "gInputX1", zt);

            }

            // set outer glow options and textures
            if (fOptions.ogOptions.use && fOptions.ogOptions.size > 0)
            {
                EShader.SetBool("ogUse", true);
                EShader.SetBool("ogUseG", fOptions.ogOptions.fillType == OuterGlowOptions.FillType.Gradient);

                ogContourBuffer = new ComputeBuffer(Options.contourBufferLength, 4);
                ogContourBuffer.SetData(fOptions.ogOptions.ContourBuffer);
                if (fOptions.ogOptions.fillType == OuterGlowOptions.FillType.Gradient)
                {
                    if (!setGradient)
                    {
                        buffers.CreateGradientBuffer();
                        EShader.SetBuffer(kernelHandle, "gInputBufferF4A", buffers.gradients);
                        setGradient = true;
                    }
                }
                EShader.SetVector("ogColor", fOptions.ogOptions.color);
                EShader.SetBuffer(kernelHandle, "fBInputD", ogContourBuffer);
                EShader.SetTexture(kernelHandle, "gInputH", fOptions.ogOptions.r_mainTexture); // outer glow R8
                EShader.SetFloat("ogOptions", 100.0f / fOptions.ogOptions.range);
                if (!setFullDTMap)
                {
                    EShader.SetBuffer(kernelHandle, "gInputBufferfloat", buffers.CBEDTAASigned);
                    setFullDTMap = true;
                }
            }
            else
            {
                EShader.SetBool("ogUse", false);
                EShader.SetBuffer(kernelHandle, "fBInputD", zb);
                EShader.SetTexture(kernelHandle, "gInputH", zt);
            }

            // set face color options
            if (fOptions.useColor)
            {
                EShader.SetBool("fcUse", true);
                EShader.SetBool("fcUseN", fOptions.cBlendMode == FaceOptions.CBlendMode.Normal);
                EShader.SetBool("fcUseL", fOptions.cBlendMode == FaceOptions.CBlendMode.Lighten);
                EShader.SetBool("fcUseS", fOptions.cBlendMode == FaceOptions.CBlendMode.Screen);
                EShader.SetBool("fcUseO", fOptions.cBlendMode == FaceOptions.CBlendMode.Overlay);
                EShader.SetBool("fcUseD", fOptions.cBlendMode == FaceOptions.CBlendMode.Darken);

                EShader.SetVector("fcColor", fOptions.fColor);
            }
            else
            {
                EShader.SetBool("fcUse", false);
            }

            // set face pattern options
            if (fOptions.usePattern && fOptions.patternText)
            {
                Vector2 tSizeM1 = new Vector2(fOptions.patternText.width, fOptions.patternText.height) - Vector2.one;
                Vector2 tSizeM1MS = tSizeM1 * (float)fOptions.pScale / 100.0f;
                EShader.SetBool("fpUse", true);
                EShader.SetBool("fpUseN", fOptions.pBlendMode == FaceOptions.PBlendMode.Normal);
                EShader.SetBool("fpUseL", fOptions.pBlendMode == FaceOptions.PBlendMode.Lighten);
                EShader.SetBool("fpUseS", fOptions.pBlendMode == FaceOptions.PBlendMode.Screen);
                EShader.SetBool("fpUseO", fOptions.pBlendMode == FaceOptions.PBlendMode.Overlay);
                EShader.SetBool("fpUseD", fOptions.pBlendMode == FaceOptions.PBlendMode.Darken);
                EShader.SetTexture(kernelHandle, "gInputK", fOptions.patternText);
                EShader.SetFloat("fpOptionsOp",(float)fOptions.pOpacity / 100.0f);
                EShader.SetVector("fpOptions", new Vector4(
                                        tSizeM1.x,
                                        tSizeM1MS.x,
                                        tSizeM1MS.y, 
                                        tSizeM1.y));
            }
            else
            {
                EShader.SetBool("fpUse", false);
                EShader.SetTexture(kernelHandle, "gInputK", zt);
            }

            // set face gradient options 
            if (fOptions.fgOptions.use)
            {
                EShader.SetBool("fgUse", true);
                EShader.SetBool("fgUseN", fOptions.fgOptions.gBlendMode == FaceGradientOptions.GBlendMode.Normal);
                EShader.SetBool("fgUseL", fOptions.fgOptions.gBlendMode == FaceGradientOptions.GBlendMode.Lighten);
                EShader.SetBool("fgUseS", fOptions.fgOptions.gBlendMode == FaceGradientOptions.GBlendMode.Screen);
                EShader.SetBool("fgUseO", fOptions.fgOptions.gBlendMode == FaceGradientOptions.GBlendMode.Overlay);
                EShader.SetBool("fgUseD", fOptions.fgOptions.gBlendMode == FaceGradientOptions.GBlendMode.Darken);

                EShader.SetBool("fgUseTBu", fOptions.fgOptions.gradType == FaceGradientOptions.GradientType.ShapeBurst);
                EShader.SetBool("fgUseTLi", fOptions.fgOptions.gradType == FaceGradientOptions.GradientType.Linear);
                EShader.SetBool("fgUseTRa", fOptions.fgOptions.gradType == FaceGradientOptions.GradientType.Radial);
                EShader.SetBool("fgUseTAn", fOptions.fgOptions.gradType == FaceGradientOptions.GradientType.Angle);
                EShader.SetBool("fgUseTRe", fOptions.fgOptions.gradType == FaceGradientOptions.GradientType.Reflected);

                if (!setGradient)
                {
                    buffers.CreateGradientBuffer();
                    EShader.SetBuffer(kernelHandle, "gInputBufferF4A", buffers.gradients);
                    setGradient = true;
                }
                EShader.SetTexture(kernelHandle, "gInputG", fOptions.fgOptions.r_mainTexture);
                EShader.SetVector("fgOptions", new Vector4(0,
                                        Mathf.Deg2Rad * fOptions.fgOptions.angle,
                                        0,
                                        0));
            }
            else
            {
                EShader.SetBool("fgUse", false);
                // EShader.SetVector("fgOptions", Vector4.zero);
                EShader.SetTexture(kernelHandle, "gInputG", zt);
            }

            // set stroke options and textures
            if (fOptions.strOptions.use && fOptions.strOptions.size > 0)
            {
                EShader.SetBool("sUse", true);
                EShader.SetBool("sUseIn", fOptions.strOptions.PosNumber == 0);
                EShader.SetBool("sUseG", fOptions.strOptions.fillType == StrokeOptions.FillType.Gradient);
                EShader.SetBool("sUseTBu", fOptions.strOptions.gradType == StrokeOptions.GradientType.ShapeBurst);
                EShader.SetBool("sUseTLi", fOptions.strOptions.gradType == StrokeOptions.GradientType.Linear);
                EShader.SetBool("sUseTRa", fOptions.strOptions.gradType == StrokeOptions.GradientType.Radial);
                EShader.SetBool("sUseTAn", fOptions.strOptions.gradType == StrokeOptions.GradientType.Angle);
                EShader.SetBool("sUseTRe", fOptions.strOptions.gradType == StrokeOptions.GradientType.Reflected);

                if (fOptions.strOptions.fillType == StrokeOptions.FillType.Gradient)
                {
                    if (!setGradient)
                    {
                        buffers.CreateGradientBuffer();
                        EShader.SetBuffer(kernelHandle, "gInputBufferF4A", buffers.gradients);
                        setGradient = true;
                    }
                }

                EShader.SetVector("strColor", fOptions.strOptions.color);

                EShader.SetTexture(kernelHandle, "gInputA", fOptions.strOptions.r_mainTexture); // stroke R8

                EShader.SetVector("strOptions", new Vector4(
                                         fOptions.strOptions.size,
                                         0,
                                         Mathf.Deg2Rad * fOptions.strOptions.angle,
                                         fOptions.extPixels));

                if (!setFullDTMap)
                {
                    EShader.SetBuffer(kernelHandle, "gInputBufferfloat", buffers.CBEDTAASigned);
                    setFullDTMap = true;
                }
            }
            else
            {
                EShader.SetBool("sUse", false);
                EShader.SetTexture(kernelHandle, "gInputA", zt);
            }

            // set inner shadow options and textures
            if (fOptions.isOptions.use && fOptions.isOptions.size > 0)
            {
                EShader.SetBool("isUse", true);
                EShader.SetBool("isUseM", fOptions.isOptions.BlendModeNumber == 0);
                EShader.SetBool("isUseD", fOptions.isOptions.blendMode == InnerShadowOptions.ISBlendMode.Darken);

                isContourBuffer = new ComputeBuffer(Options.contourBufferLength, 4);
                isContourBuffer.SetData(fOptions.isOptions.ContourBuffer);
                EShader.SetVector("isColor", fOptions.isOptions.color);
                EShader.SetBuffer(kernelHandle, "fBInputB", isContourBuffer);
                EShader.SetTexture(kernelHandle, "gInputT", fOptions.isOptions.r_mainTexture); // inner shadow R8
                EShader.SetVector("isOptions", new Vector4(0,
                                        fOptions.isOptions.DCos,
                                        fOptions.isOptions.DSin,
                                        0));
            }
            else
            {
                EShader.SetBool("isUse", false);
                // EShader.SetVector("isOptions", Vector4.zero);
                EShader.SetBuffer(kernelHandle, "fBInputB", zb);
                EShader.SetTexture(kernelHandle, "gInputT", zt);
            }

            // set inner glow options and textures
            if (fOptions.igOptions.use && fOptions.igOptions.size > 0)
            {
                EShader.SetBool("igUse", true);
                EShader.SetBool("igUseG", fOptions.igOptions.fillType == InnerGlowOptions.FillType.Gradient);
                EShader.SetBool("igUseL", fOptions.igOptions.blendMode == InnerGlowOptions.IGBlendMode.Lighten);
                EShader.SetBool("igUseS", fOptions.igOptions.blendMode == InnerGlowOptions.IGBlendMode.Screen);

                igContourBuffer = new ComputeBuffer(Options.contourBufferLength, 4);
                igContourBuffer.SetData(fOptions.igOptions.ContourBuffer);
                if (fOptions.igOptions.fillType == InnerGlowOptions.FillType.Gradient)
                {
                    if (!setGradient)
                    {
                        buffers.CreateGradientBuffer();
                        EShader.SetBuffer(kernelHandle, "gInputBufferF4A", buffers.gradients);
                        setGradient = true;
                    }
                }

                EShader.SetVector("igColor", fOptions.igOptions.color);
                EShader.SetBuffer(kernelHandle, "fBInputC", igContourBuffer);
                EShader.SetTexture(kernelHandle, "gInputP", fOptions.igOptions.r_mainTexture);
                EShader.SetVector("igOptions", new Vector4(0,
                                        fOptions.igOptions.range,
                                        0,
                                        0));
            }
            else
            {
                EShader.SetBool("igUse", false);
                EShader.SetBuffer(kernelHandle, "fBInputC", zb);
                EShader.SetTexture(kernelHandle, "gInputP", zt); // outer glow R8
            }

            // set bevel options and textures
            if (fOptions.bevOptions.use && fOptions.bevOptions.size > 0)
            {
                EShader.SetBool("bUse", true);
                EShader.SetBool("bUseSD", fOptions.bevOptions.shadowBlendMode == BevelOptions.BShadowMode.Darken);
                EShader.SetBool("bUseIn", fOptions.bevOptions.PosNumber == 0);
                EShader.SetBool("bUseLL", fOptions.bevOptions.lightBlendMode == BevelOptions.BLightMode.Lighten);
                EShader.SetBool("bUseLS", fOptions.bevOptions.lightBlendMode == BevelOptions.BLightMode.Screen);

                EShader.SetTexture(kernelHandle, "gInputE", fOptions.bevOptions.r_mainTexture); // light R8
                EShader.SetTexture(kernelHandle, "gInputF", fOptions.bevOptions.r_shadowTexture); // shadow R8
                EShader.SetVector("bevLColor", fOptions.bevOptions.lightColor);
                EShader.SetVector("bevSColor", fOptions.bevOptions.shadowColor);

            }
            else
            {
                EShader.SetBool("bUse", false);
                EShader.SetTexture(kernelHandle, "gInputE", zt);
                EShader.SetTexture(kernelHandle, "gInputF", zt);
            }


            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
            if (csContourBuffer != null) { csContourBuffer.Release(); csContourBuffer = null; }
            if (csContourBuffer_1 != null) { csContourBuffer_1.Release(); csContourBuffer_1 = null; }
            if (ogContourBuffer != null) { ogContourBuffer.Release(); ogContourBuffer = null; }
            if (isContourBuffer != null) { isContourBuffer.Release(); isContourBuffer = null; }
            if (igContourBuffer != null) { igContourBuffer.Release(); igContourBuffer = null; }
            zt.Release();
            zb.Release(); zb = null;
        }

        public void GPUCombinerShadow(Texture inputText, ShadowOptions shadowOptions)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            RenderTexture zt = new RenderTexture(2, 2, 0, RenderTextureFormat.R8);

            if (shadowOptions.r_mainTexture != null && shadowOptions.r_mainTexture.IsCreated()) shadowOptions.r_mainTexture.Release();
            shadowOptions.r_mainTexture = new RenderTexture(TextSize.Width, TextSize.Height, 0, RenderTextureFormat.ARGB32);
            shadowOptions.r_mainTexture.filterMode = FilterMode.Bilinear;
            shadowOptions.r_mainTexture.autoGenerateMips = false;
            shadowOptions.r_mainTexture.enableRandomWrite = true;
            shadowOptions.r_mainTexture.Create();

            // set face 
            kernelHandle = EShader.FindKernel("CSShadowCombiner");
            EShader.SetTexture(kernelHandle, "gOutputV", shadowOptions.r_mainTexture);
            EShader.SetTexture(kernelHandle, "gInputV", inputText); // R8
            EShader.SetFloat("fSize", shadowOptions.noise * 0.01f);
            if (shadowOptions.noise > 0)
            {
                EShader.SetTexture(kernelHandle, "gInputH", shadowOptions.noiseTexture);
            }
            else
            {
                EShader.SetTexture(kernelHandle, "gInputH", zt);
            }
            EShader.SetTexture(kernelHandle, "gInputA", shadowOptions.contourTexture);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
            zt.Release();
        }

        #endregion COPY


        #region RENDER DISTANCE BUFFER, MAP
        ComputeBuffer s;
        ComputeBuffer t;
        ComputeBuffer G;

        /// <summary>
        /// Render distance buffer  for euclidean metric from R8 texture r- channel.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void GPURenderFEDTBuffer_R8(Texture inputText, ref ComputeBuffer outBuffer, bool inverseInput)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }
            if (SoftEffects.debuglog)
                Debug.Log("Render float EDT buffer");

            s = new ComputeBuffer(TextSize.WxH, 4); // second phase temp float buffer for s []
            t = new ComputeBuffer(TextSize.WxH, 4); // second phase temp float buffer for t []
            G = new ComputeBuffer(TextSize.WxH, 4); // first phase float buffer
            outBuffer = new ComputeBuffer(TextSize.WxH, 4); // second phase float

            // first phase
            kernelHandle = EShader.FindKernel("CSDistM_R8");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBool("inverse", inverseInput);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            kernelHandle = EShader.FindKernel("CSDistME");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferB", s);
            EShader.SetBuffer(kernelHandle, "gInputBuffer", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);

            s.Release(); s = null;
            t.Release(); t = null;
            G.Release(); G = null;

        }

        /// <summary>
        /// Render distance buffer  for euclidean metric from RGBA texture a- channel.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void GPURenderFEDTBuffer(Texture inputText, ref ComputeBuffer outBuffer, bool inverseInput)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }
            if (SoftEffects.debuglog) Debug.Log("Render float EDT buffer");

            s = new ComputeBuffer(TextSize.WxH, 4);         // second phase temp float buffer for s []
            t = new ComputeBuffer(TextSize.WxH, 4);         // second phase temp float buffer for t []
            G = new ComputeBuffer(TextSize.WxH, 4);         // first phase float buffer
            outBuffer = new ComputeBuffer(TextSize.WxH, 4); // second phase float

            // first phase
            kernelHandle = (inverseInput) ? EShader.FindKernel("CSDistMInverse") : EShader.FindKernel("CSDistM");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            kernelHandle = EShader.FindKernel("CSDistME");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferB", s);
            EShader.SetBuffer(kernelHandle, "gInputBuffer", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);

            s.Release(); s = null;
            t.Release(); t = null;
            G.Release(); G = null;

        }

        /// <summary>
        /// Render full (inpositive-outnegative)  distance buffer for euclidean metric from RGBA texture a- channel.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void GPURenderFEDTSignedBuffer(Texture inputText, ref ComputeBuffer outBuffer)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }
            if (SoftEffects.debuglog) Debug.Log("Render float EDT buffer");

            G = new ComputeBuffer(TextSize.WxH, 4); // first phase float buffer

            //IDT - first phase
            kernelHandle = EShader.FindKernel("CSDistM");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferC", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            s = new ComputeBuffer(TextSize.WxH, 4); // second phase temp float buffer for s []
            t = new ComputeBuffer(TextSize.WxH, 4); // second phase temp float buffer for t []
            outBuffer = new ComputeBuffer(TextSize.WxH, 4); // second phase float
            kernelHandle = EShader.FindKernel("CSDistME");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferB", s);
            EShader.SetBuffer(kernelHandle, "gRWBufferC", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);

            //Signed DT - first phase
            kernelHandle = EShader.FindKernel("CSDistMInverse");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferC", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //Signed DT - second phase
            kernelHandle = EShader.FindKernel("CSDistMESigned");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferB", s);
            EShader.SetBuffer(kernelHandle, "gRWBufferC", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);
            s.Release(); s = null;
            t.Release(); t = null;
            G.Release(); G = null;
        }

        /// <summary>
        /// Render antialiased distance buffer  for euclidean metric from R8 texture r- channel.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void GPURenderAAFEDTBuffer_R8(Texture inputText, ref ComputeBuffer outBuffer, bool inverseInput)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }
            if (SoftEffects.debuglog)
                Debug.Log("Render floatAA buffer");

            s = new ComputeBuffer(TextSize.WxH, 4); // second phase temp float buffer for s []
            t = new ComputeBuffer(TextSize.WxH, 4); // second phase temp float buffer for t []
            G = new ComputeBuffer(TextSize.WxH, 4); // first phase float buffer
            outBuffer = new ComputeBuffer(TextSize.WxH, 4); // second phase 

            // first phase
            kernelHandle = EShader.FindKernel("CSDistMAAFloat_R8");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBool("inverse", inverseInput);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferC", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            kernelHandle = EShader.FindKernel("CSDistMEAAFloat");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", s);
            EShader.SetBuffer(kernelHandle, "gRWBufferC", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);

            s.Release(); s = null;
            t.Release(); t = null;
            G.Release(); G = null;

        }

        /// <summary>
        /// Render antialiased distance buffer  for euclidean metric from RGBA texture a- channel.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void GPURenderAAFEDTBuffer(Texture inputText, ref ComputeBuffer outBuffer, bool inverseInput)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }
            if (SoftEffects.debuglog)
                Debug.Log("Render floatAA buffer");

            s = new ComputeBuffer(TextSize.WxH, 4);   // second phase temp float buffer for s []
            t = new ComputeBuffer(TextSize.WxH, 4);   // second phase temp float buffer for t []
            G = new ComputeBuffer(TextSize.WxH, 4);   // first phase float buffer
            outBuffer = new ComputeBuffer(TextSize.WxH, 4);         // second phase 

            // first phase
            kernelHandle = EShader.FindKernel("CSDistMAAFloat");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBool("inverse", inverseInput);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            kernelHandle = EShader.FindKernel("CSDistMEAAFloat");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", s);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", G);
            EShader.SetBool("boolA", false); //create signed buffer
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);

            s.Release(); s = null;
            t.Release(); t = null;
            G.Release(); G = null;

        }

        /// <summary>
        /// Render antialiased distance buffer  for euclidean metric from RGBA texture a- channel.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void GPURenderAAFEDTSignedBuffer(Texture inputText, ref ComputeBuffer outBuffer)
        {
            TextSize.SetSize(inputText.width, inputText.height);

            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }

            if (SoftEffects.debuglog) Debug.Log("Render floatAA buffer");

            G = new ComputeBuffer(TextSize.WxH, 4);             // first phase float buffer

            //IDT
            // first phase
            kernelHandle = EShader.FindKernel("CSDistMAAFloat");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferCfloat", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            s = new ComputeBuffer(TextSize.WxH, 4);             // second phase temp float buffer for s []
            t = new ComputeBuffer(TextSize.WxH, 4);             // second phase temp float buffer for t []
            outBuffer = new ComputeBuffer(TextSize.WxH, 4);     // second phase outBuffer
            kernelHandle = EShader.FindKernel("CSDistMEAAFloat");
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", s);
            EShader.SetBuffer(kernelHandle, "gRWBufferCfloat", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);

            // signed DT
            // first phase
            kernelHandle = EShader.FindKernel("CSDistMAAFloatInverse");
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferCfloat", G);
            EShader.Dispatch(kernelHandle, TextSize.Width, 1, 1);

            //second phase
            kernelHandle = EShader.FindKernel("CSDistMEAAFloatSigned");
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", t);
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", s);
            EShader.SetBuffer(kernelHandle, "gRWBufferCfloat", G);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, 1, TextSize.Height, 1);
            s.Release(); s = null;
            t.Release(); t = null;
            G.Release(); G = null;
        }

        public void GPUSubstrFloatBuffers(ComputeBuffer inputBufferA, ComputeBuffer inputBufferB, ref ComputeBuffer outBuffer)
        {
            if (SoftEffects.debuglog) Debug.Log("Substract buffers.");
            if (outBuffer != null)
            {
                outBuffer.Release();
                outBuffer = null;
            }
            outBuffer = new ComputeBuffer(inputBufferA.count, 4);

            kernelHandle = EShader.FindKernel("CSSubstrFloatBuffers");
            EShader.SetBuffer(kernelHandle, "fBInputA", inputBufferA);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBufferB);
            EShader.SetBuffer(kernelHandle, "fBOutA", outBuffer);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(inputBufferA.count / 32.0f), 1, 1);
        }

        #endregion RENDER DISTANCE BUFFER, MAP


        #region STROKE

        /// <summary>
        /// Render stroke R8 texture from distance buffer.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outText"></param>
        public void GPURenderStrokeFromBuffer_R8(Texture inputText, ComputeBuffer inputBuffer, bool inSide, int size, ref RenderTexture outText)
        {
            if (inputBuffer == null)
            {
                if (SoftEffects.debuglog) Debug.Log("inputBuffer == null return");
                return;
            }

            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            string kName = "CSDistBufferToStroke_R8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBool("boolA", inSide);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuffer);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetInt("ext", size);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        #endregion STROKE


        #region OUTER GLOW

        /// <summary>
        /// Render outer glow precise texture from antialiased buffer to R8 texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outText"></param>
        public void GPURenderOuterGlowPrecFromBuffer_R8(Texture inputText, ComputeBuffer inputBuffer, int size, float spread, ref RenderTexture outText)
        {
            if (inputBuffer == null)
            {
                if (SoftEffects.debuglog) Debug.Log("inputBuffer == null return");
                return;
            }

            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            string kName = "CSDistBufferToOuterGlowPrec_R8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetFloat("spread", spread * 0.01f);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuffer);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetInt("ext", size);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        /// <summary>
        /// Render outer glow soft texture from antialiased buffer to R8 texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outText"></param>
        public void GPURenderOuterGlowSoftFromBuffer_R8(Texture inputText, ComputeBuffer inputBuffer, int dilateSize, float blurRadius, ref RenderTexture outText)
        {
            if (inputBuffer == null)
            {
                if (SoftEffects.debuglog) Debug.Log("inputBuffer == null return");
                return;
            }

            TextSize.SetSize(inputText.width, inputText.height);

            RenderTexture tempText = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);
            RenderTexture tempText1 = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);

            string kName = "CSDistBufferToOuterGlowSoft_R8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuffer);
            EShader.SetTexture(kernelHandle, "gOutputVf", tempText);
            EShader.SetInt("ext", dilateSize);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            GPURenderBoxBlurTexture_R8(tempText, ref tempText1, blurRadius * 0.5f, 1);
            GPURenderBoxBlurTexture_R8(tempText1, ref outText, blurRadius * 0.5f, 1);

            if (tempText != null && tempText.IsCreated())
            {
                tempText.Release();
                tempText = null;
            }

            if (tempText1 != null && tempText1.IsCreated())
            {
                tempText1.Release();
                tempText1 = null;
            }
        }

        #endregion OUTER GLOW


        #region INNER GLOW

        /// <summary>
        /// Render outer glow precise texture from antialiased buffer to R8 texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outText"></param>
        public void GPURenderInnerGlowPrecFromBuffer_R8(Texture inputText, ComputeBuffer inputBuffer, InnerGlowOptions igOptions, ref RenderTexture outText)
        {
            if (inputBuffer == null)
            {
                if (SoftEffects.debuglog) Debug.Log("inputBuffer == null return");
                return;
            }

            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            string kName = "CSDistBufferToInnerGlowPrec_R8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuffer);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.SetInt("ext", igOptions.size);
            EShader.SetFloat("spread", igOptions.spread * 0.01f);
            EShader.SetBool("boolA", igOptions.position == InnerGlowOptions.Position.FromOuter);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
        }

        /// <summary>
        /// Render outer glow soft texture from antialiased buffer to R8 texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outText"></param>
        public void GPURenderInnerGlowSoftFromBuffer_R8(Texture inputText, ComputeBuffer inputBuffer, int dilateSize, float blurRadius, InnerGlowOptions igOptions, ref RenderTexture outText)
        {
            if (inputBuffer == null)
            {
                if (SoftEffects.debuglog) Debug.Log("inputBuffer == null return");
                return;
            }

            TextSize.SetSize(inputText.width, inputText.height);
            RenderTexture tempText = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);
            RenderTexture tempText1 = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);

            string kName = "CSDistBufferToInnerGlowSoft_R8";
            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuffer);
            EShader.SetTexture(kernelHandle, "gOutputVf", tempText);
            EShader.SetBool("boolA", igOptions.position == InnerGlowOptions.Position.FromOuter);
            EShader.SetInt("ext", dilateSize);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            GPURenderBoxBlurTexture_R8(tempText, ref tempText1, blurRadius * 0.5f, 1);
            GPURenderBoxBlurTexture_R8(tempText1, ref outText, blurRadius * 0.5f, 1);

            if (tempText != null && tempText.IsCreated())
            {
                tempText.Release();
                tempText = null;
            }

            if (tempText1 != null && tempText1.IsCreated())
            {
                tempText1.Release();
                tempText1 = null;
            }
        }

        #endregion INNER GLOW


        #region INNER SHADOW
        internal void GPURenderInnerShadow_R8(Texture inputText, InnerShadowOptions innerShadowOptions, ref RenderTexture outText)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);
            GPURenderDoubleBoxBlur_AR8(inputText, ref outText, innerShadowOptions.size, innerShadowOptions.choke, float.MaxValue);
        }

        /// <summary>
        /// Render outer glow soft texture from antialiased buffer to R8 texture.
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="inputBuffer"></param>
        /// <param name="size"></param>
        /// <param name="outText"></param>
        public void GPURenderInnerShadowFromBuffer_R8(Texture inputText, ComputeBuffer inputBuffer, int dilateSize, float blurRadius, InnerShadowOptions isOptions, ref RenderTexture outText)
        {
            if (inputBuffer == null)
            {
                // if (SoftEffects.debuglog)
                Debug.Log("inputBuffer == null return");
                return;
            }

            TextSize.SetSize(inputText.width, inputText.height);
            RenderTexture tempText = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);
            RenderTexture tempText1 = CreateRenderTexture(RenderTextureFormat.R8, TextSize.Width, TextSize.Height, FilterMode.Point);


            string kName = "CSDistBufferToInnerGlowSoft_R8"; // use inner glow kernel

            kernelHandle = EShader.FindKernel(kName);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuffer);
            EShader.SetTexture(kernelHandle, "gOutputVf", tempText);
            EShader.SetBool("boolA", true); // from outer
            EShader.SetInt("ext", dilateSize);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            GPURenderBoxBlurTexture_R8(tempText, ref tempText1, blurRadius * 0.5f, 1);
            GPURenderBoxBlurTexture_R8(tempText1, ref outText, blurRadius * 0.5f, 1);

            if (tempText != null && tempText.IsCreated())
            {
                tempText.Release();
                tempText = null;
            }

            if (tempText1 != null && tempText1.IsCreated())
            {
                tempText1.Release();
                tempText1 = null;
            }
        }

        #endregion INNER SHADOW


        #region BEVEL
        ComputeBuffer cutBuffer;
        ComputeBuffer cosineBuffer;
        ComputeBuffer cutBlurBuffer;
        ComputeBuffer contourBuffer;

        public void GPURenderBevelInside_R8(Texture inputText, ComputeBuffer dtBuffer, ref RenderTexture outTextLight, ref RenderTexture outTextShadow, BevelOptions bevelOptions)
        {
            TextSize.SetSize(inputText.width, inputText.height);
            float tg25 = 0.00466308f;
            int pRow = 16;

            cutBuffer = new ComputeBuffer(TextSize.WxH, 4);//  Mkey.PrintData.BufTostring(dtBuffer, inputText.width, inputText.height, pRow, -1, false);
            cosineBuffer = new ComputeBuffer(TextSize.WxH, 4);

            bool useSmooth = (bevelOptions.bTechnique == BevelOptions.BevelTechnique.Smooth);

            //1)cut -> cutBuffer
            kernelHandle = (useSmooth) ? EShader.FindKernel("CSSepBevelCutInSmooth") : EShader.FindKernel("CSSepBevelCutIn");
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", dtBuffer);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", cutBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetFloat("fSize", (!useSmooth) ? bevelOptions.size : float.MaxValue);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1); // Mkey.PrintData.BufTostring(cutBuffer, inputText.width, inputText.height, pRow, -1, false);

            // 1a) preblur
            if (useSmooth)
                GPURenderDoubleBoxBlurBuffer(inputText, cutBuffer, out cutBlurBuffer, bevelOptions.size, 100);
            else GPURenderBoxBlurBuffer(inputText, cutBuffer, out cutBlurBuffer, 1.0f, 0);
            cutBuffer.Release(); cutBuffer = null;   // Mkey.PrintData.BufTostring(cutBlurBuffer, inputText.width, inputText.height, pRow, -1, false);

            //2) render cosine map -> cosineBuffer
            kernelHandle = EShader.FindKernel("CSCosineMap");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", cutBlurBuffer);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", cosineBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetVector("light", new Vector4(bevelOptions.LightDirection.x, bevelOptions.LightDirection.y, bevelOptions.LightDirection.z, bevelOptions.depth * tg25));
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);//  Mkey.PrintData.BufTostring(cosineBuffer, inputText.width, inputText.height, pRow, -1, false);
            cutBlurBuffer.Release(); cutBlurBuffer = null;

            // 3) blur cosine buffer ->  cutBlurBuffer
            if (bevelOptions.smoothing > 8)
                GPURenderGaussianBuffer_16(inputText, cosineBuffer, out cutBlurBuffer, bevelOptions.smoothing);
            else
                GPURenderGaussianBuffer_8(inputText, cosineBuffer, out cutBlurBuffer, bevelOptions.smoothing);
            cosineBuffer.Release(); cosineBuffer = null; // Mkey.PrintData.BufTostring(cutBlurBuffer, inputText.width, inputText.height, pRow, -1, false);

            // 4)  cosine buffer ->  light and shadow texture 
            contourBuffer = new ComputeBuffer(Options.contourBufferLength, 4);
            contourBuffer.SetData(bevelOptions.ContourBuffer);// Mkey.PrintData.BufTostring(bevelOptions.ContourBuffer, Options.contourBufferLength,1);

            ReCreateRenderTexture(ref outTextLight, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);
            ReCreateRenderTexture(ref outTextShadow, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            kernelHandle = EShader.FindKernel("CSSepBevel_R8");
            EShader.SetTexture(kernelHandle, "gOutputDf", outTextLight);
            EShader.SetTexture(kernelHandle, "gOutputVf", outTextShadow);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", cutBlurBuffer);
            EShader.SetBuffer(kernelHandle, "fBInputA", contourBuffer);
            EShader.SetBuffer(kernelHandle, "fBInputB", dtBuffer);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetBool("boolA", false); // smooth cut for shadow and light along distanace buffer - used for outside bevel
            EShader.SetVector("light", new Vector4(bevelOptions.LightDirection.x, bevelOptions.LightDirection.y, bevelOptions.LightDirection.z, bevelOptions.size));
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);
            /**/
            cutBlurBuffer.Release(); cutBlurBuffer = null;
            contourBuffer.Release(); contourBuffer = null;
        }

        public void GPURenderBevelOutside_R8(Texture inputText, ComputeBuffer dtBuffer, ref RenderTexture outTextLight, ref RenderTexture outTextShadow, BevelOptions bevelOptions)
        {
            int pRow = 16;
            float tg25 = 0.00466308f;
            TextSize.SetSize(inputText.width, inputText.height);
            ReCreateRenderTexture(ref outTextLight, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);
            ReCreateRenderTexture(ref outTextShadow, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            contourBuffer = new ComputeBuffer(Options.contourBufferLength, 4);
            contourBuffer.SetData(bevelOptions.ContourBuffer);

            cutBuffer = new ComputeBuffer(TextSize.WxH, 4); //Mkey.PrintData.BufTostring(dtBuffer, inputText.width, inputText.height, pRow, -1, false);
            cosineBuffer = new ComputeBuffer(TextSize.WxH, 4);

            bool useSmooth = (bevelOptions.bTechnique == BevelOptions.BevelTechnique.Smooth);

            //1) subtract buffers and cut -> cutBuffer
            kernelHandle = EShader.FindKernel("CSSepBevelCutOut");
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", dtBuffer);
            EShader.SetBool("boolA", !useSmooth); // 
            EShader.SetBool("inverse", !useSmooth); // inverse ouput
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", cutBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetFloat("fSize", bevelOptions.size);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1); //Mkey.PrintData.BufTostring(cutBuffer, inputText.width, inputText.height, pRow, -1, false);

            // 1a) 
            if (useSmooth)
                GPURenderDoubleBoxBlurBuffer(inputText, cutBuffer, out cutBlurBuffer, bevelOptions.size, 100);
            else
                GPURenderBoxBlurBuffer(inputText, cutBuffer, out cutBlurBuffer, 1.0f, 0);
            cutBuffer.Release(); cutBuffer = null; //Mkey.PrintData.BufTostring(cutBlurBuffer, inputText.width, inputText.height, pRow, -1, false);

            //2) render dt -> cosineBuffer
            kernelHandle = EShader.FindKernel("CSCosineMap");
            EShader.SetBuffer(kernelHandle, "gRWBufferBfloat", cutBlurBuffer);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", cosineBuffer);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetVector("light", new Vector4(bevelOptions.LightDirection.x, bevelOptions.LightDirection.y, bevelOptions.LightDirection.z, bevelOptions.depth * tg25));
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1); //Mkey.PrintData.BufTostring(cosineBuffer, inputText.width, inputText.height, pRow, -1, false);
            cutBlurBuffer.Release(); cutBlurBuffer = null;

            // 3) blur cosine buffer ->  cutBlurBuffer
            if (bevelOptions.smoothing > 8)
                GPURenderGaussianBuffer_16(inputText, cosineBuffer, out cutBlurBuffer, bevelOptions.smoothing);
            else
                GPURenderGaussianBuffer_8(inputText, cosineBuffer, out cutBlurBuffer, bevelOptions.smoothing);
            cosineBuffer.Release(); cosineBuffer = null; //Mkey.PrintData.BufTostring(cutBlurBuffer, inputText.width, inputText.height, pRow, -1, false);

            // 4)  cosine buffer ->  light and shadow texture 
            kernelHandle = EShader.FindKernel("CSSepBevel_R8");
            EShader.SetTexture(kernelHandle, "gOutputDf", outTextLight);
            EShader.SetTexture(kernelHandle, "gOutputVf", outTextShadow);
            EShader.SetBuffer(kernelHandle, "gRWBufferAfloat", cutBlurBuffer);
            EShader.SetBuffer(kernelHandle, "fBInputB", dtBuffer);
            EShader.SetFloat("fSize", bevelOptions.size);
            EShader.SetBool("boolA", true); // smooth cut for shadow and light along distanace buffer
            EShader.SetBuffer(kernelHandle, "fBInputA", contourBuffer);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetInt("width", TextSize.Width);
            EShader.SetInt("height", TextSize.Height);
            EShader.SetVector("light", new Vector4(bevelOptions.LightDirection.x, bevelOptions.LightDirection.y, bevelOptions.LightDirection.z, bevelOptions.size / 1.0f));
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            cutBlurBuffer.Release(); cutBlurBuffer = null;
            contourBuffer.Release(); contourBuffer = null;
        }

        #endregion BEVEL


        #region NORMALIZE
        /// <summary>
        /// Normalize inputbuffer to float [0,1]  to outTexture R8; out lower minCut  = 0; out higher maxCut = 1
        /// </summary>
        public void Normalize_R8(ComputeBuffer inputBuff, ref RenderTexture outText, int textureWidth, int textureHeight, float minCut, float maxCut, bool cutNegative)
        {
            ComputeBuffer N = new ComputeBuffer(textureHeight * 2, 4); // min, max values from each row
            ComputeBuffer N1 = new ComputeBuffer(2, 4); // max, min values

            TextSize.SetSize(textureWidth, textureHeight);
            ReCreateRenderTexture(ref outText, RenderTextureFormat.R8, TextSize.Width, TextSize.Height);

            kernelHandle =(cutNegative)? EShader.FindKernel("NormalizeFCN") : EShader.FindKernel("NormalizeF");
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.SetBuffer(kernelHandle, "fBInputA", inputBuff);
            EShader.SetBuffer(kernelHandle, "fBOutA", N);
            EShader.Dispatch(kernelHandle, 1, textureHeight, 1);

            kernelHandle = EShader.FindKernel("NormalizeF1");
            EShader.SetInt("height", textureHeight);
            EShader.SetInt("width", textureWidth);
            EShader.SetBuffer(kernelHandle, "fBInputA", N);
            EShader.SetBuffer(kernelHandle, "fBOutA", N1);
            EShader.Dispatch(kernelHandle, 1, 1, 1);

            kernelHandle = EShader.FindKernel("NormalizeF2");
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.SetFloat("minV", minCut);
            EShader.SetFloat("maxV", maxCut);
            EShader.SetBuffer(kernelHandle, "fBInputA", inputBuff);
            EShader.SetBuffer(kernelHandle, "fBOutA", N1);
            EShader.SetTexture(kernelHandle, "gOutputVf", outText);
            EShader.Dispatch(kernelHandle, TextSize.W32d, TextSize.H32d, 1);

            N.Release(); N = null;
            N1.Release(); N1 = null;
        }

        /// <summary>
        /// Normalize inputbuffer to float [0,1]  outputBuffer; out lower minCut  = 0; out higher maxCut = 1
        /// </summary>
        public void Normalize_Buf(ComputeBuffer inputBuff, ref ComputeBuffer outputBuff, int textureWidth, int textureHeight, float minCut, float maxCut)
        {
            ComputeBuffer N = new ComputeBuffer(textureHeight * 2, 4); // min, max values from each row
            ComputeBuffer N1 = new ComputeBuffer(2, 4); // max, min values

            if (outputBuff != null)
            {
                outputBuff.Release();
                outputBuff = null;
            }

            outputBuff = new ComputeBuffer(textureHeight * textureWidth, 4);

            kernelHandle = EShader.FindKernel("NormalizeF");
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.SetBuffer(kernelHandle, "fBInputA", inputBuff);
            EShader.SetBuffer(kernelHandle, "fBOutA", N);
            EShader.Dispatch(kernelHandle, 1, textureHeight, 1);

            kernelHandle = EShader.FindKernel("NormalizeF1");
            EShader.SetInt("height", textureHeight);
            EShader.SetInt("width", textureWidth);
            EShader.SetBuffer(kernelHandle, "fBInputA", N);
            EShader.SetBuffer(kernelHandle, "fBOutA", N1);
            EShader.Dispatch(kernelHandle, 1, 1, 1);

            kernelHandle = EShader.FindKernel("NormalizeFB");
            EShader.SetInt("width", textureWidth);
            EShader.SetInt("height", textureHeight);
            EShader.SetFloat("minV", minCut);
            EShader.SetFloat("maxV", maxCut);
            EShader.SetBuffer(kernelHandle, "gInputBufferfloat", inputBuff);
            EShader.SetBuffer(kernelHandle, "fBInputA", N1);
            EShader.SetBuffer(kernelHandle, "fBOutA", outputBuff);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(textureWidth / 32.0f), Mathf.CeilToInt(textureHeight / 32.0f), 1);

            N.Release(); N = null;
            N1.Release(); N1 = null;
        }

        public float GetMaxValue(ComputeBuffer inputBuff, int textureWidth, int textureHeight)
        {
            ComputeBuffer N = new ComputeBuffer(textureHeight, 4); // 
            ComputeBuffer N1 = new ComputeBuffer(1, 4); // 

            kernelHandle = EShader.FindKernel("NormalizeF");
            EShader.SetInt("width", textureWidth);
            EShader.SetBuffer(kernelHandle, "fBInputA", inputBuff);
			EShader.SetBuffer(kernelHandle, "fBOutA", N);
            EShader.Dispatch(kernelHandle, 1, textureHeight, 1);

            kernelHandle = EShader.FindKernel("NormalizeF1");
            EShader.SetInt("height", textureHeight);
            EShader.SetBuffer(kernelHandle, "fBInputA", N);
            EShader.SetBuffer(kernelHandle, "fBOutA", N1);
            EShader.Dispatch(kernelHandle, 1, 1, 1);
            float[] data = new float[1];
            N1.GetData(data);

            N.Release(); N = null;
            N1.Release(); N1 = null;
            return data[0];
        }
        #endregion NORMALIZE


        #region MASK
        /// <summary>
        /// Create mask buffer from a-channel of the RGBA texture. Use CSMask kernel. Output buffer gRWBufferA - int. 
        /// Buffer value 0 or 1 (if gInputV[dispatchThreadID.xy].a > 0)
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void CreateMaskBuffer(Texture inputText, ref ComputeBuffer outBuffer)
        {
            kernelHandle = EShader.FindKernel("CSMask");
            EShader.SetInt("width", inputText.width);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", outBuffer);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(inputText.width / 32.0f), Mathf.CeilToInt(inputText.height / 32.0f), 1);
        }

        /// <summary>
        /// Create mask buffer from r-channel of the R8 texture. Use CSMask_R8 kernel. Output buffer gRWBufferA - int. 
        /// Buffer value 0 or 1 (if gInputV[dispatchThreadID.xy].a > 0)
        /// </summary>
        /// <param name="inputText"></param>
        /// <param name="outBuffer"></param>
        public void CreateMaskBuffer_R8(Texture inputText, ref ComputeBuffer outBuffer)
        {
            kernelHandle = EShader.FindKernel("CSMask_R8");
            EShader.SetInt("width", inputText.width);
            EShader.SetTexture(kernelHandle, "gInputV", inputText);
            EShader.SetBuffer(kernelHandle, "gRWBufferA", outBuffer);
            EShader.Dispatch(kernelHandle, Mathf.CeilToInt(inputText.width / 32.0f), Mathf.CeilToInt(inputText.height / 32.0f), 1);
        }
        #endregion MASK

        private void ReCreateRenderTexture(ref RenderTexture rT, RenderTextureFormat rF, int width, int height)
        {
            if (rT != null && rT.IsCreated()) rT.Release();
            rT = new RenderTexture(width, height, 0, rF);
            rT.enableRandomWrite = true;
            rT.autoGenerateMips = false;
            rT.Create();
        }

        private RenderTexture CreateRenderTexture(RenderTextureFormat rF, int width, int height, FilterMode fMode)
        {
            RenderTexture rT = new RenderTexture(width, height, 0, rF);
            rT.enableRandomWrite = true;
            rT.autoGenerateMips = false;
            rT.filterMode = fMode;
            rT.Create();
            return rT;
        }
#endif
    }

    public static class TextSize
    {
        public static int Width;
        public static int Height;
        public static int W32d;
        public static int H32d;
        public static int WxH;

        public static void SetSize(int width, int height)
        {
            if (Width != width || Height != height)
            {
                Width = width;
                Height = height;
                W32d = Mathf.CeilToInt(Width / 32.0f);
                H32d = Mathf.CeilToInt(Height / 32.0f);
                WxH = Width * Height;
            }
        }
    }

    [Serializable]
    public struct int2
    {
        public int X;
        public int Y;
        public int2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override string ToString()
        {
            return (X + " : " + Y + " ;");
        }

        public static int2 operator +(int2 a, int2 b)
        {
            int2 i = new int2();
            i.X = a.X + b.X;
            i.Y = a.Y + b.Y;
            return i;
        }

        public static int2 operator -(int2 a, int2 b)
        {
            int2 i = new int2();
            i.X = a.X - b.X;
            i.Y = a.Y - b.Y;
            return i;
        }

        public static int2 operator /(int2 a, int b)
        {
            int2 i = new int2();
            i.X = a.X / b;
            i.Y = a.Y / b;
            return i;
        }

        public static int2 operator *(int2 a, int b)
        {
            int2 i = new int2();
            i.X = a.X * b;
            i.Y = a.Y * b;
            return i;
        }

        public static bool operator ==(int2 a, int2 b)
        {
            return (a.X == b.X && a.Y == b.Y);
        }

        public static bool operator !=(int2 a, int2 b)
        {
            return (a.X != b.X || a.Y != b.Y);
        }

        public int ChessLength
        {
            get { return (Mathf.Abs(X) + Mathf.Abs(Y)); }
        }
    }

    public enum BlendMode { Normal, Darken, Muliplie, Lighten, Screen, Overlay }

    public enum DistFieldType { EDT, MDT, CDT }

}