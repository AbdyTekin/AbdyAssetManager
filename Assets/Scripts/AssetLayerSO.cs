using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace AbdyManagement
{
    [CreateAssetMenu(menuName = "AssetData")]
    public class AssetLayerSO : ScriptableObject
    {
        // Remove Serializables on final

        [System.Serializable]
        public class AssetData<T>
        {
            public string name;
            public T asset;
        }

        [System.Serializable]
        public class AssetListData<T>
        {
            public string groupName;
            public List<AssetData<T>> data;
        }

        public List<SceneAsset> sceneLayerMask;

        public List<AssetListData<GameObject>> prefabs;
        public List<AssetListData<AudioClip>> audioClips;
        public List<AssetListData<ScriptableObject>> scriptableObjects;
        public List<AssetListData<Material>> materials;
        public List<AssetListData<Sprite>> sprites;
        public List<AssetListData<Font>> fonts;
        public List<AssetListData<AnimationClip>> animations;
        public List<AssetListData<Texture>> textures;
        public List<AssetListData<Shader>> shaders;
        public List<AssetListData<Mesh>> meshes;
        public List<AssetListData<SceneAsset>> scenes;
    }
}
