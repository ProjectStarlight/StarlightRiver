sampler uImage0 : register(s0);
uint occlusionType : register(c0);

float visc;
float dT;
int N;
float2 resolution;

float2 relativeScreenPos;
float MistDims;
float2 ScreenDims;

texture velocityXField;
sampler velXsampler = sampler_state
{
    Texture = (velocityXField);
};

texture velocityYField;
sampler velYsampler = sampler_state
{
    Texture = (velocityYField);
};

texture bufferTarget;
sampler bufferSample = sampler_state
{
    Texture = (bufferTarget);
};

texture divMap;
sampler divSample = sampler_state
{
    Texture = (divMap);
};

texture pMap;
sampler pSample = sampler_state
{
    Texture = (pMap);
};

texture adDensity;
sampler ADS = sampler_state
{
    Texture = (adDensity);
};

texture boundaries;
sampler boundariesSample = sampler_state
{
    Texture = (boundaries);
};

float4x4 MATRIX;

struct VertexShaderInput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : POSITION0;
    float4 Color : COLOR0;
};

struct VertexShaderOutput
{
    float2 TextureCoordinates : TEXCOORD0;
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
};

VertexShaderOutput MainVS(float4 position : SV_POSITION, float4 color : COLOR0, float2 texCoord : TEXCOORD0)
{
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.Position = mul(position, MATRIX);
    output.TextureCoordinates = texCoord;

    return output;
}

float round(float number, float precision)
{
    return (float) floor(number / precision) * precision;
}

float2 round(float2 number, float precision)
{
    return float2(round(number.x, precision), round(number.y, precision));
}

float convertToTrue(float4 c)
{
    return c.g - c.r;
}

float convertToTruePrecision(float4 c)
{
    float combine = c.g + c.r;
    return c.g - c.r + (1 - sign(floor(abs(combine * 255.0f)))) * (c.b - 0.5f) * (2.0f / 255.0f);
}

float4 convertToColor(float c)
{
    int s = (sign(c) + 1) / 2;
    return float4((1 - s) * abs(c), s * c, 0, abs(c));
}

float4 convertToColorPrecision(float c)
{
    float s = (sign(c) + 1) / 2.0f;
    int sI = s;
    int p = c == 0 ? 2 : 1 - sign(floor(abs(c * 255.0f)));
    float3 clr = float3((1 - sI) * abs(c), sI * c, p * (s + abs(c * 255.0f)) * 0.5f);
    return float4(clr, 1);
}


float constrict(float x)
{
    return x;
}

float4 diffuse(VertexShaderOutput input) : COLOR0
{
    float cVisc = visc * N * N * dT;

    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoords = float2(((float) floor(input.TextureCoordinates.x / h) * h), ((float) floor(input.TextureCoordinates.y / h) * h)) + smallValue;

    float4 colour;
    float4 pColour = tex2D(bufferSample, input.TextureCoordinates);
    
    float up = convertToTrue(tex2D(uImage0, roundedCoords + float2(0, -h)));
    float down = convertToTrue(tex2D(uImage0, roundedCoords + float2(0, h)));
    float left = convertToTrue(tex2D(uImage0, roundedCoords + float2(-h, 0)));
    float right = convertToTrue(tex2D(uImage0, roundedCoords + float2(h, 0)));

    float totalPressure;
    float numberOfNeighbours;
  
    totalPressure = right + left + down + up;
    numberOfNeighbours = 6;
    
    colour = (pColour + totalPressure * cVisc) / (1 + numberOfNeighbours * cVisc);
    
    return pColour;
}

