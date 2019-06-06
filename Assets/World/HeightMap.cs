using UnityEngine;

namespace World
{
    public class HeightMap
        {
            private float[,] data;
            private float heightMultiplier;
            private AnimationCurve heightCurve;

            public HeightMap(float[,] data, AnimationCurve heightCurve, float heightMultiplier = 1f)
            {
                this.data = data;
                this.heightMultiplier = heightMultiplier;
                this.heightCurve = heightCurve;
            }

            public float this[int x, int y]
            {
                get 
                {
                    return heightCurve.Evaluate(data[x,y]) * heightMultiplier;
                }
            }

            public int Height
            {
                get { return data.GetLength(0); }
            }

            public int Width
            {
                get { return data.GetLength(1); }
            }
        }
}