using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AbdyManagement
{
    public class ResourceSystem : PersistentSingleton<ResourceSystem>
    {
        #region Resource Classes
        [Serializable]
        public class AudioWithName
        {
            public string name;
            public AudioClip audio;
        }
        [Serializable]
        public class AudiosWithCategory
        {
            public string category;
            public List<AudioWithName> audios;
        }

        [Serializable]
        public class VisualEffectWithName
        {
            public string name;
            public GameObject prefab;
        }
        [Serializable]
        public class VisualEffectsWithCategory
        {
            public string category;
            public List<PrefabWithName> prefabs;
        }

        [Serializable]
        public class PrefabWithName
        {
            public string name;
            public GameObject prefab;
        }
        [Serializable]
        public class PrefabsWithCategory
        {
            public string category;
            public List<PrefabWithName> prefabs;
        }

        [Serializable]
        public class MaterialWithName
        {
            public string name;
            public Material material;
        }
        [Serializable]
        public class MaterialsWithCategory
        {
            public string category;
            public List<MaterialWithName> materials;
        }
        #endregion

        public List<AudiosWithCategory> Audios;
        public List<VisualEffectsWithCategory> Visuals;
        public List<PrefabsWithCategory> Prefabs;
        public List<MaterialsWithCategory> Materials;

    }

#if UNITY_EDITOR
    public class ResourceSystemEditor : EditorWindow
    {
        // All code about editor window functionality
    }
#endif
}
