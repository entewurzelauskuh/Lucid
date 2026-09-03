// The mist that fills a fog door (docs/SPEC.md §7, docs/UI.md §1).
//
// Noise is generated rather than sampled: a texture would be an asset, and
// CLAUDE.md rule 5 asks that every committed asset carry a licence line. A
// hash-based value noise costs a few instructions and nothing else.
//
// The four states must not differ by hue alone (docs/UI.md §1: "Fog is dark
// and matte, Exit is bright and radiant"), so the parameters that separate
// them are density, brightness and how fast the layers drift, not just colour.
Shader "Lucid/Mist"
{
    Properties
    {
        _Tint          ("Tint", Color) = (0.55, 0.58, 0.65, 1)
        _Brightness    ("Brightness", Range(0, 4)) = 0.6
        _Density       ("Density", Range(0, 1)) = 0.85
        _Scale         ("Noise scale", Range(0.1, 8)) = 1.6
        _Drift         ("Drift speed", Range(0, 2)) = 0.12
        _EdgeSoftness  ("Edge softness", Range(0.001, 0.5)) = 0.18
        _Dissolve      ("Dissolve", Range(0, 1)) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Transparent"
            "Queue"          = "Transparent"
        }

        Pass
        {
            Name "Mist"
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; float2 uv : TEXCOORD0; };
            struct Varyings   { float4 positionHCS : SV_POSITION; float2 uv : TEXCOORD0; };

            CBUFFER_START(UnityPerMaterial)
                float4 _Tint;
                float  _Brightness;
                float  _Density;
                float  _Scale;
                float  _Drift;
                float  _EdgeSoftness;
                float  _Dissolve;
            CBUFFER_END

            // The frac/dot hash in wide circulation from Inigo Quilez's
            // articles and countless Shadertoy derivatives. Named because this
            // repository names what it did not invent, not because two lines
            // of arithmetic carry a licence.
            float Hash(float2 p)
            {
                p = frac(p * float2(123.34, 456.21));
                p += dot(p, p + 45.32);
                return frac(p.x * p.y);
            }

            // Value noise: smooth interpolation between hashed lattice corners.
            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                f = f * f * (3.0 - 2.0 * f);
                float a = Hash(i);
                float b = Hash(i + float2(1, 0));
                float c = Hash(i + float2(0, 1));
                float d = Hash(i + float2(1, 1));
                return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
            }

            // Three layers drifting at different speeds and angles. One layer
            // reads as a moving texture; three read as depth.
            float Mist(float2 uv, float t)
            {
                float n = 0.0;
                n += 0.55 * ValueNoise(uv * _Scale        + float2( 0.10,  1.00) * t * _Drift);
                n += 0.30 * ValueNoise(uv * _Scale * 2.1  + float2(-0.35,  0.62) * t * _Drift);
                n += 0.15 * ValueNoise(uv * _Scale * 4.3  + float2( 0.60, -0.28) * t * _Drift);
                return n;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float t = _Time.y;
                float n = Mist(IN.uv, t);

                // Feathered at the frame, so the mist meets the doorway rather
                // than ending at a straight cut.
                float2 d = min(IN.uv, 1.0 - IN.uv);
                float edge = smoothstep(0.0, _EdgeSoftness, min(d.x, d.y));

                // Dissolve eats the thinnest parts of the noise first, so a
                // door clears in wisps instead of fading uniformly.
                float alpha = saturate(n * _Density) * edge;
                alpha *= saturate((n - _Dissolve) / max(1e-3, 1.0 - _Dissolve));

                half3 colour = _Tint.rgb * _Brightness * (0.65 + 0.35 * n);
                return half4(colour, alpha * _Tint.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
