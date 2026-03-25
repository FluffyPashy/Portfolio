Shader "Custom/Vertex Color Terrain"
{
    Properties
    {
        _BaseColor ("Tint", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "Queue" = "Geometry"
            "RenderPipeline" = "UniversalPipeline"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
        CBUFFER_END

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            half4 color : COLOR;
        };

        struct Varyings
        {
            float4 positionHCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            half3 normalWS : TEXCOORD1;
            half4 color : COLOR;
        };

        struct DepthVaryings
        {
            float4 positionHCS : SV_POSITION;
            half3 normalWS : TEXCOORD0;
        };

        Varyings vert(Attributes input)
        {
            Varyings output;
            VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
            VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);

            output.positionHCS = positionInputs.positionCS;
            output.positionWS = positionInputs.positionWS;
            output.normalWS = normalInputs.normalWS;
            output.color = input.color;
            return output;
        }

        half4 frag(Varyings input) : SV_Target
        {
            half3 normalWS = normalize(input.normalWS);
            Light mainLight = GetMainLight();
            half ndotl = saturate(dot(normalWS, mainLight.direction));
            half3 ambient = SampleSH(normalWS);
            half3 litColor = input.color.rgb * _BaseColor.rgb * (ambient + mainLight.color * (ndotl + 0.15h));
            return half4(litColor, input.color.a * _BaseColor.a);
        }

        DepthVaryings vertDepthOnly(Attributes input)
        {
            DepthVaryings output;
            VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
            VertexNormalInputs normalInputs = GetVertexNormalInputs(input.normalOS);
            output.positionHCS = positionInputs.positionCS;
            output.normalWS = normalInputs.normalWS;
            return output;
        }

        half4 fragDepthOnly(DepthVaryings input) : SV_Target
        {
            return 0;
        }

        DepthVaryings vertDepthNormals(Attributes input)
        {
            return vertDepthOnly(input);
        }

        half4 fragDepthNormals(DepthVaryings input) : SV_Target
        {
            float3 normalWS = normalize(input.normalWS);
            return float4(PackNormalOctRectEncode(normalWS) * 0.5 + 0.5, 0.0, 0.0);
        }

        DepthVaryings vertShadowCaster(Attributes input)
        {
            DepthVaryings output;
            VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
            output.positionHCS = positionInputs.positionCS;
            output.normalWS = 0;
            return output;
        }

        half4 fragShadowCaster(DepthVaryings input) : SV_Target
        {
            return 0;
        }
        ENDHLSL

        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            Cull Back
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vertShadowCaster
            #pragma fragment fragShadowCaster
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            Cull Back
            ZWrite On
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex vertDepthOnly
            #pragma fragment fragDepthOnly
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormalsOnly" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma vertex vertDepthNormals
            #pragma fragment fragDepthNormals
            ENDHLSL
        }
    }

    FallBack Off
}
