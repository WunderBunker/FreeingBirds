Shader "Custom/ColorBlind"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ColorBlind"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            int _Type;

            static float3x3 color_matrices[4] =
            {
                float3x3(
                1,0,0,
                0,1,0,
                0,0,1
                ),
                float3x3(
                0.567,0.433,0,
                0.558,0.442,0,
                0,0.242,0.758
                ),
                float3x3(
                0.625,0.375,0,
                0.7,0.3,0,
                0,0.3,0.7
                ),
                float3x3(
                0.95,0.05,0,
                0,0.433,0.567,
                0,0.475,0.525
                )
            };

            half4 Frag(Varyings input) : SV_Target0
            {
                float2 uv = input.texcoord.xy;
                half4 color = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearRepeat, uv);

                float3 shifted = mul(color.rgb, color_matrices[_Type]);
                return half4(shifted, color.a); 
            }
            ENDHLSL
        }
    }
}