float4 advect(VertexShaderOutput input) : COLOR0
{
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;
    
    float2 roundedCoords = round(input.TextureCoordinates, h);

    float colour;
    float velX = convertToTruePrecision(tex2D(velXsampler, roundedCoords + smallValue));
    float velY = convertToTruePrecision(tex2D(velYsampler, roundedCoords + smallValue));
        
    float visc = dT * N;
    float XGrid, YGrid, XGrid1, YGrid1;
    
    float X = roundedCoords.x - (visc * velX) * h;
    float Y = roundedCoords.y - (visc * velY) * h;
    
    X = clamp(X, 0, 1 - h);
    Y = clamp(Y, 0, 1 - h);
    
    XGrid = round(X, h);
    XGrid1 = XGrid + h;
    YGrid = round(Y, h);
    YGrid1 = YGrid + h;

    float XRelative1 = (X - XGrid) / h;
    float XRelative0 = 1 - XRelative1;
    float YRelative1 = (Y - YGrid) / h;
    float YRelative0 = 1 - YRelative1;

    float c1 = convertToTruePrecision(tex2D(bufferSample, float2(XGrid, YGrid) + smallValue));
    float c2 = convertToTruePrecision(tex2D(bufferSample, float2(XGrid, YGrid1) + smallValue));
    float c3 = convertToTruePrecision(tex2D(bufferSample, float2(XGrid1, YGrid) + smallValue));
    float c4 = convertToTruePrecision(tex2D(bufferSample, float2(XGrid1, YGrid1) + smallValue));

    colour = XRelative0 * (YRelative0 * c1 + YRelative1 * c2) +
             XRelative1 * (YRelative0 * c3 + YRelative1 * c4);

    return convertToColorPrecision(colour);
}

float4 projectdiv(VertexShaderOutput input) : COLOR0
{
     //use poisson equations to make sure vector field magnitudes are being conserved
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordsTrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordsTrue + smallValue;

    if (roundedCoordsTrue.x == 0 || roundedCoordsTrue.y == 0 || roundedCoordsTrue.x > 1 - h || roundedCoordsTrue.y > 1 - h)
        return tex2D(uImage0, roundedCoords);

    float velX1 = convertToTruePrecision(tex2D(velXsampler, roundedCoords + float2(h, 0)));
    float velX2 = convertToTruePrecision(tex2D(velXsampler, roundedCoords + float2(-h, 0)));
    float velY1 = convertToTruePrecision(tex2D(velYsampler, roundedCoords + float2(0, h)));
    float velY2 = convertToTruePrecision(tex2D(velYsampler, roundedCoords + float2(0, -h)));
    
    float colour = -0.5f * h * (velX1 - velX2 +
                     velY1 - velY2);
    
    return convertToColorPrecision(colour);
}

float4 projectp(VertexShaderOutput input) : COLOR0
{
     //use poisson equations to make sure vector field magnitudes are being conserved
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordsTrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordsTrue + smallValue;

    if (roundedCoordsTrue.x == 0 || roundedCoordsTrue.y == 0 || roundedCoordsTrue.x > 1 - h || roundedCoordsTrue.y > 1 - h)
        return tex2D(uImage0, roundedCoords);
    
    float up = convertToTruePrecision(tex2D(uImage0, roundedCoords + float2(0, -h)));
    float down = convertToTruePrecision(tex2D(uImage0, roundedCoords + float2(0, h)));
    float left = convertToTruePrecision(tex2D(uImage0, roundedCoords + float2(-h, 0)));
    float right = convertToTruePrecision(tex2D(uImage0, roundedCoords + float2(h, 0)));

    float div = convertToTruePrecision(tex2D(divSample, roundedCoords));
    float c = (div + up + down + left + right) / 6.0f;

    return convertToColorPrecision(c);
}

