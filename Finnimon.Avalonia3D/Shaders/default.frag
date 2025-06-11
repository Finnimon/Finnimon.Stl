#version 330 core

in float shade;
uniform vec4 shaded_uniform_color;
out vec4 FragColor;

void main()
{
    //grayscale on color
    FragColor = vec4(shaded_uniform_color.xyz*shade, shaded_uniform_color.w);
}
