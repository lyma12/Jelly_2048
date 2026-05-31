using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Watermelon.JellyMerge;
namespace ROOT.Scripts.Controllers
{
    public class ColorsController: SerializedMonoBehaviour
    {
        private static ColorsController instance;
        public static ColorsController Instance => instance;
        
        [SerializeField] private Dictionary<ColorId, Material> materials = new Dictionary<ColorId, Material>();
        public Material GetMaterial(ColorId id)
        {
            if (materials.TryGetValue(id, out Material mat))
            {
                return mat;
            }
            return null;
        }
    }
}