Shader "Hidden/CloudFullscreen"
{
    Properties
    {
        _ValueNoise ("Noise Texture", 2D) = "white" {}
        _MinHeight ("Min Height", Float) = 0
        _MaxHeight ("Max Height", Float) = 5
        _FadeDist ("Fade Distance", Float) = 2
        _Scale ("Scale", Float) = 5
        _Steps ("Raymarch Steps", Float) = 50
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" }
        Pass
        {
            Name "FullscreenCloudPass"
            Tags { "LightMode" = "UniversalFullscreen" }

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment Frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            sampler2D _ValueNoise;
            float _MinHeight, _MaxHeight, _FadeDist, _Scale, _Steps;
            float4 _SunDir;
            float3 _CameraPosWS;
            float4x4 _FrustumCornersWS;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings FullscreenVert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                float3 rayOrigin = _CameraPosWS;
                float3 rayDir = normalize(mul(_FrustumCornersWS, float4(IN.uv * 2 - 1, 1, 1)).xyz);
                float3 pos = rayOrigin;

                float alpha = 0;
                for (int j = 0; j < _Steps; j++)
                {
                    float height = pos.y;
                    if (height >= _MinHeight && height <= _MaxHeight)
                    {
                        float density = tex2D(_ValueNoise, pos.xz * _Scale).r;
                        alpha += density * 0.05;
                    }

                    pos += rayDir * (_MaxHeight / _Steps);
                    if (alpha >= 1.0) break;
                }

                return half4(1, 1, 1, alpha);
            }
            ENDHLSL
        }
    }
}
