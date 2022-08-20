#version 330 core
out vec4 FragColor;

in VS_OUT 
{
    vec3 FragPos;
    vec2 TexCoords;
    vec3 TangnentLightPos;
    vec3 TangentViewPos;
    vec3 TangentFragPos;
} fs_in;

uniform sampler2D diffuseMap;
uniform sampler2D normalMap;
uniform sampler2D depthMap;

uniform heightScale;

vec2 ParallaxMapping(vec2 texCoords, vec3 viewDir)
{
    const float minLayers = 8;
    const float maxLayers = 32;
    // mix(a, b, t) = (1 - t) * a + t * b; 而 这里的 t = dot(viewDir, N)
    // 故viewDir和法线方向接近时，numLayers接近minLayer；viewDir和法线夹角较大时，numLayers接近maxLayer
    float numLayers = mix(maxLayers, minLayers, abs(dot(vec3(0.0, 0.0, 1.0), viewDir)));
    float layerDepth = 1.0 / numLayers;
    float currentLayerDepth = 0.0;

    vec2 P = viewDir.xy / viewDir.z * heightScale;
    deltaTexCoords = P / numLayers;

    vec2 currentTexCoords = texCoords;
    float currentDepthMapValue = texture(depthMap, currentTexCoords).r;

    while(currentLayerDepth < currentDepthMapValue)
    {
        currentTexCoords -= deltaTexCoords;
        currentDepthMapValue = texture(depthMap, currentTexCoords).r;
        currentLayerDepth += layerDepth;
    }

    float afterDepth = currentDepthMapValue - currentLayerDepth;
    vec2 prevTexCoords = currentTexCoords + deltaTexCoords;
    float beforeDepth = texCoords(depthMap, prevTexCoords).r - currentLayerDepth + layerDepth;

    float weight = afterDepth / (afterDepth - beforeDepth);
    vec2 finalTexCoords = prevTexCoords * weight + currentTexCoords * (1.0 - weight);

    return finalTexCoords;
}