float4 projectuv(VertexShaderOutput input, uniform bool dir) : COLOR0
{
     //use poisson equations to make sure vector field magnitudes are being conserved
    float h = 1 / (float) N;
    
    float2 smallValue = float2(h, h) / 2.0f;
    
    float2 roundedCoordsTrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordsTrue + smallValue;

    if (roundedCoordsTrue.x == 0 || roundedCoordsTrue.y == 0 || roundedCoordsTrue.x > 1 - h || roundedCoordsTrue.y > 1 - h)
        return tex2D(uImage0, roundedCoords);
    
    float colour = convertToTruePrecision(tex2D(uImage0, roundedCoords));
    
    float velX1 = convertToTruePrecision(tex2D(pSample, roundedCoords + float2(h, 0)));
    float velX2 = convertToTruePrecision(tex2D(pSample, roundedCoords + float2(-h, 0)));
    float velY1 = convertToTruePrecision(tex2D(pSample, roundedCoords + float2(0, h)));
    float velY2 = convertToTruePrecision(tex2D(pSample, roundedCoords + float2(0, -h)));
    
    colour -= dir ? 0.5f * (velX1 - velX2) / h : 0.5f * (velY1 - velY2) / h;
    
    return convertToColorPrecision(colour);
}

float4 addsource(VertexShaderOutput input) : COLOR0
{
    float4 add = tex2D(ADS, input.TextureCoordinates);
    
    return add + tex2D(uImage0, input.TextureCoordinates);
}

float4 maptogas(VertexShaderOutput input) : COLOR0
{
    float4 map = tex2D(uImage0, input.TextureCoordinates);
    float c = convertToTruePrecision(map);
    return float4(c, c, c, c);
}

float4 configureOcclusion(VertexShaderOutput input, int type) : COLOR0
{

    /*
    for (int i = 1; i < N - 1; i++)
    {
        array[XY(0, i)] = type == 1 ? -array[XY(1, i)] : array[XY(1, i)];
        array[XY(N - 1, i)] = type == 1 ? -array[XY(N - 2, i)] : array[XY(N - 2, i)];
        array[XY(i, 0)] = type == 2 ? -array[XY(i, 1)] : array[XY(i, 1)];
        array[XY(i, N - 1)] = type == 2 ? -array[XY(i, N - 2)] : array[XY(i, N - 2)];
    }

    array[XY(0, 0)] = 0.5f * (array[XY(1, 0)] + array[XY(0, 1)]);
    array[XY(0, N - 1)] = 0.5f * (array[XY(1, N - 1)] + array[XY(0, N - 2)]);
    array[XY(N - 1, 0)] = 0.5f * (array[XY(N - 2, 0)] + array[XY(N - 1, 1)]);
    array[XY(N - 1, N - 1)] = 0.5f * (array[XY(N - 2, N - 1)] + array[XY(N - 1, N - 2)]);
    */    
    
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    //TODO: figure out some edge function to stop branching
    
    float2 roundedCoords = round(input.TextureCoordinates, h);
    float2 ac = input.TextureCoordinates;
    
    return 0.5f * (tex2D(uImage0, float2(h, 0)) + tex2D(uImage0, float2(0, h)));
    return 0.5f * (tex2D(uImage0, float2(h, 1 - h)) + tex2D(uImage0, float2(0, 1 - 2 * h)));
    return 0.5f * (tex2D(uImage0, float2(1 - 2 * h, 0)) + tex2D(uImage0, float2(1 - h, h)));
    return 0.5f * (tex2D(uImage0, float2(1 - 2 * h, 1 - h)) + tex2D(uImage0, float2(1 - h, h - 2 * h)));
    return type == 1 ? -tex2D(uImage0, float2(h, ac.y)) : tex2D(uImage0, float2(1, ac.y));
    return type == 1 ? -tex2D(uImage0, float2(1 - 2 * h, ac.y)) : tex2D(uImage0, float2(1 - 2 * h, ac.y));
    return type == 2 ? -tex2D(uImage0, float2(ac.x, h)) : tex2D(uImage0, float2(ac.x, h));
    return type == 2 ? -tex2D(uImage0, float2(ac.x, 1 - 2 * h)) : tex2D(uImage0, float2(ac.x, 1 - 2 * h));

    return tex2D(uImage0, input.TextureCoordinates);
}

