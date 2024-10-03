sampler uImage0 : register(s0);

float rotation;
float xRotation;
float holdout;

float2 rotate2D(float2 vec, float angle)
{
    float2x2 rotationMatrix = { {sin(3.14159265 / 2 - angle), -sin(angle)}, {sin(angle), sin(3.14159265 / 2 - angle)} };
    return mul(rotationMatrix, vec);
}

float3 rotate3D(float3 vec, float angle)
{
    float3x3 rotationMatrix = { {1, 0, 0}, {0, sin(3.14159265 / 2 - angle), -sin(angle)}, {0, sin(angle), sin(3.14159265 / 2 - angle)} };
    return mul(rotationMatrix, vec);
}

float4 WeaponDepth(float2 coords : TEXCOORD0) : COLOR0
{
    // center image at handle
    float2 centeredCoords = {coords.x - holdout, coords.y - holdout};

    // rotate image to be level and rotate again depending on passed data
    centeredCoords = rotate2D(centeredCoords, 3.14159265 * 3 / 4 + rotation);

    // rotate in 3D
    float3 coords3D = {centeredCoords.x, centeredCoords.y, centeredCoords.y * sin(xRotation) / sin(3.14159265 / 2 - xRotation)};
    coords3D = rotate3D(coords3D, -xRotation);

    // project coordinates back to 2D
    float2 projCoords = coords3D.xy;

    // undo rotation done before
    projCoords = rotate2D(projCoords, -3.14159265 * 3 / 4 - rotation);

    // translate image back to original space
    projCoords.y += holdout;
    projCoords.x += holdout;

    float2 newCoords = projCoords;

    if (newCoords.x <= 1 && newCoords.x >= 0 && newCoords.y <= 1 && newCoords.y >= 0)
    {
        float4 finalTexture = tex2D(uImage0, newCoords);
        return finalTexture;
    }
    else
    {
        return float4(0, 0, 0, 0);
    }
}

technique Technique1
{
    pass WeaponDepth
    {
        PixelShader = compile ps_2_0 WeaponDepth();
    }
}