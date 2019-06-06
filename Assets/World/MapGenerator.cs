using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LibNoise;
using LibNoise.Generator;

namespace World {

    public class MapGenerator : MonoBehaviour
    {

        public const int ChunkSize = 97;
        public const int LevelsOfDetail = 5;

        public Vector2 mapSize = new Vector2(1f, 1f);
        public Vector2 noiseShift = new Vector2(0f, 0f);
        public Vector2 noiseScale = new Vector2(1f, 1f);
        
        // noise settings 
        // TODO move to separate component
        public float frequency = 1f;
        public float lacunarity = 2f;
        [MinAttribute(0)]
        public int octaves = 6;
        [Range(0,1)]
        public float persistence = 0.5f;
        public int seed;

        // map texture
        public Renderer textureRenderer;
        public Renderer meshRenderer;
        public MeshFilter meshFilter;

        public Gradient terrainColors;

        public bool autoUpdate;

        private System.Random random = new System.Random();

        public float meshHeightMultiplier = 10f;
        public AnimationCurve meshHeightCurve;

        [Range(0, LevelsOfDetail - 1)]
        public int levelOfDetail;

        public void Start() 
        {
            NewSeed();
        }

        public void NewSeed()
        {
            seed = random.Next();
        }

        public void DrawMapInEditor()
        {
            Map map = GenerateMap(Vector2.zero);

            var texture = new Texture2D(map.heightMap.Width, map.heightMap.Height);
            texture.SetPixels(map.colorMap);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point; // makes changes between regions more rough
            texture.Apply();

            // Draw as texture
            textureRenderer.material.mainTexture = texture; 
            textureRenderer.transform.localScale = new Vector3(mapSize.x, 1, mapSize.y);

            // Draw as mesh
            MeshBuilder meshBuilder = TerrainMeshGenerator.GenerateFlatTerrainMesh(map.heightMap, levelOfDetail);
            meshFilter.sharedMesh = meshBuilder.Build();
            meshRenderer.sharedMaterial.mainTexture = texture;
            //meshRenderer.material.mainTexture = texture;
        }

        public Map GenerateMap(Vector2 offset) 
        {
            Debug.Log(string.Format("Generating map with offset {0}:{1}, seed: {2}", offset.x, offset.y, seed));
            var perlin = new Perlin();
            perlin.OctaveCount = octaves;
            perlin.Frequency = frequency;
            perlin.Lacunarity = lacunarity;
            perlin.Persistence = persistence;
            perlin.Seed = seed;

            var heightMapBuilder = new Noise2D(ChunkSize, ChunkSize, perlin);
            var halfShift = new Vector2(0.5f / noiseScale.x, 0.5f / noiseScale.y);
            heightMapBuilder.GeneratePlanar(
                (offset.x + noiseShift.x) / noiseScale.x - halfShift.x, 
                (offset.x + noiseShift.x) / noiseScale.x + halfShift.x,  
                (-offset.y + noiseShift.y) / noiseScale.y - halfShift.y, 
                (-offset.y + noiseShift.y) / noiseScale.y + halfShift.y
            );

            // prepare texture
            var colorMap = heightMapBuilder.GetTexturePixels(terrainColors);
            Debug.Log("Texture created");
            // preare height map
            var heightMap = new HeightMap(
                data: heightMapBuilder.GetNormalizedData(),
                heightMultiplier: meshHeightMultiplier,
                heightCurve: new AnimationCurve(meshHeightCurve.keys)
            );
            

            return new Map(heightMap, colorMap);
        }

        private void OnValidate() 
        {
            validateMapSize();
            validateLacunarity();
            validateOctaves();
        }

        private void validateMapSize()
        {
            mapSize.x = Mathf.Max(1f, mapSize.x);
            mapSize.y = Mathf.Max(1f, mapSize.y);
        }

        private void validateLacunarity()
        {
            lacunarity = Mathf.Max(1f, lacunarity);
        }

        private void validateOctaves()
        {
            octaves = Mathf.Max(0, octaves);
        }
    }

    public struct Map 
    {
        public readonly HeightMap heightMap;
        public readonly Color[] colorMap;

        public Map(HeightMap heightMap, Color[] colorMap)
        {
            this.heightMap = heightMap;
            this.colorMap = colorMap;
        }
    }
}
