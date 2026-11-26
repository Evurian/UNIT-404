// El Olvido (jefe nivel 8): tinta viva para los 7 guardianes, la masa y la forma real.
// URP 17 (Unity 6). Las letras y la disolución se proyectan en triplanar desde el
// espacio del objeto, en metros: funciona en cualquier modelo sin preparar sus UV.
//
// T_Olvido_Glifos: R = letras, G = fase de cada letra, B = ruido de disolución.
// _Dissolve 0 = modelo entero · 1 = desaparecido (lo anima TintaOlvido.cs).
Shader "UNIT404/Olvido/Tinta"
{
    Properties
    {
        [Header(Tinta)]
        _BaseColor ("Color de la tinta", Color) = (0.012, 0.012, 0.018, 1)
        _Smoothness ("Brillo (mojado)", Range(0, 1)) = 0.88
        _RimColor ("Color del contorno", Color) = (0.18, 0.2, 0.32, 1)
        _RimPower ("Contorno: finura", Range(0.5, 8)) = 3.5

        [Header(Letras)]
        [NoScaleOffset] _GlyphTex ("Glifos (R letra, G fase, B ruido)", 2D) = "black" {}
        [HDR] _GlyphColor ("Color de las letras", Color) = (3.5, 2.2, 0.9, 1)
        _GlyphSize ("Tamano de letra (m)", Float) = 0.13
        _GlyphScroll ("Deriva de las letras (m/s, mundo)", Vector) = (0, -0.04, 0, 0)
        _GlyphCycle ("Ritmo aparecer/borrar (ciclos/s)", Float) = 0.2
        _GlyphCoverage ("Letras visibles", Range(0, 1)) = 0.25

        [Header(Ondulacion)]
        _WobbleAmp ("Amplitud (m)", Float) = 0.012
        _WobbleFreq ("Frecuencia", Float) = 3
        _WobbleSpeed ("Velocidad", Float) = 1.6

        [Header(Disolucion)]
        _Dissolve ("Disolver", Range(0, 1)) = 0
        _NoiseSize ("Tamano de mancha (m)", Float) = 1.2
        _MeltFromTop ("Derretir desde arriba", Range(0, 1)) = 0.65
        _MeltHeight ("Altura del modelo (m)", Float) = 3
        _MeltBase ("Base del modelo (Y de mundo, lo pone TintaOlvido)", Float) = 0
        [HDR] _EdgeColor ("Color del borde que se derrite", Color) = (5, 3, 1.2, 1)
        _EdgeWidth ("Ancho del borde", Range(0.005, 0.3)) = 0.06
    }

    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType" = "Opaque" "Queue" = "AlphaTest" "UniversalMaterialType" = "Lit" }

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            half _Smoothness;
            half4 _RimColor;
            half _RimPower;
            half4 _GlyphColor;
            float _GlyphSize;
            float4 _GlyphScroll;
            float _GlyphCycle;
            half _GlyphCoverage;
            float _WobbleAmp;
            float _WobbleFreq;
            float _WobbleSpeed;
            half _Dissolve;
            float _NoiseSize;
            half _MeltFromTop;
            float _MeltHeight;
            float _MeltBase;
            half4 _EdgeColor;
            half _EdgeWidth;
        CBUFFER_END

        TEXTURE2D(_GlyphTex);
        SAMPLER(sampler_GlyphTex);

        struct Attributes
        {
            float4 positionOS : POSITION;
            float3 normalOS   : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings
        {
            float4 positionCS : SV_POSITION;
            float3 positionWS : TEXCOORD0;
            float3 normalWS   : TEXCOORD1;
            float3 posM       : TEXCOORD2;   // posición del objeto en metros (sin ondulación)
            float3 normalOS   : TEXCOORD3;
            float  alturaM    : TEXCOORD4;   // altura sobre la base del modelo, en metros de mundo
            half   fog        : TEXCOORD5;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        float3 EscalaObjeto()
        {
            float4x4 m = GetObjectToWorldMatrix();
            return float3(length(m._m00_m10_m20), length(m._m01_m11_m21), length(m._m02_m12_m22));
        }

        // Vértice común a todos los pases (la ondulación debe coincidir con la sombra).
        Varyings VertTinta(Attributes a)
        {
            Varyings o = (Varyings)0;
            UNITY_SETUP_INSTANCE_ID(a);
            UNITY_TRANSFER_INSTANCE_ID(a, o);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

            float3 s = EscalaObjeto();
            float3 pM = a.positionOS.xyz * s;
            float t = _Time.y;
            float ola = sin(dot(pM, float3(1.0, 1.37, 0.71)) * _WobbleFreq + t * _WobbleSpeed)
                      * sin(dot(pM, float3(-0.6, 0.9, 1.21)) * _WobbleFreq * 0.7 + t * _WobbleSpeed * 1.3);
            float3 posOS = a.positionOS.xyz + a.normalOS * (_WobbleAmp * ola) / max(s, 1e-4);

            o.positionWS = TransformObjectToWorld(posOS);
            o.normalWS = TransformObjectToWorldNormal(a.normalOS);
            o.posM = pM;
            o.normalOS = a.normalOS;
            o.alturaM = TransformObjectToWorld(a.positionOS.xyz).y - _MeltBase;
            o.positionCS = TransformWorldToHClip(o.positionWS);
            return o;
        }

        float3 PesosTri(float3 n)
        {
            float3 w = pow(abs(n), 8.0);
            return w / (w.x + w.y + w.z + 1e-5);
        }

        // Una letra: aparece y se borra según la fase guardada en G. Cada repetición de la
        // textura suma una fase al azar, así no se ve la misma cuadrícula de letras repetida.
        half Letra(float2 uv)
        {
            half4 g = SAMPLE_TEXTURE2D(_GlyphTex, sampler_GlyphTex, uv);
            float azar = frac(sin(dot(floor(uv), float2(12.9898, 78.233))) * 43758.5453);
            float pulso = 0.5 + 0.5 * sin((_Time.y * _GlyphCycle + g.g + azar * 3.7) * 6.28318);
            return g.r * smoothstep(1.0 - _GlyphCoverage, 1.2 - _GlyphCoverage, pulso);
        }

        half Letras(float3 pM, float3 w)
        {
            // la deriva se da en mundo (p. ej. hacia abajo) y se pasa al objeto
            float3 deriva = mul((float3x3)GetWorldToObjectMatrix(), _GlyphScroll.xyz) * EscalaObjeto();
            float3 p = (pM - deriva * _Time.y) / (8.0 * _GlyphSize);   // la textura tiene 8 letras por lado
            return Letra(p.zy) * w.x + Letra(p.xz) * w.y + Letra(p.xy) * w.z;
        }

        half Ruido(float3 pM, float3 w)
        {
            float3 p = pM / _NoiseSize;
            return SAMPLE_TEXTURE2D(_GlyphTex, sampler_GlyphTex, p.zy).b * w.x
                 + SAMPLE_TEXTURE2D(_GlyphTex, sampler_GlyphTex, p.xz + 0.37).b * w.y
                 + SAMPLE_TEXTURE2D(_GlyphTex, sampler_GlyphTex, p.xy + 0.71).b * w.z;
        }

        // Descarta lo ya derretido y devuelve el brillo del borde (0-1).
        half Disolver(Varyings i, float3 w)
        {
            half n = Ruido(i.posM, w);
            half arriba = saturate(i.alturaM / max(_MeltHeight, 0.01));
            half campo = saturate(lerp(n, (1.0 - arriba) + (n - 0.5) * 0.3, _MeltFromTop));
            half c = campo - (_Dissolve * 1.02 - 0.01);
            clip(c);
            return (1.0 - smoothstep(0.0, _EdgeWidth, c)) * step(0.001, _Dissolve);
        }
        ENDHLSL

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            Cull Back
            ZWrite On

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION
            #pragma multi_compile_fragment _ _SHADOWS_SOFT _SHADOWS_SOFT_LOW _SHADOWS_SOFT_MEDIUM _SHADOWS_SOFT_HIGH
            #pragma multi_compile_fragment _ _SCREEN_SPACE_OCCLUSION
            #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
            #include_with_pragmas "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Fog.hlsl"
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            Varyings vert(Attributes a)
            {
                Varyings o = VertTinta(a);
                o.fog = ComputeFogFactor(o.positionCS.z);
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                float3 w = PesosTri(normalize(i.normalOS));
                half borde = Disolver(i, w);

                float3 N = normalize(i.normalWS);
                float3 V = GetWorldSpaceNormalizeViewDir(i.positionWS);
                half contorno = pow(1.0 - saturate(dot(N, V)), _RimPower);
                half letras = Letras(i.posM, w) * (1.0 - borde);

                InputData d = (InputData)0;
                d.positionWS = i.positionWS;
                d.positionCS = i.positionCS;
                d.normalWS = N;
                d.viewDirectionWS = V;
                d.shadowCoord = TransformWorldToShadowCoord(i.positionWS);
                d.fogCoord = i.fog;
                d.bakedGI = SampleSH(N);
                d.normalizedScreenSpaceUV = GetNormalizedScreenSpaceUV(i.positionCS);
                d.shadowMask = half4(1, 1, 1, 1);

                SurfaceData s = (SurfaceData)0;
                s.albedo = _BaseColor.rgb;
                s.metallic = 0;
                s.smoothness = _Smoothness;
                s.occlusion = 1;
                s.alpha = 1;
                s.normalTS = half3(0, 0, 1);
                s.emission = letras * _GlyphColor.rgb + contorno * _RimColor.rgb + borde * _EdgeColor.rgb;

                half4 color = UniversalFragmentPBR(d, s);
                color.rgb = MixFog(color.rgb, d.fogCoord);
                color.a = 1;
                return color;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On
            ZTest LEqual
            ColorMask 0
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_vertex _ _CASTING_PUNCTUAL_LIGHT_SHADOW
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            float3 _LightDirection;
            float3 _LightPosition;

            Varyings vert(Attributes a)
            {
                Varyings o = VertTinta(a);
                #if _CASTING_PUNCTUAL_LIGHT_SHADOW
                    float3 dirLuz = normalize(_LightPosition - o.positionWS);
                #else
                    float3 dirLuz = _LightDirection;
                #endif
                o.positionCS = ApplyShadowClamping(TransformWorldToHClip(ApplyShadowBias(o.positionWS, o.normalWS, dirLuz)));
                return o;
            }

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                Disolver(i, PesosTri(normalize(i.normalOS)));
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode" = "DepthOnly" }
            ZWrite On
            ColorMask R
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex VertTinta
            #pragma fragment frag
            #pragma multi_compile_instancing

            half frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                Disolver(i, PesosTri(normalize(i.normalOS)));
                return i.positionCS.z;
            }
            ENDHLSL
        }

        Pass
        {
            Name "DepthNormals"
            Tags { "LightMode" = "DepthNormals" }
            ZWrite On
            Cull Back

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex VertTinta
            #pragma fragment frag
            #pragma multi_compile_instancing

            half4 frag(Varyings i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                Disolver(i, PesosTri(normalize(i.normalOS)));
                return half4(NormalizeNormalPerPixel(i.normalWS), 0.0);
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
