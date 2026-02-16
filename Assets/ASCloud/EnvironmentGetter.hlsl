
//#include "Packages/com. unity.render-pipelines. universal/ShaderLibrary/UnityInput.hlsl"

void GetMainLight_float(out float3 dir, out float3 col) 
{
#if defined(SHADERGRAPH_PREVIEW)
	dir = float3(0, -1, 0);
	col = float(1, 0.96, 0.84);
#else
	Light mainLight = GetMainLight(0);
	dir = -mainLight.direction;
	col = mainLight.color;
#endif
}

/*
void GetSkyCube_float(out float4 color) 
{
    color = unity_SpecCube0_HDR;
}
*/
/*
void GetSkyColor_float(float3 dir, float lod, out float3 color) 
{
	color = DecodeHDREnvironment(SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, dir, lod), unity_SpecCube0_HDR);
	
	//half4 envCol = SAMPLE_TEXTURECUBE(unity_SpecCube0, samplerunity_SpecCube0, dir);
    //color = DecodeHDREnvironment(envCol, unity_SpecCube0_HDR);
}*/