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

        /*
        public List<AssetGroupData<GameObject>> prefabs;
        public List<AssetGroupData<AudioClip>> audioClips;
        public List<AssetGroupData<ScriptableObject>> scriptableObjects;
        public List<AssetGroupData<Material>> materials;
        public List<AssetGroupData<Sprite>> sprites;
        public List<AssetGroupData<Font>> fonts;
        public List<AssetGroupData<AnimationClip>> animations;
        public List<AssetGroupData<Texture>> textures;
        public List<AssetGroupData<Shader>> shaders;
        public List<AssetGroupData<Mesh>> meshes;
        public List<AssetGroupData<SceneAsset>> scenes;
        */
    }
}
