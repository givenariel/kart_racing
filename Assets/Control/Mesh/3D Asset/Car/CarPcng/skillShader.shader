Shader "Custom/WavyCloth_URP"
{
    Properties
    {
        _BaseMap ("Base Texture", 2D) = "white" {}
        _Speed ("Wave Speed", Range(0,10)) = 3
        _Amp ("Wave Amplitude", Range(0,0.5)) = 0.1
        _Freq ("Wave Frequency", Range(1,10)) = 5
        _Detail ("Wave Detail", Range(0,5)) = 2
        _ScrollProgress ("Scroll Progress", Range(0,1)) = 0 // Diatur dari script
        _BlackThreshold ("Black Threshold", Range(0, 0.2)) = 0.05
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            Name "MainPass"
            Tags { "LightMode"="UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float4 positionCS : SV_POSITION;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            float _Speed;
            float _Amp;
            float _Freq;
            float _Detail;
            float _ScrollProgress; // Diatur dari script
            float _BlackThreshold;

            Varyings vert(Attributes IN)
{
    Varyings OUT;

    // **Waktu untuk animasi gelombang**
    float time = _Time.y * _Speed;

    // **Gunakan posisi X untuk menentukan intensitas gelombang**
    float distanceFactor = saturate((IN.positionOS.x - 0.25) * 2.0); // Pangkal diam, lalu naik bertahap

    // Amplitudo & Frekuensi bertahap sesuai jarak dari pangkal
    float dynamicAmp = _Amp * distanceFactor; // Semakin jauh, semakin besar
    float dynamicFreq = _Freq + (_Freq * distanceFactor * 2.0); // Semakin jauh, semakin rapat

    // **Noise untuk variasi per vertex**
    float noiseOffset = sin(IN.positionOS.x * 2.0 + IN.positionOS.z * 1.5 + time * 0.3) * 0.2;
    float noiseMotion = cos(IN.positionOS.x * 3.0 - IN.positionOS.z * 2.0 + time * 0.4) * 0.15;

    // **Gelombang utama**
    float wavePhase = (IN.positionOS.x * 1.5 + IN.positionOS.z * 1.2) * dynamicFreq;
    float waveMotion = sin(wavePhase + time) * dynamicAmp;
    float waveNoise = cos((wavePhase * 0.8) - time * 0.5) * (dynamicAmp * 0.7);

    // **Gabungkan semua pergerakan**
    float waveHeight = waveMotion + waveNoise + noiseMotion;
    float3 waveDisplacement = float3(
        sin(time + IN.positionOS.z) * 0.05,  // Gerakan ke samping acak
        waveHeight,                         // Gerakan naik-turun utama
        cos(time * 0.8 + IN.positionOS.x) * 0.05 // Gerakan ke depan-belakang
    );

    // **Pangkal tidak bergelombang (distanceFactor = 0)**
    IN.positionOS.xyz += waveDisplacement * distanceFactor;

    // **UV Scroll ke atas secara bertahap**
    OUT.uv = IN.uv;
    OUT.uv.y += _ScrollProgress; // Progress diatur dari script

    // Transformasi ke World Space
    OUT.positionWS = TransformObjectToWorld(IN.positionOS);
    OUT.normalWS = normalize(mul((float3x3)UNITY_MATRIX_M, IN.normalOS));
    OUT.positionCS = TransformWorldToHClip(OUT.positionWS);

    return OUT;
}



            half4 frag(Varyings IN) : SV_Target
            {
                float4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // **Hitam ? Transparan**
                float brightness = dot(baseColor.rgb, float3(0.299, 0.587, 0.114));
                if (brightness < _BlackThreshold)
                {
                    baseColor.a = 0;
                }

                return baseColor;
            }
            ENDHLSL
        }
    }
}
