Shader "Holistic/8/WavyObject_URP"
{
    Properties
    {
        _MainTex ("Water Texture", 2D) = "white" {}
        _FoamTex ("Foam Texture", 2D) = "white" {}
        _Tint("Water Tint", Color) = (0.1, 0.5, 0.8, 1)
        _Freq("Wave Frequency", Range(5, 50)) = 20
        _Speed("Wave Speed", Range(0, 10)) = 2
        _Amp("Wave Amplitude", Range(0, 0.5)) = 0.1
        _Detail("Wave Detail", Range(0, 5)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            Name "MainPass"
            Tags { "LightMode"="UniversalForward" }

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

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_FoamTex);
            SAMPLER(sampler_FoamTex);

            float4 _Tint;
            float _Freq;
            float _Speed;
            float _Amp;
            float _Detail;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Waktu animasi
                float time = _Time.y * _Speed;

                // Konversi normal dari object space ke world space agar smooth
                float3 normalWS = normalize(mul((float3x3)UNITY_MATRIX_M, IN.normalOS));

                // Gunakan posisi lokal agar setiap vertex memiliki gelombang berbeda
                float wavePhase = (IN.positionOS.x + IN.positionOS.z) * _Freq + time;

                // Gunakan sin dan cos agar variasi lebih smooth dan tidak membesar/mengerut
                float wave1 = sin(wavePhase) * _Amp * 0.5;
                float wave2 = cos(wavePhase * 1.1) * _Amp * 0.3;
                float wave3 = sin(wavePhase * 0.8) * _Amp * 0.2;

                // Total gelombang dengan interpolasi vertex sekitar agar tetap menyatu
                float waveHeight = wave1 + wave2 + wave3;

                // Pastikan vertex hanya naik-turun sesuai normalnya
                IN.positionOS.xyz += normalWS * waveHeight;

                // Transform ke world space setelah perubahan
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.normalWS = normalWS; // Simpan normal yang sudah diinterpolasi
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.uv = IN.uv;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 water = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv).rgb;
                float3 foam = SAMPLE_TEXTURE2D(_FoamTex, sampler_FoamTex, IN.uv).rgb;
                
                float3 finalColor = (water + foam) * 0.5 * _Tint.rgb;
                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}
