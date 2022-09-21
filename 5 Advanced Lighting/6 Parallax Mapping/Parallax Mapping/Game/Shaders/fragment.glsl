#version luma-dx

uniform lx_Material material;
uniform lx_Light light;
uniform sampler2D normalMap;
uniform sampler2D displaceMap;
uniform vec3 cameraPos;

uniform int normalMapping;

in VS_OUT {
    vec3 fragPos;
    vec3 normal;
    vec2 texCoords;
    vec3 TBNfragPos;
    vec3 TBNlightPos;
    vec3 TBNcameraPos;
    vec3 tangent;
} fs_in;

uniform float height_scale;

[scene]
void main()
{
    vec2 texCoords = lx_ParallaxMapping(displaceMap, fs_in.texCoords,  normalize(fs_in.TBNcameraPos - fs_in.TBNfragPos), height_scale, 8.0, 32.0);
    if(texCoords.x > 1.0 || texCoords.y > 1.0 || texCoords.x < 0.0 || texCoords.y < 0.0) discard;

    vec3 normal = lx_NormalMap(normalMap,texCoords);
    vec3 phong = lx_Phong(normal, fs_in.TBNfragPos, fs_in.TBNcameraPos, texCoords, material, lx_MoveLight(light,fs_in.TBNlightPos), 1.0);
    lx_FragColour = lx_Colour(phong);

}

[light]
void main()
{
    lx_FragColour = vec4(1.0);
}