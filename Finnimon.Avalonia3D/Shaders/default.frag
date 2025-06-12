#version 330 core

in vec3 fragNormal;
uniform vec4 shaded_uniform_color;
uniform vec3 shade_against;
out vec4 FragColor;

void main()
{
    float shade = dot(fragNormal, shade_against);
    shade=shade*0.5+1;
    shade=shade*0.75+0.25;
    FragColor = vec4(shaded_uniform_color.xyz*shade, shaded_uniform_color.w);
}
