

uniform sampler2D _Noise;

/*float iq_noise( in float3 x )
{
	float3 p = floor(x);
	float3 f = frac(x);
	f = f*f*(3.0-2.0*f);
	
	
	float add = float2(-37.0,17.0);
	
	float2 uv = (p.xy+add*p.z) + f.xy;
	uv = float2(-uv.x, uv.y);
//	float2 rg = tex2D( _Noise, (uv+0.5)/256.0).yx;
	float2 rg = tex2D( _Noise, (uv+0.5)/256.0).yx;
//	float2 rg = tex2D( _Noise, uv +  / 256.0).yx;
	return lerp( rg.x, rg.y, f.z );
}

*/

float hash( float n )
{
    return frac(sin(n)*43758.5453);
}
 
float iq_noise( float3 x )
{
    // The noise function returns a value in the range -1.0f -> 1.0f
 
    float3 p = floor(x);
    float3 f = frac(x);
 
    f       = f*f*(3.0-2.0*f);
    float n = p.x + p.y*57.0 + 113.0*p.z;
 
    return lerp(lerp(lerp( hash(n+0.0), hash(n+1.0),f.x),
                   lerp( hash(n+57.0), hash(n+58.0),f.x),f.y),
               lerp(lerp( hash(n+113.0), hash(n+114.0),f.x),
                   lerp( hash(n+170.0), hash(n+171.0),f.x),f.y),f.z);
}
 