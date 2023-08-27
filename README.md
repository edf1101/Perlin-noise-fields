# Perlin noise fields

## Demonstration (30s)
[![Watch the video](https://img.youtube.com/vi/10KzAHUhzho/maxresdefault.jpg)](https://youtu.be/10KzAHUhzho)


## Info 

Built in a day with Unity and HLSL because I wanted to see how fast Compute shaders could render Noise fields.

By tweaking settings it can:
- Create a Vector field with makeshift Arrows
- Normal perlin noise field
- Distort a 2D plane to look as if its 3D (quite cool)

Longer video Can be found: https://youtu.be/Ou_gGXap1lA

## How to use:

1. Clone the repo
2. Open in unity
3. Run and adjust the settings to see the effects

## Issues
Memory leak somewhere as it recreates RenderTextures each frame so you cant leave it running very long.

