texture screen;
texture tileTexture;
texture blurTileTexture;
float tileSize;
float2 screenSize;
float2 tileOrigin;
float2 tileTextureSize;
float zoom;
float2 cameraPos;
float ambient;
// X, Y is position, Z is intensity
float3 lights[50];
int lightCount;
sampler screenSampler = sampler_state {
    Texture = <screen>;
};
// Sampler for checking whether there is a tile at a given point
sampler tileSampler = sampler_state
{
    Texture = <tileTexture>;
    magfilter = POINT;
    minfilter = POINT;
    mipfilter = POINT;
};
// Sampler for a gradient effect on tiles
sampler blurredTileSampler = sampler_state {
    Texture = <blurTileTexture>;
};


float solidity(float2 p)
{       
    float2 texCoord = (p / tileSize) - tileOrigin;     
    texCoord += float2(0.5, 0.5);
    texCoord.x /= tileTextureSize.x;
    texCoord.y /= tileTextureSize.y;   
    return tex2D(tileSampler, texCoord).r;
}
float2 toScreenCoords(float2 TexCoord) 
{
    return float2(TexCoord.x * screenSize.x, TexCoord.y * screenSize.y);
}
float round(float value, float direction) {
    return sign(direction) * ceil(value * sign(direction));
}

float4 PixelShaderFunction(float2 TexCoord : TEXCOORD0) : COLOR0
{    
    float2 pixelPos = (toScreenCoords(TexCoord) - screenSize / 2) / zoom + cameraPos;
    float2 tileTexCoord = (pixelPos / tileSize) - tileOrigin;
    float4 color = tex2D(screenSampler, TexCoord);
    tileTexCoord += float2(0.5, 0.5);
    tileTexCoord.x /= tileTextureSize.x;
    tileTexCoord.y /= tileTextureSize.y;
    float pixelSolidity = solidity(pixelPos);
    float blurredSolidity = 1- tex2D(blurredTileSampler, tileTexCoord).r;        
    float light = blurredSolidity * pixelSolidity + 1 - pixelSolidity;    
    float outLight = light * ambient;
    
    for (int i = 0; i < lightCount; i++)
    {
        
        float intersected = 0;     
        float2 position = pixelPos + float2(tileSize / 2, tileSize / 2);
        float3 l = lights[i];

        float2 dir = normalize(l.xy - position);
        float travelDistance = 0;
        float dist = distance(l.xy, pixelPos);        
        int stepCount = 0;     
        float startedInBlock = pixelSolidity;
        if (pixelSolidity == 0) {
            [unroll(10)] while (travelDistance < dist && intersected == 0 && dist < l.z && stepCount < 10 && outLight != 0)
            {
                float solid = solidity(position - float2(tileSize / 2, tileSize / 2));
                intersected = solid;

                float targetX = round(position.x / tileSize, dir.x) * tileSize;
                float targetY = round(position.y / tileSize, dir.y) * tileSize;

                float2 xStep = dir * (targetX - position.x) / dir.x;
                float2 yStep = dir * (targetY - position.y) / dir.y;

                float2 step = ((length(yStep) > length(xStep)) ? xStep : yStep) * 1.1;

                position += step;
                travelDistance += length(step);
                stepCount++;
            }
        }
        outLight += light * clamp(1 - dist / l.z, 0, 1) * (1 - intersected);
    }    
    return color * outLight;
}

technique Plain
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}