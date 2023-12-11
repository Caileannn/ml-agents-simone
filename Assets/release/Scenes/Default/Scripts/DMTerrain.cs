using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DMTerrain : MonoBehaviour
{
    Terrain terrain;
    
    public int posXInTerrain;
    public int posYInTerrain;
    float[,] _heights;
    float[,] _rowHeight;

    public int heightIndex;
    public float curHeight;

    internal const float _minHeight = 0f;
    internal const float _maxHeight = 10f;
    internal const float _minSpawnHeight = 0f;//2f;
    internal const float _maxSpawnHeight = 10f;//8f;
    const float _midHeight = 5f;
    float _mapScaleY;
    float[,] _heightMap;
    public void Reset()
    {
        if (this.terrain == null)
        {
            var parent = gameObject.transform.parent;
            terrain = parent.GetComponentInChildren<Terrain>();
            var sharedTerrainData = terrain.terrainData;
            terrain.terrainData = new TerrainData();
            terrain.terrainData.heightmapResolution = sharedTerrainData.heightmapResolution;
            terrain.terrainData.baseMapResolution = sharedTerrainData.baseMapResolution;
            terrain.terrainData.SetDetailResolution(sharedTerrainData.detailResolution, sharedTerrainData.detailResolutionPerPatch);
            terrain.terrainData.size = sharedTerrainData.size;
            // terrain.terrainData.splatPrototypes = sharedTerrainData.splatPrototypes;
            terrain.terrainData.terrainLayers = sharedTerrainData.terrainLayers;
            var collider = terrain.GetComponent<TerrainCollider>();
            collider.terrainData = terrain.terrainData;
            _rowHeight = new float[terrain.terrainData.heightmapResolution, 1];
        }

        
        
        
        _mapScaleY = this.terrain.terrainData.heightmapScale.y;

        // get the normalized position of this game object relative to the terrain
        Vector3 tempCoord = (transform.position - terrain.gameObject.transform.position);
        Vector3 coord;

        tempCoord.x = Mathf.Clamp(tempCoord.x, 0f, terrain.terrainData.size.x - 0.000001f);
        tempCoord.z = Mathf.Clamp(tempCoord.z, 0f, terrain.terrainData.size.z - 0.000001f);
        coord.x = (tempCoord.x - 1) / terrain.terrainData.size.x;
        coord.y = tempCoord.y / terrain.terrainData.size.y;
        coord.z = tempCoord.z / terrain.terrainData.size.z;

        // get the position of the terrain heightmap where this game object is
        posXInTerrain = (int)(coord.x * terrain.terrainData.heightmapResolution);
        posYInTerrain = (int)(coord.z * terrain.terrainData.heightmapResolution);
        // we set an offset so that all the raising terrain is under this game object
        int offset = 1 / 2;
        //Debug.Log(posXInTerrain + " : " + posYInTerrain);
        // get the heights of the terrain under this game object
        _heights = terrain.terrainData.GetHeights(posXInTerrain - offset, posYInTerrain - offset, 100, 1);
        curHeight = _midHeight;
        heightIndex = posXInTerrain;

        ResetHeights();
    }
    void ResetHeights()
    {
        if (_heightMap == null)
        {
            _heightMap = new float[terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution];
        }
        heightIndex = 0;
        while (heightIndex < posXInTerrain)
            SetNextHeight(0);

        SetNextHeight(0);
        SetNextHeight(0);
        SetNextHeight(0);
        SetNextHeight(0);
        SetNextHeight(0);
        SetNextHeight(0);
        while (heightIndex < terrain.terrainData.heightmapResolution)
        {
            int action = Random.Range(0, 10);
            try
            {
                SetNextHeight(action);
            }
            catch (System.Exception)
            {
                SetNextHeight(action);
                throw;
            }
        }
        this.terrain.terrainData.SetHeights(0, 0, _heightMap);

    }
    void SetNextHeight(int action)
    {
        float actionSize = 0f;
        bool actionPos = (action - 1) % 2 == 0;
        if (action != 0)
        {
            actionSize = ((float)((action + 1) / 2)) * 0.1f;
            curHeight += actionPos ? actionSize : -actionSize;
            if (curHeight < _minSpawnHeight)
            {
                curHeight = _minSpawnHeight;
                actionSize = 0;
            }
            if (curHeight > _maxSpawnHeight)
            {
                curHeight = _maxSpawnHeight;
                actionSize = 0;
            }
        }

        float height = curHeight / _mapScaleY;
        // var unit = terrain.terrainData.heightmapWidth / (int)_mapScaleY;
        int unit = 1;
        int startH = heightIndex * unit;
        for (int h = startH; h < startH + unit; h++)
        {
            for (int w = 0; w < terrain.terrainData.heightmapResolution; w++)
            {
                _heightMap[w, h] = height;
            }
            height += 1 / 300f / _mapScaleY;
        }
        heightIndex++;
    }
}
