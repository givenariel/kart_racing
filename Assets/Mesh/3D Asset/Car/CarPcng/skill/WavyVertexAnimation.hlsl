#ifndef WAVY_OBJECT_URP_INCLUDED
#define WAVY_OBJECT_URP_INCLUDED

// **Custom Function untuk Vertex Animation (Gelombang)**
void WavyVertexAnimation_float(
    float3 PositionOS, float3 NormalOS, float2 UV,
    float Freq, float Speed, float Amp, float Detail,
    out float3 OutPositionOS, out float3 OutNormalWS, out float2 OutUV)
{
    // Waktu animasi
    float time = _Time.y * Speed;

    // Transform normal ke world space
    float3 normalWS = normalize(mul((float3x3) UNITY_MATRIX_M, NormalOS));

    // Hitung fase gelombang
    float wavePhase = (PositionOS.x + PositionOS.z) * Freq + time;

    // Gelombang berbasis sin dan cos
    float wave1 = sin(wavePhase) * Amp * 0.5;
    float wave2 = cos(wavePhase * 1.1) * Amp * 0.3;
    float wave3 = sin(wavePhase * 0.8) * Amp * 0.2;

    // Gelombang dengan detail tambahan
    float waveDetail = sin(wavePhase * Detail) * Amp * 0.1;
    
    // Total gelombang
    float waveHeight = wave1 + wave2 + wave3 + waveDetail;

    // Pastikan vertex hanya bergerak sesuai normalnya
    OutPositionOS = PositionOS + normalWS * waveHeight;
    OutNormalWS = normalWS; // Simpan normal yang sudah diubah
    OutUV = UV;
}

#endif // WAVY_OBJECT_URP_INCLUDED
