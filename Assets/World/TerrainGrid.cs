using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

namespace World
{

    public class TerrainGrid : MonoBehaviour
    {

        public const float maxViewDistance = 600;
        
        public Transform viewer;

        public Material material;

        private Vector2 viewerPosition;
        
        private int chunkSize;
        private int maxChunksVisible;

        private Dictionary<Vector2, TerrainChunk> chunks = new Dictionary<Vector2, TerrainChunk>();

        private List<TerrainChunk> lastVisibleChunks = new List<TerrainChunk>();

        private MapGenerator mapGenerator;

        // Start is called before the first frame update
        void Start()
        {
            mapGenerator = FindObjectOfType<MapGenerator>();
            chunkSize = MapGenerator.ChunkSize - 1; // todo remove dependency on MapGenerator
            maxChunksVisible = Mathf.RoundToInt(maxViewDistance / chunkSize); 
        }

         // Update is called once per frame
        void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            updateVisibleChunks();
        }

        private void updateVisibleChunks()
        {
            foreach (TerrainChunk chunk in lastVisibleChunks)
            {
                chunk.Hide();
            }
            lastVisibleChunks.Clear();

            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int y = -maxChunksVisible; y <= maxChunksVisible; y++)
            {
                for (int x = -maxChunksVisible; x <= maxChunksVisible; x++)
                {
                    Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + x, currentChunkCoordY + y);
                    if (chunks.ContainsKey(viewedChunkCoord))
                    {
                        onAlreadyExistingChunk(chunks[viewedChunkCoord]);
                    }
                    else
                    {
                        onNewChunk(viewedChunkCoord);
                    }
                    var chunk = chunks[viewedChunkCoord];
                    _ = chunk.Update(mapGenerator, resolveLevelOfDetailForChunk(chunk));
                }
            }
        }

        private int resolveLevelOfDetailForChunk(TerrainChunk chunk)
        {
            return Mathf.FloorToInt((chunk.GetDistanceFrom(viewerPosition) / maxViewDistance) * MapGenerator.LevelsOfDetail);
        }

        private void onAlreadyExistingChunk(TerrainChunk chunk)
        {
            if (chunk.GetDistanceFrom(viewerPosition) <= maxViewDistance)
            {
                chunk.Show();
                lastVisibleChunks.Add(chunk);
            }
            else
            {
                chunk.Hide();
            }
        }

        private void onNewChunk(Vector2 chunkCoord)
        {
            var chunk = new TerrainChunk(chunkCoord, chunkSize, material);
            chunks.Add(chunkCoord, chunk);
        }
    }

    public class TerrainChunk 
    {
        private Vector2 coord;
        private GameObject mesh;
        private Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        
        private LodMesh[] meshes = new LodMesh[MapGenerator.LevelsOfDetail];

        public TerrainChunk(Vector2 coord, int size, Material material)
        {
            this.coord = coord;
            bounds = new Bounds(coord * size, Vector2.one);
            mesh = new GameObject(string.Format("TerrainChunk_{0}_{1}", coord.x, coord.y));
            meshRenderer = mesh.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            meshFilter = mesh.AddComponent<MeshFilter>();
            mesh.transform.position = new Vector3(Position.x, 0, Position.y);
            Hide();
        }

        public async Task Update(MapGenerator generator, int levelOfDetail)
        {
            if (IsVisible) {
                LodMesh mesh = meshes[levelOfDetail];
                if (mesh == null)
                {
                    meshes[levelOfDetail] = new LodMesh(levelOfDetail);
                    Debug.Log(string.Format("Chunk [{0}, {1}, {2}] Creating mesh", coord.x, coord.y, levelOfDetail));
                    meshes[levelOfDetail] = await createMesh(generator, levelOfDetail);
                    Debug.Log(string.Format("Chunk [{0}, {1}, {2}] Mesh created", coord.x, coord.y, levelOfDetail));
                    mesh = meshes[levelOfDetail];
                }
                else 
                {
                    applyMesh(mesh);
                }
            }
            
        }

        private async Task<LodMesh> createMesh(MapGenerator generator, int levelOfDetail)
        {
            (Map, MeshBuilder) result = await Task.Run(() =>
            {
                Map map = generator.GenerateMap(coord);
                MeshBuilder builder = TerrainMeshGenerator.GenerateFlatTerrainMesh(map.heightMap, levelOfDetail);
                return (map, builder);
            });

            var texture = new Texture2D(result.Item1.heightMap.Width, result.Item1.heightMap.Height);
            texture.SetPixels(result.Item1.colorMap);
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Point; // makes changes between regions more rough
            texture.Apply();

            LodMesh mesh = new LodMesh(levelOfDetail);
            mesh.Load(result.Item2.Build(), texture);

            return mesh;            
        }

        private void applyMesh(LodMesh mesh)
        {
            if (mesh.IsLoaded) {
                meshFilter.mesh = mesh.Mesh;
                meshRenderer.material.mainTexture = mesh.Texture;
            }
        }

        public float GetDistanceFrom(Vector2 position)
        {
            return Mathf.Sqrt(bounds.SqrDistance(position));
        }

        public Vector2 Position
        {
            get { return bounds.center; }
        }

        public bool IsVisible
        {
            get { return mesh.activeSelf; }
        }

        public void Show()
        {
            mesh.SetActive(true);
        }

        public void Hide()
        {
            mesh.SetActive(false);
        }
    }
    
    class LodMesh
    {
        private int lod;
        private Mesh mesh;
        private Texture2D texture;

        public LodMesh(int lod)
        {
            this.lod = lod;
        }

        public void Load(Mesh mesh, Texture2D texture)
        {
            this.mesh = mesh;
            this.texture = texture;
        }

        public bool IsLoaded
        {
            get { return mesh != null && texture != null; }
        }

        public Texture2D Texture
        {
            get { return texture; }
        }

        public Mesh Mesh
        {
            get { return mesh; }
        }
    }
}
