Shader "HDRP/ToonShader"
{
    Properties
    {
        [MainTexture] _MainTex ("Albedo", 2D) = "white" {}
        [MainColor] _Color ("Color", Color) = (1,1,1,1)
        
        [Header(Toon Shading)]
        _ToonRamp ("Toon Ramp", 2D) = "white" {}
        _ToonSteps ("Toon Steps", Range(1, 10)) = 3
        _ToonSmoothness ("Toon Smoothness", Range(0, 1)) = 0.1
        
        [Header(Rim Lighting)]
        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 1
        
        [Header(Shadow)]
        _ShadowColor ("Shadow Color", Color) = (0.3, 0.3, 0.3, 1)
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.5
        
        [Header(Outline)]
        _OutlineWidth ("Outline Width", Range(0, 0.1)) = 0.01
        _OutlineColor ("Outline Color", Color) = (0,0,0,1)
        
        [Header(Specular)]
        _SpecularColor ("Specular Color", Color) = (1,1,1,1)
        _SpecularGloss ("Specular Gloss", Range(1, 100)) = 10
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderPipeline" = "HDRenderPipeline"
            "RenderType" = "Opaque" 
            "Queue" = "Geometry"
        }
        
        HLSLINCLUDE
        #pragma target 4.5
        #pragma only_renderers d3d11 playstation xboxone xboxseries vulkan metal switch
        
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/ShaderLibrary/ShaderVariables.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
        #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Lighting/LightLoop/LightLoopDef.hlsl"
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);
        TEXTURE2D(_ToonRamp);
        SAMPLER(sampler_ToonRamp);
        
        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float4 _Color;
            float4 _RimColor;
            float4 _ShadowColor;
            float4 _OutlineColor;
            float4 _SpecularColor;
            float _ToonSteps;
            float _ToonSmoothness;
            float _RimPower;
            float _RimIntensity;
            float _ShadowIntensity;
            float _OutlineWidth;
            float _SpecularGloss;
            float _SpecularIntensity;
        CBUFFER_END
        
        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS : NORMAL;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };
        
        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS : TEXCOORD1;
            float2 uv : TEXCOORD2;
            float3 viewDirWS : TEXCOORD3;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };
        
        float3 ToonShading(float3 lightColor, float3 lightDir, float3 normal, float3 viewDir, float3 albedo)
        {
            float NdotL = dot(normal, lightDir);
            
            // Quantize the lighting
            float toonNdotL = smoothstep(0, _ToonSmoothness, NdotL);
            toonNdotL = floor(toonNdotL * _ToonSteps) / _ToonSteps;
            
            // Sample toon ramp if available
            float2 rampUV = float2(toonNdotL, 0.5);
            float3 ramp = SAMPLE_TEXTURE2D(_ToonRamp, sampler_ToonRamp, rampUV).rgb;
            
            // Combine with light color and albedo
            float3 diffuse = lightColor * albedo * ramp * toonNdotL;
            
            // Add shadow color in unlit areas
            float3 shadow = lerp(albedo * _ShadowColor.rgb, diffuse, saturate(toonNdotL + _ShadowIntensity));
            
            return shadow;
        }
        
        float3 ToonSpecular(float3 lightColor, float3 lightDir, float3 normal, float3 viewDir)
        {
            float3 halfVector = normalize(lightDir + viewDir);
            float NdotH = saturate(dot(normal, halfVector));
            float specular = pow(NdotH, _SpecularGloss);
            
            // Quantize specular
            specular = smoothstep(0.005, 0.01, specular);
            
            return lightColor * _SpecularColor.rgb * specular * _SpecularIntensity;
        }
        
        float3 RimLighting(float3 normal, float3 viewDir)
        {
            float rim = 1.0 - saturate(dot(normal, viewDir));
            rim = pow(rim, _RimPower);
            return _RimColor.rgb * rim * _RimIntensity;
        }
        
        ENDHLSL
        
        // Main rendering pass
        Pass
        {
            Name "Forward"
            Tags { "LightMode" = "Forward" }
            
            Cull Back
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_instancing
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionWS = TransformObjectToWorld(input.positionOS.xyz);
                output.positionCS = TransformWorldToHClip(output.positionWS);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.viewDirWS = normalize(_WorldSpaceCameraPos - output.positionWS);
                
                return output;
            }
            
            float4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                // Sample main texture
                float4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv) * _Color;
                
                // Normalize vectors
                float3 normalWS = normalize(input.normalWS);
                float3 viewDirWS = normalize(input.viewDirWS);
                
                // Get main light
                Light mainLight = GetMainLight();
                float3 lightDir = normalize(mainLight.direction);
                float3 lightColor = mainLight.color;
                
                // Calculate toon shading
                float3 diffuse = ToonShading(lightColor, lightDir, normalWS, viewDirWS, albedo.rgb);
                
                // Add specular
                float3 specular = ToonSpecular(lightColor, lightDir, normalWS, viewDirWS);
                
                // Add rim lighting
                float3 rim = RimLighting(normalWS, viewDirWS);
                
                // Combine all lighting
                float3 finalColor = diffuse + specular + rim;
                
                // Add ambient lighting
                float3 ambient = SampleSH(normalWS) * albedo.rgb * 0.3;
                finalColor += ambient;
                
                return float4(finalColor, albedo.a);
            }
            ENDHLSL
        }
        
        // Outline pass
        Pass
        {
            Name "Outline"
            Tags { "LightMode" = "SRPDefaultUnlit" }
            
            Cull Front
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline
            #pragma multi_compile_instancing
            
            struct AttributesOutline
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct VaryingsOutline
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            VaryingsOutline vertOutline(AttributesOutline input)
            {
                VaryingsOutline output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                
                // Expand vertices along normal for outline
                positionWS += normalWS * _OutlineWidth;
                
                output.positionCS = TransformWorldToHClip(positionWS);
                
                return output;
            }
            
            float4 fragOutline(VaryingsOutline input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                return _OutlineColor;
            }
            ENDHLSL
        }
        
        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back
            
            HLSLPROGRAM
            #pragma vertex vertShadow
            #pragma fragment fragShadow
            #pragma multi_compile_instancing
            
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Material.hlsl"
            #include "Packages/com.unity.render-pipelines.high-definition/Runtime/Material/Unlit/Unlit.hlsl"
            
            struct AttributesShadow
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            struct VaryingsShadow
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            
            VaryingsShadow vertShadow(AttributesShadow input)
            {
                VaryingsShadow output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                
                output.positionCS = TransformObjectToHClip(TransformObjectToWorld(input.positionOS.xyz));
                
                return output;
            }
            
            float4 fragShadow(VaryingsShadow input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
    
    CustomEditor "UnityEditor.ShaderGraph.PBRMasterGUI"
}