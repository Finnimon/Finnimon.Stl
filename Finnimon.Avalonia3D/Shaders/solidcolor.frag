#version 330 core

uniform vec4 uniform_color;
out vec4 FragColor;

void main()
{
    FragColor = uniform_color;
}
