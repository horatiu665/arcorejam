Shader "Outlined/VRTweakTool_Outline"
{
    Properties
    {
        _OutlineColor ("Outline Color", Color) = (0, 0, 0, 1)
        _Outline ("Outline width", Float) = .5
        _OutlineNNFF ("Near dist, width; Far dist,width", Vector) = (0.1, 0.005, 1000, 0.01)

    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
    };

    struct v2f
    {
        float4 pos : POSITION;
        float4 color : COLOR;
    };

    uniform float _Outline;
    uniform float4 _OutlineColor;
    //uniform float _LogDiv;
    uniform float4 _OutlineNNFF;

    // a smooth clamped inverse lerp. maps a value between a and b to 0 and 1, but clamps between 0 and 1 and then smoothens at the ends with the undocumented smoothstep shader function.
    float inverselerp(float a, float b, float value)
    {
        return smoothstep(0, 1, clamp((value - a) / (b - a), 0, 1));
    }

    v2f vert(appdata v)
    {  
        // just make a copy of incoming vertex data but scaled according to normal direction
        v2f o;
        //o.pos = UnityObjectToClipPos(v.vertex);

        float3 camPos = _WorldSpaceCameraPos;
        float3 worldPos = mul(unity_ObjectToWorld, v.vertex);
        float dist = distance(camPos, worldPos);
        //float logDist = log2(dist / _LogDiv);
        //float3 norm = mul((float3x3)UNITY_MATRIX_IT_MV, v.normal);
        //float2 offset = TransformViewToProjection(norm.xy);

        // simplest case is a constant local space offset, output.pos = v.vertex + v.normal * _Outline. with an optimal scale when close-up, this is not very visible when far away.
        // an improvement is a constant world space offset, normal * _Outline * dist. Increases the normal magnitude with the same rate as the distance to the camera. With an optimal scale when close-up, this looks very thick when far away, cause it's a bit too unnaturally big when the obj is super tiny.
        // now we know: what we want is a multiplier function that is approx. 1 when close, but approaches zero when moving far away. So we can do some lerping based on distance and some parameters, and we can have this smooth lerped parameter based on distance, to multiply the normal lenght with.
        float distMultiplier = lerp(_OutlineNNFF.y, _OutlineNNFF.w, inverselerp(_OutlineNNFF.x, _OutlineNNFF.z, dist));
        float3 localOffset = v.normal * _Outline * distMultiplier * dist;
        o.pos = UnityObjectToClipPos(v.vertex + localOffset);

        //o.pos.xy += normalize(offset) * o.pos.z * _Outline * logDist;
        o.color = _OutlineColor;
        return o;
    }
    ENDCG

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
        }

        Pass
        {
            Name "BASE"
            Cull Back
            Blend Zero One
            // uncomment this to hide inner details:

            //Offset -8, -8
            SetTexture [_OutlineColor]
            { 
                constantColor (0, 0, 0, 0)
                combine constant
            }
        }

        // note that a vertex shader is specified here but its using the one above
        Pass
        {
            Name "OUTLINE"
            Tags
            {
                "LightMode" = "Always"
            }
            Cull Front

            // you can choose what kind of blending mode you want for the outline
            Blend SrcAlpha OneMinusSrcAlpha
            // Normal

            //Blend One One // Additive
            //Blend One OneMinusDstColor // Soft Additive
            //Blend DstColor Zero // Multiplicative
            //Blend DstColor SrcColor // 2x Multiplicative
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            half4 frag(v2f i) : COLOR
            {
                return i.color;
            }
            ENDCG
        }

    }

    FallBack "Diffuse"
}