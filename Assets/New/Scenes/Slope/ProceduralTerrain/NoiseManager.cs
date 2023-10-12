using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NoiseManager : MonoBehaviour
{
    public RawImage m_noiseTexture;
    public Terrain m_NoiseTerrain;
    public int width = 256;
    public int height = 256;

    private Noise _noise;

    private void Awake()
    {
        _noise = new PerlinNoise();
        _RecomputeNoise();
    }

    private void _RecomputeNoise()
    {
        float[,] noise = new float[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                noise[y,x] = _noise.GetNoiseMap(x, y, 0.02f);
            }
        }

        _SetNoiseTexture(noise);
    }

    private void _SetNoiseTexture(float[,] noise)
    {
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pixels[x + width * y] = Color.Lerp(Color.black, Color.white, noise[y, x]);
            }
        }
        Texture2D texture = new Texture2D(width, height);

        texture.SetPixels(pixels);
        texture.Apply();

        m_noiseTexture.texture = texture;
        m_NoiseTerrain.terrainData.SetHeights(0, 0, noise);
    }
}