float4 configureOcclusionL(VertexShaderOutput input) : COLOR0
{
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordstrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordstrue + smallValue;
    
    float condition = 1 - sign(floor(roundedCoordstrue.x / h));

    float4 negative = convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(h + h / 2, roundedCoords.y))));
    
    int buffer = occlusionType;
    int typecondition = sign(abs(buffer - 1));
    
    float4 occlusion = condition * ((1 - typecondition) * negative + typecondition * tex2D(uImage0, float2(h + h / 2, roundedCoords.y)));
    float4 regular = (1 - condition) * tex2D(uImage0, input.TextureCoordinates);
    
    return regular + occlusion;
}


float4 configureOcclusionR(VertexShaderOutput input) : COLOR0
{
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordstrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordstrue + smallValue;
    
    float condition = 1 - sign(floor((1 - h - roundedCoordstrue.x) / h));
    float4 negative = convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(1 - 2 * h + h / 2, roundedCoords.y))));

    int buffer = occlusionType;
    int typecondition = sign(abs(buffer - 1));
    
    float4 occlusion = condition * ((1 - typecondition) * negative + typecondition * tex2D(uImage0, float2(1 - 2 * h + h / 2, roundedCoords.y)));
    float4 regular = (1 - condition) * tex2D(uImage0, input.TextureCoordinates);
    
    return (regular + occlusion);
}

float4 configureOcclusionU(VertexShaderOutput input) : COLOR0
{
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;
   
    float2 roundedCoordstrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordstrue + smallValue;

    float condition = 1 - sign(floor(roundedCoordstrue.y / h));
    float4 negative = convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(roundedCoords.x, h + h / 2))));

    int buffer = occlusionType;
    int typecondition = sign(abs(buffer - 2));
    
    float4 occlusion = condition * ((1 - typecondition) * negative + typecondition * tex2D(uImage0, float2(roundedCoords.x, h + h / 2)));
    float4 regular = (1 - condition) * tex2D(uImage0, input.TextureCoordinates);
    
    return (regular + occlusion);
}

float4 configureOcclusionD(VertexShaderOutput input) : COLOR0
{
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordstrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordstrue + smallValue;

    float condition = 1 - sign(floor((1 - h - roundedCoordstrue.y) / h));
    float4 negative = convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(roundedCoords.x, 1 - 2 * h + h / 2))));
    
    int buffer = occlusionType;
    int typecondition = sign(abs(buffer - 2));
    
    float4 occlusion = condition * ((1 - typecondition) * negative + typecondition * tex2D(uImage0, float2(roundedCoords.x, 1 - 2 * h + h / 2)));
    float4 regular = (1 - condition) * tex2D(uImage0, input.TextureCoordinates);
    
    return regular + occlusion;
}

float4 configureCorners(VertexShaderOutput input) : COLOR0
{
    
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordstrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordstrue + smallValue;

    if (roundedCoordstrue.x == 0 && roundedCoordstrue.y == 0)
        return 0.5f * (tex2D(uImage0, float2(h, 0) + smallValue) + tex2D(uImage0, float2(0, h) + smallValue));
    if (roundedCoordstrue.x == 0 && roundedCoordstrue.y >= 1 - h)
        return 0.5f * (tex2D(uImage0, float2(h, 1 - h) + smallValue) + tex2D(uImage0, float2(0, 1 - 2 * h) + smallValue));
    if (roundedCoordstrue.x >= 1 - h && roundedCoordstrue.y == 0)
        return 0.5f * (tex2D(uImage0, float2(1 - 2 * h, 0) + smallValue) + tex2D(uImage0, float2(1 - h, h) + smallValue));
    if (roundedCoordstrue.x >= 1 - h && roundedCoordstrue.y >= 1 - h)
        return 0.5f * (tex2D(uImage0, float2(1 - 2 * h, 1 - h) + smallValue) + tex2D(uImage0, float2(1 - h, 1 - 2 * h) + smallValue));
    
    return tex2D(uImage0, input.TextureCoordinates);
}

