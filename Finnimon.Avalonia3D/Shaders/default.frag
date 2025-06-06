#version 330 core

out vec4 FragColor;

void main()
{
    // simple grayscale
    FragColor = vec4(1.0, 1.0, 1.0, 1.0);

    // or: use a colormap (e.g. blue-red)
    // FragColor = mix(vec4(0, 0, 1, 1), vec4(1, 0, 0, 1), vScalar);
}
