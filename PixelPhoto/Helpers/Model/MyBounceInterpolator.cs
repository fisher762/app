using System;
using Android.Views.Animations;

namespace PixelPhoto.Helpers.Model
{
    public class MyBounceInterpolator : Java.Lang.Object, IInterpolator
    {
        private readonly double MAmplitude = 1;
        private readonly double MFrequency = 10;

        public MyBounceInterpolator(double amplitude, double frequency)
        {
            MAmplitude = amplitude;
            MFrequency = frequency;
        }
        float IInterpolator.GetInterpolation(float time)
        {
            return (float)(-1 * Math.Pow(Math.E, -time / MAmplitude) *
                Math.Cos(MFrequency * time) + 1);
        }

        public float GetInterpolation(float input)
        {
            return 0;
        }
    }
}