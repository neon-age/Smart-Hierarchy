// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Modified version of Internal-GUITextureClip
Shader "Hidden/Internal-IconClip"
{
    Properties {
        _MainTex ("Texture", Any) = "white" {}
    }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0

    #include "UnityCG.cginc"

    struct appdata_t {
        float4 vertex : POSITION;
        half4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        half4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float2 clipUV : TEXCOORD1;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    sampler2D _MainTex;
    sampler2D _GUIClipTexture;
    uniform bool _ManualTex2SRGB;

    uniform float4 _MainTex_ST;
    uniform half4 _Color;
    uniform half4 _OnColor;
    uniform int _IsOn;
    uniform float4x4 unity_GUIClipTextureMatrix;

    v2f vert (appdata_t v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
        float3 eyePos = UnityObjectToViewPos(v.vertex);
        o.clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));
        o.color = v.color;
        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
        return o;
    }

    half4 frag (v2f i) : SV_Target
    {
        half4 colTex = tex2D(_MainTex, i.texcoord);
        if (_ManualTex2SRGB)
            colTex.rgb = LinearToGammaSpace(colTex.rgb);

        half4 col = colTex * i.color * _Color;
        half clip = tex2D(_GUIClipTexture, i.clipUV).a;

        col.a *= clip;

        if (_IsOn == 0)
            return col;

        half left   = tex2D(_MainTex, float2(0.30, 0.50));
        half right  = tex2D(_MainTex, float2(0.70, 0.50));
        half top    = tex2D(_MainTex, float2(0.50, 0.90));
        half bottom = tex2D(_MainTex, float2(0.50, 0.10));

        // Special case for "file" icons, where background color is white and content is not in alpha channel
        if (left   > 0.8 && 
            right  > 0.8 && 
            top    > 0.8 && 
            bottom > 0.8)
            return col;
        
        col = i.color * _Color * _OnColor;
        col.a *= colTex.a * clip;
        
        return col;
    }
    ENDCG

    SubShader {
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One One
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {
        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }
}
