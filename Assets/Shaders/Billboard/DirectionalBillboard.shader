Shader "Unlit/DirectionalBillboard"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Columns ("Columns", Integer) = 4
        _Rows ("Rows", Integer) = 2
        _TotalFrames ("Total Active Frames", Integer) = 8
        _ObjectRotation ("Object Rotation", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "TransparentCutout"
            "Queue" = "AlphaTest"
            "RenderPipeline" = "UniversalPipeline"
            "DisableBatching" = "True"
            "IgnoreProjector" = "True"
        }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        #define PI 3.14159265

        CBUFFER_START(UnityPerMaterial)
            float4 _MainTex_ST;
            float  _Cutoff;
            int    _Columns;
            int    _Rows;
            int    _TotalFrames;
            float  _ObjectRotation;
        CBUFFER_END

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        struct Meshdata
        {
            float4 vertex : POSITION;
            float2 uv     : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Interpolators
        {
            float4 vertex : SV_POSITION;
            float2 uv     : TEXCOORD0;
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float3 ObjSpaceViewDirURP(float3 objPos)
        {
            float3 worldPos = TransformObjectToWorld(objPos);
            float3 worldView = GetWorldSpaceViewDir(worldPos);
            return TransformWorldToObjectDir(worldView, false);
        }
        
        Interpolators BillboardVertex(Meshdata v)
        {
            Interpolators o = (Interpolators)0;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            // Find camera in local space and ignore the Y axis
            float3 lookDir = ObjSpaceViewDirURP(float3(0, 0, 0));
            lookDir.y = 0;
            lookDir = normalize(lookDir);

            // Calculate the new rotation for vertex
            float3 upDir = float3(0, 1, 0);
            float3 rightDir = cross(lookDir, upDir);
            float3 rotatedVertex = (rightDir * v.vertex.x) + (upDir * v.vertex.y);
            o.vertex = TransformObjectToHClip(rotatedVertex);

            // Calculate the angle at which the camera is looking at the object, as a normalize value [0, 1]
            float rad = atan2(lookDir.x, lookDir.z);
            float objectRad = _ObjectRotation * (PI / 180.0);
            rad -= objectRad;

            // Map the radians to a [0.0 to 1.0] circle percentage.
            float anglePercent = frac(rad / (2.0 * PI));

            // Get frameID from angle
            int frameID = (int)floor((anglePercent * _TotalFrames) + 0.5) % _TotalFrames;

            // Find Grid Coordinates
            int frameCol = frameID % _Columns;
            int frameRow = frameID / _Columns;
            int invertedRow = (_Rows - 1) - frameRow;

            // Scale and offset UVs
            float2 baseUV = TRANSFORM_TEX(v.uv, _MainTex);
            baseUV.x = (baseUV.x + frameCol) / (float)_Columns;
            baseUV.y = (baseUV.y + invertedRow) / (float)_Rows;

            o.uv = baseUV;
            return o;
        }

        half4 SampleWithClip(float2 uv)
        {
            half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
            clip(col.a - _Cutoff);
            return col;
        }
        ENDHLSL

        Pass
        {
            Name "Billboard"
            Tags { "LightMode" = "UniversalForwardOnly" }

            ZWrite On
            Cull Off
            Blend One Zero

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            Interpolators vert(Meshdata v) { return BillboardVertex(v); }

            half4 frag(Interpolators i) : SV_Target
            {
                return SampleWithClip(i.uv);
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }

            ZWrite On
            Cull Off
            ColorMask R

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            Interpolators vert(Meshdata v) { return BillboardVertex(v); }

            half4 frag(Interpolators i) : SV_Target
            {
                SampleWithClip(i.uv);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }

            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            Interpolators vert(Meshdata v) { return BillboardVertex(v); }

            half4 frag(Interpolators i) : SV_Target
            {
                SampleWithClip(i.uv);
                // Billboard always faces the camera, so the view-space normal is -forward
                float3 normalVS = float3(0, 0, 1);
                return half4(normalVS * 0.5 + 0.5, 0);
            }
            ENDHLSL
        }
    }
}