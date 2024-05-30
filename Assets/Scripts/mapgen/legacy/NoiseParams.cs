using UnityEngine;

[System.Serializable]
// explains the concepts: frequency, wavelength, amplitude, octaves, pink and blue and white noise, etc. (low level)
// https://www.redblobgames.com/articles/noise/introduction.html

// main article 
// https://www.redblobgames.com/maps/terrain-from-noise/
public class NoiseParams
{
    public FastNoiseLite.NoiseType noiseType = FastNoiseLite.NoiseType.Perlin;
    
    public int seed = 10;

    [Tooltip("Scrolls the map in/out")]
    public float noiseScale;

    [Tooltip("lower frequencies make wider hills \n" +
             "and higher frequencies make narrower hills.")]
    [Range(0.01f, .00000001f)]
    public float frequency = 0.01f;

    public FastNoiseLite.FractalType fractalType = FastNoiseLite.FractalType.PingPong;
    
    [Tooltip("How many different frequencies we are using,\n" +
             "each being distanced by the fractalGain")]
    [Range(1, 10)]
    public int fractalOctaves = 3;

    [Tooltip("Also known as persistance.\n " +
        "The ratio of increase for each fractal octave.\n" + 
        "[1, 1/2, 1/4, 1/8, 1/16, …]\n" + 
        "where each amplitude is 1/2 the previous one.\n"+
        "Don't have to use a fixed ratio,\n" + 
        "could do [1, 1/2, 1/3, 1/4, 1/5]\n" + 
        "to get finer details than what the conventional amplitudes allow.")]
    [Range(0f,1f)]
    public float fractalGain = 0.5f;

    [Tooltip("specifies the frequency multipler between successive octaves.\n" +
              "The effect of modifying the lacunarity is subtle.\n" +
              "For best results, set the lacunarity to a number between 1.5 and 3.5\n")]
    [Range(1, 5)]
    public float fractalLacunarity = 2;

    [Tooltip("Sets octave weighting for all none DomainWarp fractal types\n" +
             "Default: 0.0")]
    [Range(0f, 1f)]
    public float weightedStrength = 0;



    // Non-FastNoiseLite values
    //
    // This is the power for the noise to be raised to, in order to get flat valleys.
    // Set to 1 for no change.
    [Tooltip("This is the power for the noise to be raised to,\n in order to get flat valleys.")]
    [Range(1f, 100f)]
    public float raisedPower = 1f;

    //
    [Tooltip("Minimum height value so that we can set into ground to get flat ocean.\n Don't think being used anymore")]
    public float minValue = .5f;

    public string SerializeParamsToJson()
    {
        return JsonUtility.ToJson(this, true); //Always export with prettyprint
    }
}
