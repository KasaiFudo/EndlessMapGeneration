using UnityEngine;

public class TerrainChunk
{
    const float colliderGenerationDistanceThreashold = 5;
    public event System.Action<TerrainChunk, bool> onVisabilityChanged;
    public Vector2 coord;

    GameObject meshObject;
    Vector2 sampleCenter;
    Bounds bounds;


    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    MeshCollider meshCollider;

    LODInfo[] detailLevels;
    LODMesh[] lodMeshes;
    int colliderLODindex;

    HeightMap heightMap;
    bool heightMapReceieved;
    int previousLODIndex = -1;
    bool hasSetCollider;
    float maxViewDst;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    Vector2 ViewerPosition
    {
        get { return new Vector2(viewer.position.x, viewer.position.z); }
    }

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODindex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODindex = colliderLODindex;
        this.meshSettings = meshSettings;
        this.heightMapSettings = heightMapSettings;
        this.viewer = viewer;

        sampleCenter = coord * meshSettings.MeshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.MeshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.MeshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();
        meshRenderer.material = material;

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODindex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMash;
            }
        }
        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshhold;

    }
    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.NumVertsPerLine, meshSettings.NumVertsPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
    }
    void OnHeightMapReceived(object heightMapObject)
    {
        this.heightMap = (HeightMap)heightMapObject;
        heightMapReceieved = true;

        UpdateTerrainChunk();
    }
    public void UpdateTerrainChunk()
    {
        if (heightMapReceieved)
        {
            float viewerDstFroNearestEdge = Mathf.Sqrt(bounds.SqrDistance(ViewerPosition));
            bool wasVisible = IsVisible();
            bool visible = viewerDstFroNearestEdge <= maxViewDst;

            if (visible)
            {
                int lodIndex = 0;
                for (int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if (viewerDstFroNearestEdge > detailLevels[i].visibleDstThreshhold)
                    {
                        lodIndex = i + 1;
                    }
                    else
                        break;
                }
                if (lodIndex != previousLODIndex)
                {
                    LODMesh lodMesh = lodMeshes[lodIndex];
                    if (lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    }
                    else if (!lodMesh.hasRequestedMesh)
                        lodMesh.RequestMesh(heightMap, meshSettings);
                }

            }
            if (wasVisible != visible)
            {
                SetVisible(visible);
                if(onVisabilityChanged != null)
                    onVisabilityChanged(this, visible);
            }
        }
    }

    public void UpdateCollisionMash()
    {
        if (!hasSetCollider)
        {
            float sqrDstFromViewerToEdge = bounds.SqrDistance(ViewerPosition);

            if (sqrDstFromViewerToEdge < detailLevels[colliderLODindex].SqrVisibleDstThreashold)
            {
                if (!lodMeshes[colliderLODindex].hasRequestedMesh)
                    lodMeshes[colliderLODindex].RequestMesh(heightMap, meshSettings);
            }

            if (sqrDstFromViewerToEdge < colliderGenerationDistanceThreashold * colliderGenerationDistanceThreashold)
            {
                if (lodMeshes[colliderLODindex].hasMesh)
                {
                    meshCollider.sharedMesh = lodMeshes[colliderLODindex].mesh;
                    hasSetCollider = true;
                }
            }
        }
    }
    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }
    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }
}

class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    int lod;
    public event System.Action updateCallback;
    public LODMesh(int lod)
    {
        this.lod = lod;
    }
    void OnMeshDataReceived(object meshDataObject)
    {
        mesh = ((MeshData)meshDataObject).CreateMesh();
        hasMesh = true;

        updateCallback();
    }
    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        ThreadedDataRequester.RequestData(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), OnMeshDataReceived);
    }
}