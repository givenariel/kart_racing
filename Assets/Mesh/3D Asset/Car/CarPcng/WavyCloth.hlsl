#ifndef WAVY_CLOTH_INCLUDED
#define WAVY_CLOTH_INCLUDED

// Fungsi utama untuk deformasi kain bergelombang
void WavyCloth_float(
    float3 PositionOS, float2 UV, float Speed, float Amp, float Freq, float ScrollProgress,
    out float3 OutPosition, out float2 OutUV)
{
    float time = _Time.y * Speed;
    float distanceFactor = saturate((PositionOS.x - 0.25) * 2.0);
    float dynamicAmp = Amp * distanceFactor;
    float dynamicFreq = Freq + (Freq * distanceFactor * 2.0);

    float wavePhase = (PositionOS.x * 1.5 + PositionOS.z * 1.2) * dynamicFreq;
    float waveMotion = sin(wavePhase + time) * dynamicAmp;
    float waveNoise = cos((wavePhase * 0.8) - time * 0.5) * (dynamicAmp * 0.7);
    float noiseMotion = cos(PositionOS.x * 3.0 - PositionOS.z * 2.0 + time * 0.4) * 0.15;

    float waveHeight = waveMotion + waveNoise + noiseMotion;
    float3 waveDisplacement = float3(
        sin(time + PositionOS.z) * 0.05,
        waveHeight,
        cos(time * 0.8 + PositionOS.x) * 0.05
    );

    OutPosition = PositionOS + waveDisplacement * distanceFactor;
    OutUV = UV + float2(0, ScrollProgress);
}

#endif // WAVY_CLOTH_INCLUDED
