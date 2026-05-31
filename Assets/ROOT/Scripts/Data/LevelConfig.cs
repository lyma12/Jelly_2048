using System;
using System.Collections.Generic;
using UnityEngine;
using Watermelon.JellyMerge;
namespace ROOT.Scripts.Data
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Data/LevelConfig")]
    public class LevelConfig: ScriptableObject
    {
        public List<LevelConfigData> LevelConfigDatas = new List<LevelConfigData>();
    }
    [Serializable]
    public class LevelConfigData
    {
        public int levelScore;
        public SpawnDifficulty spawnRule;
        public List<ColorSpawnData> colorWeight;
    }
    public enum SpawnDifficulty : byte
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
    }
    [Serializable]
    public class ColorSpawnData
    {
        public ColorId ColorId;
        public int Weight;
    }
}