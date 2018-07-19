

Shader "Custom/Billboard" 
{
	Properties 
	{
		[HDR] _Color ("color", Color) = (1,1,1,1)
		_Size ("Size", float) = 0.5
		_ColorScale ("ColorScale", float) = 1.0
		_Palette ("Palette", float) = 1.0
	}

	SubShader 
	{
		Pass
		{
			Tags { "RenderType"="Transparent" }
			Blend SrcAlpha One // Additive
		    Ztest Always
			LOD 200
		
			CGPROGRAM
				#pragma target 5.0
				#pragma vertex VS_Main
				#pragma fragment FS_Main
				#pragma geometry GS_Main
				#include "UnityCG.cginc" 

				struct pointData
				{
					float4	pos		: POSITION;
					float3	normal	: NORMAL;
					float2  tex0	: TEXCOORD0;
					float4  color : COLOR;

				};

				struct Particle
				{
					float3 position;
					float3 velocity;
				};

				float _Palette;
				float _Size;
				float _ColorScale;
				sampler2D _SpriteTex;
				float4 _Color;
				float4x4 modelToWorld;
				StructuredBuffer<Particle> particleBuffer;

				float nrand(float2 uv, float salt)
				{
					uv += float2(salt, 1);
					return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
				}

				float3 pal( in float t, in float3 a, in float3 b, in float3 c, in float3 d )
				{
					return a + b*cos(_Time.y+6.28318*(c*t+d) );
				}


				// Vertex Shader ------------------------------------------------
   				pointData VS_Main(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
				{
					pointData output = (pointData)0;

					float4 particlePos = float4(particleBuffer[instance_id].position, 1.0f);
					output.pos =  mul(modelToWorld, particlePos);

					float colormap = particleBuffer[instance_id].velocity.y / _ColorScale;
					float colormapSig = exp(colormap) /(exp(colormap) + 1);

					float3 col ;
					
					if (_Palette < 0.1)
					{
						col= pal(colormapSig, float3(0.5,0.5,0.5),float3(0.5,0.5,0.5),float3(1.0,1.0,1.0),float3(0.0,0.33,0.67) );
					}
					else if ( _Palette < 0.2)
					{
						col = pal( colormapSig, float3(0.5,0.5,0.5),float3(0.5,0.5,0.5),float3(1.0,1.0,1.0),float3(0.0,0.10,0.20) );
					}
					else if ( _Palette < 0.3)
					{
						col = pal( colormapSig, float3(0.5,0.5,0.5),float3(0.5,0.5,0.5),float3(1.0,1.0,1.0),float3(0.3,0.20,0.20) );
					}
					else if ( _Palette < 0.4)
					{
						col = pal( colormapSig, float3(0.5,0.5,0.5),float3(0.5,0.5,0.5),float3(1.0,1.0,0.5),float3(0.8,0.90,0.30) );
					}
					else if ( _Palette < 0.5)
					{
						col = pal( colormapSig, float3(0.5,0.5,0.5),float3(0.5,0.5,0.5),float3(1.0,0.7,0.4),float3(0.0,0.15,0.20) );
					}
					else if ( _Palette < 0.6)
					{
						col = pal( colormapSig, float3(0.5,0.5,0.5),float3(0.5,0.5,0.5),float3(2.0,1.0,0.0),float3(0.5,0.20,0.25) );
					}
					else
					{
						col = pal( colormapSig, float3(0.8,0.5,0.4),float3(0.2,0.4,0.2),float3(2.0,1.0,1.0),float3(0.0,0.25,0.25) );
					}


 					output.color = saturate(float4(col, 1.0) * _Color);

					return output;
				}


				float compute_depth(float4 clippos)
				{
					#if defined(SHADER_TARGET_GLSL) || defined(SHADER_API_GLES) || defined(SHADER_API_GLES3)
						return ((clippos.z / clippos.w) + 1.0) * 0.5;
					#else
						return clippos.z / clippos.w;
					#endif
				}

				// Geometry Shader -----------------------------------------------------
				[maxvertexcount(4)]
				void GS_Main(point pointData p[1], inout TriangleStream<pointData> triStream)
				{
					float4 v[4];

					float halfS = 0.5f * _Size * 0.001  * length(p[0].pos - _WorldSpaceCameraPos);

					float3 delta = float3(halfS, -halfS, 0);

					// float2 aspect = float2(_ScreenParams.x/_ScreenParams.y, 1.0);

					p[0].pos = UnityObjectToClipPos(p[0]. pos);

					v[3] = p[0].pos + float4(halfS,-halfS,0,0);
					v[1] = p[0].pos + float4(-halfS,-halfS,0,0);
					v[0] = p[0].pos + float4(-halfS,halfS,0,0);
					v[2] = p[0].pos + float4(halfS,halfS,0,0);

					pointData pIn;
					pIn.color  = p[0].color;
					pIn.normal = p[0].normal;

					pIn.pos = v[0];
					pIn.tex0 = float2(1.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = v[1];
					pIn.tex0 = float2(1.0f, 1.0f);
					triStream.Append(pIn);

					pIn.pos = v[2];
					pIn.tex0 = float2(0.0f, 0.0f);
					triStream.Append(pIn);

					pIn.pos = v[3];
					pIn.tex0 = float2(0.0f, 1.0f);
					triStream.Append(pIn);
				}

			fixed4 FS_Main (pointData i) : SV_Target
			{
				float alpha =  clamp(0.5 - length(i.tex0.xy - 0.5),0,1);

				return float4(i.color.rgb, i.color.a * alpha);
			}

			ENDCG
		}
	} 
}
