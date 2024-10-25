using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AbdyManagement
{
    [CreateAssetMenu(menuName = "AssetData")]
    public class AssetLayerSO : ScriptableObject
    {
        [System.Serializable]
        public class AssetData<T>
        {
            public string name;
            public T asset;
        }

        [System.Serializable]
        public class AssetGroupData<T>
        {
            public string groupName;
            public List<AssetData<T>> assets;
        }

        public List<SceneAsset> sceneLayerMask;

        public List<AssetGroupData<Object>> groups;

    }
}
