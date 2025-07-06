#ifndef INCLUDED_COMMON
#define INCLUDED_COMMON

#define PI 3.14159265359f
#define TWO_PI 6.28318530718f
#define HALF_PI 1.57079632679f

float GetLuminance(float3 color)
{
    return dot(color, float3(0.299, 0.587, 0.114));
}

#endif