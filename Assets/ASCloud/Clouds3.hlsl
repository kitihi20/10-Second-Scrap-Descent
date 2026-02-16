// 3D擬似乱数
float hash(float3 p) {
    return frac(sin(dot(p, half3(12.9898, 48.233,31.5233)))*43758.5453);
}

// 3Dノイズ (補間付き)
float LerpNoise(float3 x) {
    float3 i = floor(x);
    float3 f = frac(x);
    f = f * f * (3.0 - 2.0 * f);
    return lerp(lerp(lerp(hash(i + float3(0, 0, 0)), hash(i + float3(1, 0, 0)), f.x),
                   lerp(hash(i + float3(0, 1, 0)), hash(i + float3(1, 1, 0)), f.x), f.y),
               lerp(lerp(hash(i + float3(0, 0, 1)), hash(i + float3(1, 0, 1)), f.x),
                   lerp(hash(i + float3(0, 1, 1)), hash(i + float3(1, 1, 1)), f.x), f.y), f.z);
}

float ValueNoise3D_float(float3 Position, float Scale) {
    // スケールを適用
    float3 p = Position * Scale;
    
    // 格子点の特定（整数部と小数部）
    float3 i = floor(p);
    float3 f = frac(p);

    // スムーズな補間のためのS字カーブ (Quintic interpolation)
    // これを適用することでバイリニア/トリリニア補間が滑らかになります
    float3 u = f * f * (3.0 - 2.0 * f);

    // 立方体の8つの角のハッシュ値を取得
    float v000 = hash(i + float3(0, 0, 0));
    float v100 = hash(i + float3(1, 0, 0));
    float v010 = hash(i + float3(0, 1, 0));
    float v110 = hash(i + float3(1, 1, 0));
    float v001 = hash(i + float3(0, 0, 1));
    float v101 = hash(i + float3(1, 0, 1));
    float v011 = hash(i + float3(0, 1, 1));
    float v111 = hash(i + float3(1, 1, 1));

    // X, Y, Z軸に沿ってトリリニア補間を実行
    return lerp(
        lerp(lerp(v000, v100, u.x), lerp(v010, v110, u.x), u.y),
        lerp(lerp(v001, v101, u.x), lerp(v011, v111, u.x), u.y),
        u.z
    );
}






float3 hash33(float3 p) {
    return frac(sin(dot(p, float3(12.9898, 78.233, 45.164))) * float3(43758.5453, 28001.83, 50249.12));
}


/*
uint hash(uint x) {
    x = ((x >> 16) ^ x) * 0x45d9f3b;
    x = ((x >> 16) ^ x) * 0x45d9f3b;
    x = (x >> 16) ^ x;
    return x;
}
float3 GetRandomFloat3Area(float seed, float3 minRange, float3 maxRange) {
    // floatのビット表現をuintとして解釈
    uint s = asuint(seed);

    // 3つの異なるハッシュ値を生成（x, y, z用）
    uint hx = hash(s);
    uint hy = hash(hx);
    uint hz = hash(hy);

    // [0, 1] の範囲のfloatに変換 (0xffffffff = 4294967295)
    float3 t = float3(hx, hy, hz) / 4294967295.0;

    // 指定された範囲（minRange ～ maxRange）に線形補間
    return lerp(minRange, maxRange, t);
}
*/

//特定座標、法線方向の無限平面の表面までの距離を求める
inline float GetIntersectLength(float3 rayPos, float3 rayDir, float3 planePos, float3 planeNormal)
{
    return dot(planePos - rayPos, planeNormal) / dot(rayDir, planeNormal);
}


// 雲発生高度上限/下限までレイを近づけておく
// 発生高度範囲内ならレイを進めない
float getFirstDistance(float3 pos, float3 dir, float lower, float higher)
{
    float dis = GetIntersectLength(pos, dir, float3(0,lower,0), float3(0,-1,0));
    dis = min(dis, GetIntersectLength(pos, dir, float3(0,higher,0), float3(0,1,0)));
    dis = clamp(dis, 0 , 100000000);//多分オーバーフロー対策、無いとバグる
    //dis = (pos.y >= lower && pos.y <= higher) ? 0 : dis;
    //dis *= 1 - step(lower, pos.y) - step(higher, pos.y);
    dis *= 1 - step(lower,pos.y) * step(pos.y, higher);
    
    return dis;
}

