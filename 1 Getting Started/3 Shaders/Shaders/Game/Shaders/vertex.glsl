﻿#version 330 core
layout (location = 0) in vec3 aPos;

out vec4 vertexColour;

void main()
{
    vertexColour = vec4(1.0,0.5,0.2,1.0);
    gl_Position = vec4(aPos, 1.0);
}