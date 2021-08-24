using UnityEngine;

public static class AudioUnitConverter
{
    #region Float <-> dB (Debibel)
    public static float FloatToDecibel(float floatToDecibel)
    {
        return Mathf.Log10(floatToDecibel) * 20f;
    }
    public static float DecibelToFloat(float decibelToFloat)
    {
        return Mathf.Pow(10f, (decibelToFloat / 20f));
    }
    #endregion

    #region Float <-> mB (Millibel)
    public static float FloatToMillibel(float floatToMillibel)
    {
        return FloatToDecibel(floatToMillibel) * 100;
    }
    public static float MillibelToFloat(float millibelToFloat)
    {
        return DecibelToFloat(millibelToFloat / 100);
    }
    #endregion

    #region Second (s) <-> Millisecond (ms)
    public static float SecondsToMilliseconds(float secondsToMilliseconds)
    {
        return secondsToMilliseconds * 1000f;
    }
    public static float MillisecondsToSeconds(float millisecondsToSeconds)
    {
        return millisecondsToSeconds / 1000f;
    }
    #endregion

    #region Second (s) <-> Hertz (Hz)
    public static float SecondsToHertz(float secondsToHertz)
    {
        return 1 / secondsToHertz;
    }
    public static float HertzToSeconds(float hertzToSeconds)
    {
        return 1 / hertzToSeconds;
    }
    #endregion
}