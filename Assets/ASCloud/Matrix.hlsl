void GetCameraVectors_float(float4x4 ViewMatrix, out float3 Forward, out float3 Right, out float3 Up)
{
    // UnityのViewMatrix (UNITY_MATRIX_V) は World-to-View 変換行列です。
    // この行列の各行（Row）は、ワールド空間におけるカメラの基底ベクトルを表します。
    Right = -float3(ViewMatrix[0][0], ViewMatrix[0][1], ViewMatrix[0][2]);
    Up = -float3(ViewMatrix[1][0], ViewMatrix[1][1], ViewMatrix[1][2]);
    Forward = -float3(ViewMatrix[2][0], ViewMatrix[2][1], ViewMatrix[2][2]);

    //Right = -transpose(UNITY_MATRIX_V)[0].xyz;
    //Up = -transpose(UNITY_MATRIX_V)[1].xyz;
    //Forward = -transpose(UNITY_MATRIX_V)[2].xyz;
}


/*void GetCameraVectors2_float(out float3 Forward, out float3 Right, out float3 Up)
{
    float4x4 viewMat = GetWorldToViewMatrix();
    Right = -viewMat[0].xyz;
    Up = -viewMat[1].xyz;
    Forward = -viewMat[2].xyz;
}*/
void GetCameraVectors2_float(out float3 Forward, out float3 Right, out float3 Up)
{
    Right = -UNITY_MATRIX_V[0].xyz;
    Up = -UNITY_MATRIX_V[1].xyz;
    Forward = -UNITY_MATRIX_V[2].xyz;
}
/*void GetCameraVectors2WebGL_float(out float3 Forward, out float3 Right, out float3 Up)
{
    float4x4 viewMat = GetWorldToViewMatrix();
    Right = viewMat[0].xyz;
    Up = viewMat[1].xyz;
    Forward = -viewMat[2].xyz;
}*/
void GetCameraVectors2WebGL_float(out float3 Forward, out float3 Right, out float3 Up)
{
    Right = UNITY_MATRIX_V[0].xyz;
    Up = UNITY_MATRIX_V[1].xyz;
    Forward = -UNITY_MATRIX_V[2].xyz;
}
/*void GetCameraVectors2_float(out float3 Forward, out float3 Right, out float3 Up)
{
    #pragma only_renderers gles3 glcore

    float4x4 viewMat = GetWorldToViewMatrix();
    Right = viewMat[0].xyz;
    Up = viewMat[1].xyz;
    Forward = -viewMat[2].xyz;
}*/

void GetFOVFromProjection_float(float4x4 ProjMatrix, out float FOV)
{
    // ProjMatrix[1][1] は 1.0 / tan(fov_radian / 2.0)
    // HLSLの行列アクセスは [row][col]
    float cotHalfFov = ProjMatrix[1][1];
    
    // 逆三角関数を用いてラジアン単位のFOVを算出
    float fovRad = 2.0 * atan(1.0 / cotHalfFov);
    
    // 度数法（Degree）に変換して出力
    FOV = degrees(fovRad);
}


void Inverse_float(float4x4 m, out float4x4 im)
{
	float4 det1 = mad(m._33_31_13_11, m._44_42_24_22, -m._34_32_14_12 * m._43_41_23_21);
	float4 det2 = mad(m._23_21_13_11, m._44_42_34_32, -m._24_22_14_12 * m._43_41_33_31);
	float4 det3 = mad(m._23_21_13_11, m._34_32_44_42, -m._24_22_14_12 * m._33_31_43_41);
	
	im._11_21_31_41 = mad(m._22_21_24_23, det1.xxyy, mad(-m._32_31_34_33, det2.xxyy, m._42_41_44_43 * det3.xxyy));
	im._12_22_32_42 = mad(m._12_11_14_13, det1.xxyy, mad(-m._32_31_34_33, det3.zzww, m._42_41_44_43 * det2.zzww));
	im._13_23_33_43 = mad(m._12_11_14_13, det2.xxyy, mad(-m._22_21_24_23, det3.zzww, m._42_41_44_43 * det1.zzww));
	im._14_24_34_44 = mad(m._12_11_14_13, det3.xxyy, mad(-m._22_21_24_23, det2.zzww, m._32_31_34_33 * det1.zzww));
	
	im._21_41 = -im._21_41;
	im._12_32 = -im._12_32;
	im._23_43 = -im._23_43;
	im._14_34 = -im._14_34;
	
	float invDet = rcp(dot(m[0], im._11_21_31_41));
	im._11_21_31_41 *= invDet;
	im._12_22_32_42 *= invDet;
	im._13_23_33_43 *= invDet;
	im._14_24_34_44 *= invDet;
}

