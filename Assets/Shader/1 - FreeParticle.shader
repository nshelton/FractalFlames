// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "XParticle/1 - FreeParticle"
{
	Properties
	{
		_ColorLow ("Color Slow Speed", Color) = (0, 0, 0.5, 0.3)
		_ColorHigh ("Color High Speed", Color) = (1, 0, 0, 0.3)
		_HighSpeedValue ("High speed Value", Range(0, 50)) = 25
	}

	SubShader 
	{
		Pass 
		{
			Blend SrcAlpha DstAlpha

			CGPROGRAM
			#pragma target 5.0
			
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			
			// Particle's data
			struct Particle
			{
				float3 position;
				float3 velocity;
			};
			
			// Pixel shader input
			struct PS_INPUT
			{
				float4 position : SV_POSITION;
				float4 color : COLOR;
			};
			
			// Particle's data, shared with the compute shader
			StructuredBuffer<Particle> particleBuffer;
			
			// Properties variables
			uniform float4 _ColorLow;
			uniform float4 _ColorHigh;
			uniform float _HighSpeedValue;
			
			// Vertex shader
			PS_INPUT vert(uint vertex_id : SV_VertexID, uint instance_id : SV_InstanceID)
			{
				PS_INPUT o = (PS_INPUT)0;

				// Color
				float speed = (particleBuffer[instance_id].velocity).x;
				float lerpValue = frac((float)instance_id / 1000000.0);
				o.color = lerp(_ColorLow, _ColorHigh, lerpValue);

				// Position
				o.position = UnityObjectToClipPos(float4(particleBuffer[instance_id].position, 1.0f));

				return o;
			}

			// Pixel shader
			float4 frag(PS_INPUT i) : COLOR
			{
				return i.color;
			}
			
			ENDCG
		}
	}

	Fallback Off
}
