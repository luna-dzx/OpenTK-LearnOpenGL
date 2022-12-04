#version luma-dx

layout(binding = 0) uniform sampler2D gPosition;
layout(binding = 1) uniform sampler2D gNormal;
layout(binding = 2) uniform sampler2D noiseTex;

uniform vec3 cameraPos;
uniform mat4 proj;
uniform vec3 samples[64];

in vec2 texCoords;

// tile noise texture over screen, based on screen dimensions divided by noise size
const vec2 noiseScale = vec2(1600.0/4.0, 900.0/4.0);

void main()
{
    vec3 fragPos   = texture(gPosition, texCoords).xyz;
    vec3 normal    = texture(gNormal, texCoords).rgb;
    vec3 randomVec = texture(noiseTex, texCoords*noiseScale).xyz;
    
    vec3 tangent   = normalize(randomVec - normal * dot(randomVec, normal));
    vec3 bitangent = cross(normal, tangent);
    mat3 TBN       = mat3(tangent, bitangent, normal);
    
    float kernelSize = 64.0;
    float radius = 0.5;
    float bias = 0.025;
    
    float occlusion = 0.0;
    for(int i = 0; i < kernelSize; ++i)
    {
        // get sample position
        vec3 samplePos = TBN * samples[i]; // from tangent to view-space
        samplePos = fragPos + samplePos * radius; 
        
        vec4 offset = vec4(samplePos, 1.0);
        offset      = proj * offset;    // from view to clip-space
        offset.xyz /= offset.w;               // perspective divide
        offset.xyz  = offset.xyz * 0.5 + 0.5; // transform to range 0.0 - 1.0  
        
        float sampleDepth = texture(gPosition, offset.xy).z;
        float rangeCheck = smoothstep(0.0,1.0, radius / abs(fragPos.z - sampleDepth));
        occlusion       += (sampleDepth >= samplePos.z +bias ? 1.0 : 0.0)*rangeCheck;    
    } 
    
    occlusion = 1.0 - (occlusion / kernelSize);
    lx_FragColour = vec4(occlusion);
    
}