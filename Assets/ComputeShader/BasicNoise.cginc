

float3 random3(int id, float3 pos)
{

	float2 uv = float2((float)(id % 4096) , id / 4096.0);

	uv /=  float2(4096.0, NumParticles / 4096.0 + 1 );

	return frac( pos + NoiseTex.SampleLevel(_LinearClamp, uv, 0).rgb );
}
