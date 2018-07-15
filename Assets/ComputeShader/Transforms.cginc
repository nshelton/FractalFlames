

const float PI = 3.14159265358979;

float3 TowardsOriginNegativeBias(float3 p)
{
    return float3(
        (p.x - 1.0) / 2.0 + 0.25, 
        (p.y - 1.0) / 2.0, 
        p.z / 2.0);
}

float3 TowardsOrigin2(float3 p)
{
    return float3(
        (p.x + 1.0) / 2.0, 
        (p.y - 1.0) / 2.0 - 0.1, 
        (p.z + 1.0) / 2.0 - 0.1);
}

float3 Swap(float3 p)
{
    return float3(
        (p.y + p.z) / 2.5, 
        (p.x + p.z) / 2.5, 
        (p.x + p.y) / 2.5);
}

float3 SwapSub(float3 p)
{
    return p.yxz - p.zyx / 2.0;
}

float3 Negate(float3 p)
{
    return -p;
}

float3 NegateSwap(float3 p)
{
    return float3(
        (-p.x + p.y + p.z) / 2.1,
        (-p.y + p.x + p.z) / 2.1,
        (-p.z + p.x + p.y) / 2.1
    );
}

float3 Up1(float3 p)
{
    return float3(p.x, p.y, p.z + 1);
}

float3 Linear(float3 p)
{
    return p;
}

float3 Sin(float3 p)
{
    return float3(
        sin(p.x), 
        sin(p.y), 
        sin(p.z));
}

float3 Spherical(float3 p)
{
    float r2 = dot(p, p);

    if ( r2 == 0.0 )
        r2 = 1.0;

    return p * (1.0 / r2);
}

float3 Polar(float3 p)
{
    return float3(
        atan2(p.y, p.x) / PI, 
        length(p), 
        atan2(p.z, p.x));
}

float3 Swirl(float3 p)
{
    float r2 = dot(p, p);
    return float3(p.z * sin(r2) - p.y * cos(r2),
                  p.x * cos(r2) + p.z * sin(r2),
                  p.x * sin(r2) - p.y * sin(r2));
}

float3 Normalize(float3 p)
{
    return normalize(p);
}

float3 Shrink(float3 p)
{   
    return normalize(p) * length(exp( -dot(p, p)));
}



float3 F1(float3 p)
{
	return p * 0.9;
}

float3 F2(float3 p)
{
	return sin( p ) * cos(3.0 * p);
}

float3 F3(float3 p)
{
	// return p / (p.x * p.x + p.y * p.y + p.z * p.z);
	return float3(
		p.x + 3.0 * sin(tan(3.0 * p.y)), 
		p.y + 3.0 * sin(tan(3.0 * p.z)), 
		p.z + 3.0 * sin(tan(3.0 * p.x)));
}

float3 F4(float3 p)
{
	float r2 = dot(p,p);

	return float3(
		p.x * sin(r2) - p.y * cos(r2),
		p.x * sin(r2) - p.z * cos(r2),
		p.z * cos(r2) - p.x * sin(r2));
}

float3 F5(float3 p)
{
	float r2 = dot(p,p);

	return float3(
		p.x * sin(r2) - p.y * cos(r2),
		p.x * sin(r2) - p.z * cos(r2),
		p.z * cos(r2) - p.x * cos(r2));
}



