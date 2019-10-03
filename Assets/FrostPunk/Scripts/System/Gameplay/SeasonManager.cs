using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SeasonManager : Singleton<SeasonManager>, ISaveable {
   public enum Season { Summer, Winter}

   [SerializeField]
   public Season season;

   private Season prevSeason;

   [SerializeField]
   private Color summerColor;

   [SerializeField]
   private Color winterColor;

   [SerializeField]
   private AnimationCurve seasonCurve;

   [SerializeField]
   private List<Material> materials;

   private Terrain terrain;

   private void Start() {
      terrain = FindObjectOfType<Terrain>();
      Application.targetFrameRate = 60;
   }

   private void Update() {
      if (DayCycleManager.GetInstance().Day > 15) {
         season = Season.Winter;
      } else {
         season = Season.Summer;
      }
      UpdateSeason();
   }

   private void OnValidate() {
      UpdateSeason();
   }

   private void UpdateSeason() {
      if (season != prevSeason && terrain != null) {

         if (DayCycleManager.GetInstance().Day / 10 % 2 != 0) {
            season = Season.Winter;
         } else {
            season = Season.Summer;
         }

         if (season == Season.Summer) {
            materials.ForEach(x => x.color = summerColor);
            UpdateTerrainTexture(terrain.terrainData, 2, 0);
            UpdateTerrainTexture(terrain.terrainData, 3, 1);
         } else {
            materials.ForEach(x => x.color = winterColor);
            UpdateTerrainTexture(terrain.terrainData, 0, 2);
            UpdateTerrainTexture(terrain.terrainData, 1, 3);
         }
         prevSeason = season;
      }
   }

   private void UpdateTerrainTexture(TerrainData terrainData, int textureNumberFrom, int textureNumberTo) {
      //get current paint mask
      float[,,] alphas = terrainData.GetAlphamaps(0, 0, terrainData.alphamapWidth, terrainData.alphamapHeight);
      // make sure every grid on the terrain is modified
      for (int i = 0; i < terrainData.alphamapWidth; i++) {
         for (int j = 0; j < terrainData.alphamapHeight; j++) {
            //for each point of mask do:
            //paint all from old texture to new texture (saving already painted in new texture)
            alphas[i, j, textureNumberTo] = Mathf.Max(alphas[i, j, textureNumberFrom], alphas[i, j, textureNumberTo]);
            //set old texture mask to zero
            alphas[i, j, textureNumberFrom] = 0f;
         }
      }
      // apply the new alpha
      terrainData.SetAlphamaps(0, 0, alphas);
   }

   public object OnSave() {
      var data = new Dictionary<string, object>();
      data["season"] = season;
      return data;
   }

   public void OnLoad(object data) {
      var savedData = (Dictionary<string, object>)data;
      season = (Season)savedData["season"];
   }

   public void OnLoadDependencies(object data) {
      // Ignored
   }
}
