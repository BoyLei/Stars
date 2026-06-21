TEXTURE2D(_CausticsTex);
SAMPLER(sampler_CausticsTex);

half3 SampleCaustics(float2 uv, float2 time, float tiling)
{
	half3 caustics1 = SAMPLE_TEXTURE2D(_CausticsTex, sampler_CausticsTex, uv * tiling + (time.xy)).rgb;
	half3 caustics2 = SAMPLE_TEXTURE2D(_CausticsTex, sampler_CausticsTex, (uv * tiling * 0.8) - (time.xy)).rgb;

	#if UNITY_COLORSPACE_GAMMA
	caustics1 = SRGBToLinear(caustics1);
	caustics2 = SRGBToLinear(caustics2);
	#endif

	half3 caustics = min(caustics1, caustics2);
	
	#if WAVE_SIMULATION
	SampleWaveCaustics(float4(uv.x, 0, uv.y), caustics);
	#endif
	
	return caustics;
}