// 密度計算 (fBm)
float getDensity(float3 p, float lower, float higher, float threshold) {
    float3 q = p*0.001;
    float f = 0;
    f += 0.6000 * LerpNoise(q); q = q * 2.02;
    f += 0.2500 * LerpNoise(q); q = q * 4.03;
    f += 0.1250 * LerpNoise(q); q = q * 4.04;

    //f *= LerpNoise(p*0.003);
    
    // 高度制限
    float cloudheight = higher - lower;
    float heightMask = smoothstep(lower, lower+cloudheight*0.1, p.y) * smoothstep(higher, higher-cloudheight*0.4, p.y);
    return saturate((f-threshold) * heightMask); 
}

void RaymarchClouds_float(
    float3 CamPos,       // カメラのワールド座標
    float3 CamForward,   // カメラの前方向ベクトル
    float3 CamUp,        // カメラの上方向ベクトル
    float3 CamRight,     // カメラの右方向ベクトル
    float CamDepth,
    float2 ScreenPos,    // スクリーン座標 (0〜1 UV)
    float TanHalfFOV,    // 視野角の計算用 (tan(FOV/2))
    float AspectRatio,   // アスペクト比
    float Depth,
    float3 LightDir,     // Directional Lightの向き
    float3 LightColor,   // Directional Lightの色
    float3 AmbientColor, // 環境光の色
    float LowerHeight,   // 雲発生高度下限
    float HigherHeight,  // 雲発生高度上限
    float CloudThreshold,// 雲の発生範囲?
    float StepSize,      // レイのステップ幅
    float LowDensityStepSize,// 周囲に雲が無いときの最大ステップ幅
    int MaxSteps,        // 最大ステップ数
    float DensityMult,   // 密度の強さ
    out float4 OutColor // 最終的な色
) {
    // 1. スクリーン座標とカメラベクトルからレイの方向を計算
    float2 uv = ScreenPos * 2.0 - 1.0; // -1 ～ 1 に変換
    float3 rayDir = normalize(CamForward + 
                             uv.x * CamRight * TanHalfFOV * AspectRatio + 
                             uv.y * CamUp * TanHalfFOV);
    //rayDir += hash33(float3(uv.x, uv.y, _Time.x)) * 0.004;//Jitter
    float3 startPos = CamPos + float3(_Time.x * 300,0,0);
    float transmittance = 1.0;
    float3 scatteredLight = 0.0;
    float sumdist = 0;

    float FarPlane = CamDepth;
    float realDepth = Depth*CamDepth;
    
    //sumdist += 10 * hash(startPos);
    sumdist += getFirstDistance(startPos, rayDir, LowerHeight, HigherHeight);
    //sumdist += (StepSize + ((1-saturate(getDensity(startPos, LowerHeight, HigherHeight, CloudThreshold)))*LowDensityStepSize));

    [loop]
    for (int i = 0; i < MaxSteps; i++) {

        float3 p = startPos + rayDir * sumdist;
        float d = getDensity(p, LowerHeight, HigherHeight, CloudThreshold);
        
        if (d > 0.01) {

            if (transmittance < 0.01 || (realDepth <= sumdist && realDepth < FarPlane)) break;

            float densitySample = d * DensityMult;
            
            // 光の方向に少し進んで密度を再サンプリング (ボリュームシャドウ)
            float shadowStep = 50;
            float dLight;
            float dLight1 = getDensity(p - LightDir * shadowStep, LowerHeight, HigherHeight, CloudThreshold);
            float dLight2 = getDensity(p - LightDir * shadowStep*2, LowerHeight, HigherHeight, CloudThreshold);
            dLight = (dLight1 + dLight2*0.5)/1.5;
            //dLight = pow(dLight,3);
            dLight *= 4;
            //dLight = 1-saturate(dLight);
            //dLight = (dLight-d) < 0.001 ? 0 : 1;
            //dLight = (dLight-d)*10;
            //dLight *= 2;
            //dLight = 0;// 0: 明るい 1: 暗い
            float shadowTransmittance = exp(-dLight * 2.0); // 影の強さ

            // Beerの法則による透過率の減衰
            float currentTransmittance = exp(-densitySample * StepSize);
            
            // 環境光 + 直接光(影あり)
            float3 localLight = AmbientColor + LightColor * shadowTransmittance;
            
            // 散乱光の蓄積
            scatteredLight += localLight * (transmittance * (1.0 - currentTransmittance));
            
            // 全体透過率の更新
            transmittance *= currentTransmittance;
            
            //if (transmittance < 0.01 || (realDepth <= sumdist && realDepth < FarPlane)) break;

            sumdist += StepSize + (i*2) + (pow(1-saturate(d),3)*100);
        }else
        {
            sumdist += StepSize + (i*4) + ((1-saturate(d))*LowDensityStepSize);
        }


        //sumdist += StepSize + (i*0.1f) + ((1-saturate(d))*LowDensityStepSize);
        //sumdist += StepSize;
    }
    
    OutColor = float4(scatteredLight, 1.0 - transmittance);
}