float4 configureMiscBoundaries(VertexShaderOutput input) : COLOR0
{
    /*
    float h = 1 / (float) N;
    float2 smallValue = float2(h, h) / 2.0f;

    float2 roundedCoordstrue = round(input.TextureCoordinates, h);
    float2 roundedCoords = roundedCoordstrue + smallValue;
    
    float i = roundedCoords.x;
    float j = roundedCoords.y;
        
    float2 dimRelative = float2(MistDims / ScreenDims.x, MistDims / ScreenDims.y);
    
    float2 boundaryRelative = relativeScreenPos + dot(roundedCoordstrue, dimRelative);
    
    int type = occlusionType;

    bool u = tex2D(boundariesSample, boundaryRelative + float2(0, -h)) != 0;
    bool d = tex2D(boundariesSample, boundaryRelative + float2(0, h)) != 0;
    bool l = tex2D(boundariesSample, boundaryRelative + float2(-h, 0)) != 0;
    bool r = tex2D(boundariesSample, boundaryRelative + float2(h, 0)) != 0;
    
    if(u)
        return float4(1, 0, 0, 1);
    
    if (u && r && !d && !l)
        return 0.5f * (tex2D(uImage0, float2(i - h, j)) + tex2D(uImage0, float2(i, j + h)));
    if (!u && !r && d && l)
        return 0.5f * (tex2D(uImage0, float2(i + h, j)) + tex2D(uImage0, float2(i, j - h)));
    if (u && !r && !d && l)
        return 0.5f * (tex2D(uImage0, float2(i + h, j)) + tex2D(uImage0, float2(i, j + h)));
    if (!u && r && d && !l)
        return 0.5f * (tex2D(uImage0, float2(i - h, j)) + tex2D(uImage0, float2(i, j - h)));

    if (u && !r && !d && !l)
        return type == 2 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i, j + h)))) : tex2D(uImage0, float2(i, j + h));
    if (d && !u && !r && !l)
        return type == 2 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i, j - h)))) : tex2D(uImage0, float2(i, j - h));
    if (l && !r && !d && !u)
        return type == 1 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i + h, j)))) : tex2D(uImage0, float2(i + h, j));
    if (r && !u && !d && !l)
        return type == 1 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i - h, j)))) : tex2D(uImage0, float2(i - h, j));

    if (!u && r && d && l)
        return type == 2 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i, j - h)))) : tex2D(uImage0, float2(i, j - h));
    if (!d && u && r && l)
        return type == 2 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i, j + h)))) : tex2D(uImage0, float2(i, j + h));
    if (!l && r && d && u)
        return type == 1 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i - h, j)))) : tex2D(uImage0, float2(i - h, j));
    if (!r && u && d && l)
        return type == 1 ? convertToColorPrecision(-convertToTruePrecision(tex2D(uImage0, float2(i + h, j)))) : tex2D(uImage0, float2(i + h, j));
    */
    return tex2D(uImage0, input.TextureCoordinates);
}



technique Technique1
{
    pass diffuse
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 diffuse();
    }

    pass advect
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 advect();
    }
       //Nomis is dumb and duymb
       //Nomis devourer of children, the mighty frog
       //Hello
       //Im mister frog
       //This is my show
       //I eat the bug
    pass projectdiv
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 projectdiv();
    }

    pass projectp
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 projectp();
    }

    pass projectu
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 projectuv(true);
    }

    pass projectv
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 projectuv(false);
    }

    pass projectadd
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 addsource();
    }

    pass projectmap
    {
        PixelShader = compile ps_2_0 maptogas();
    }

    //OCCLUSION

    pass configureOcclusionL
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 configureOcclusionL();
    }

    pass configureOcclusionR
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 configureOcclusionR();
    }

    pass configureOcclusionU
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 configureOcclusionU();
    }

    pass configureOcclusionD
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 configureOcclusionD();
    }

    pass configureCorners
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 configureCorners();
    }

    pass configureMiscBoundaries
    {
        VertexShader = compile vs_3_0 MainVS();
        PixelShader = compile ps_3_0 configureMiscBoundaries();
    }
}