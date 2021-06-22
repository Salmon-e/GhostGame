texture ScreenTexture;
float2 resolution;

sampler screenSampler = sampler_state
{
    Texture = <ScreenTexture>;
};
float4 PixelShaderFunction(float2 TexCoord : TEXCOORD0) : COLOR0
{
    float2 pixelCoord = TexCoord * resolution;
    float2 up = float2(0, -1);
    float2 down = -up;
    float2 left = float2(-1, 0);
    float2 right = -left;
    float4 color = tex2D(screenSampler, TexCoord);
    float2 dirs[8] = { up, down, left, right, up + left, up + right, down + left, down + right };
    for (int i = 0; i < 8; i++) {
        color += tex2D(screenSampler, (pixelCoord + dirs[i]) / resolution);
    }
    return color/9;
}

technique Plain
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    }
}
