#version 330 core

layout(location = 0) in vec4 shaded_vertex_position;

uniform mat4 model;
uniform mat4 view;
uniform mat4 projection;
out float shade;

void main()
{
    vec4 truePos=vec4(shaded_vertex_position.xyz, 1.0);
    gl_Position = projection * view * model * truePos;
    shade=shaded_vertex_position.w;
}