void RaymarchClouds_Acc_float(
    float FrameRatio,
    float PixelIndex,
    float2 ScreenSize,
    float4 BeforeColor,
    float3 CamPos,       // カメラのワールド座標
    float3 CamForward,   // カメラの前方向ベクトル
    float3 CamUp,        // カメラの上方向ベクトル
    float3 CamRight,     // カメラの右方向ベクトル
    float CamDepth,
    float2 ScreenPos,    // スクリーン座標 (0〜1 UV)
    float TanHalfFOV,    // 視野角の計算用 (tan(FOV/2))
    float AspectRatio,   // アスペクト比
    float Depth,
    float3 LightDir,     // Directional Lightの向き
    float3 LightColor,   // Directional Lightの色
    float3 AmbientColor, // 環境光の色
    float LowerHeight,   // 雲発生高度下限
    float HigherHeight,  // 雲発生高度上限
    float CloudThreshold,// 雲の発生範囲?
    float StepSize,      // レイのステップ幅
    float LowDensityStepSize,// 周囲に雲が無いときの最大ステップ幅
    int MaxSteps,        // 最大ステップ数
    float DensityMult,   // 密度の強さ
    out float4 OutColor
)
{
    int screenPixel_x = ScreenPos.x*ScreenSize.x;
    int screenPixel_y = ScreenPos.y*ScreenSize.y;
    float delayFrame = 4;
    float frame = FrameRatio * delayFrame;

    if(screenPixel_x < 1 && screenPixel_y < 1)
    {
        frame += 1;
        frame = (frame >= delayFrame) ? 0 : frame;
        //frame = fmod(frame + 1, 4);
        OutColor = float4(frame/delayFrame, 1, 1, 1);
    }
    else
    {
        float4 o_color;

        if(abs(frame - fmod(screenPixel_x+fmod(screenPixel_y,delayFrame/2)*delayFrame/2, delayFrame)) < 0.1)
        {
            RaymarchClouds_float(
                CamPos,CamForward,CamUp,CamRight,CamDepth,
                ScreenPos,TanHalfFOV,AspectRatio,
                Depth,
                LightDir,LightColor,AmbientColor,
                LowerHeight,HigherHeight,CloudThreshold,
                StepSize,LowDensityStepSize,MaxSteps,DensityMult,
                o_color
            );

            OutColor = o_color;
            //OutColor = lerp(BeforeColor, o_color, 0.6);
            
            /*OutColor = float4(0,0,0,1);
            if(abs(frame) < 0.1){ OutColor = float4(1,0,0,1); }
            if(abs(frame-1) < 0.1){ OutColor = float4(0,1,0,1); }
            if(abs(frame-2) < 0.1){ OutColor = float4(0,0,1,1); }
            if(abs(frame-3) < 0.1){ OutColor = float4(1,1,1,1); }*/
        }else
        {
            OutColor = BeforeColor;
            //OutColor = lerp(BeforeColor, float4(BeforeColor.rgb, 0), 0.01);
        }
    }
}



void RaymarchClouds_Acc2_float(
    float FrameRatio,
    float PixelIndex,
    float2 ScreenSize,
    float4 BeforeColor,
    float3 CamPos,       // カメラのワールド座標
    float3 CamForward,   // カメラの前方向ベクトル
    float3 CamUp,        // カメラの上方向ベクトル
    float3 CamRight,     // カメラの右方向ベクトル
    float CamDepth,
    float2 ScreenPos,    // スクリーン座標 (0〜1 UV)
    float TanHalfFOV,    // 視野角の計算用 (tan(FOV/2))
    float AspectRatio,   // アスペクト比
    float Depth,
    float3 LightDir,     // Directional Lightの向き
    float3 LightColor,   // Directional Lightの色
    float3 AmbientColor, // 環境光の色
    float LowerHeight,   // 雲発生高度下限
    float HigherHeight,  // 雲発生高度上限
    float CloudThreshold,// 雲の発生範囲?
    float StepSize,      // レイのステップ幅
    float LowDensityStepSize,// 周囲に雲が無いときの最大ステップ幅
    int MaxSteps,        // 最大ステップ数
    float DensityMult,   // 密度の強さ
    out float4 OutColor
)
{

    float4 o_color;
    
    RaymarchClouds_float(
        CamPos,CamForward,CamUp,CamRight,CamDepth,
        ScreenPos,TanHalfFOV,AspectRatio,
        Depth,
        LightDir,LightColor,AmbientColor,
        LowerHeight,HigherHeight,CloudThreshold,
        StepSize,LowDensityStepSize,MaxSteps,DensityMult,
        o_color
    );

    OutColor = o_color;

}