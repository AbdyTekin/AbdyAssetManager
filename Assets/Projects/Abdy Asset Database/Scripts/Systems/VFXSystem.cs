using UnityEngine;
using System;
using UnityEngine.VFX;
using System.Collections.Generic;
using System.Linq;

namespace EasyResourceSystem
{
    using static ResourceSystem;

    public static class VFXSystem
    {
        /// <summary>
        /// Plays ParticleSystems which are restored in ResourceManger and returns GameObject attached.
        /// </summary>
        public static GameObject Play(string name, string category, GameObject gameObject = null, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool clearBeforePlay = false, bool destroyAtFinish = false, float vfxDestroyTime = 1f)
        {
            List<VisualEffectsWithCategory> _visuals = ResourceSystem.Instance.Visuals;
            if (_visuals.Any(x => x.category == category) && _visuals.Any(x => x.prefabs.Any(y => y.name == name)))
            {
                GameObject _prefab = _visuals.Find(x => x.category == category).prefabs.Find(x => x.name == name).prefab;
                
                if (!gameObject)
                {
                    gameObject = GameObject.Instantiate(_prefab, position, rotation, parent);
                }
                bool hasPS = gameObject.TryGetComponent(out ParticleSystem ps);
                bool hasVFX = gameObject.TryGetComponent(out VisualEffect vfx);
                if (hasPS || hasVFX)
                {
                    if (hasPS)
                    {
                        if (clearBeforePlay) ps.Clear();
                        ps.Play();
                        if (destroyAtFinish) GameObject.Destroy(gameObject, ps.main.duration);
                        return gameObject;
                    }
                    if (hasVFX)
                    {
                        if (clearBeforePlay) vfx.Reinit();
                        else vfx.Play();
                        if (destroyAtFinish) GameObject.Destroy(gameObject, vfxDestroyTime);
                        return gameObject;
                    }
                }
                else
                {
                    Debug.LogError(gameObject.name + " has no ParticleSystem component attached!");
                }
            }
            Debug.Log("Prefab named " + name + " not found at category: " + category + " !");
            return null;
        }
        public static GameObject Play<T>(T fxEnum, GameObject gameObject = null, Vector3 position = default, Quaternion rotation = default, Transform parent = null, bool clearBeforePlay = false, bool destroyAtFinish = false, float vfxDestroyTime = 1f) where T : Enum, IConvertible
        {
            string _name = fxEnum.ToString();
            string _category = typeof(T).ToString();

            List<VisualEffectsWithCategory> _visuals = ResourceSystem.Instance.Visuals;
            if (_visuals.Any(x => x.category == _category) && _visuals.Any(x => x.prefabs.Any(y => y.name == _name)))
            {
                GameObject _prefab = _visuals.Find(x => x.category == _category).prefabs.Find(x => x.name == _name).prefab;

                if (!gameObject)
                {
                    gameObject = GameObject.Instantiate(_prefab, position, rotation, parent);
                }
                bool hasPS = gameObject.TryGetComponent(out ParticleSystem ps);
                bool hasVFX = gameObject.TryGetComponent(out VisualEffect vfx);
                if (hasPS || hasVFX)
                {
                    if (hasPS)
                    {
                        if (clearBeforePlay) ps.Clear();
                        ps.Play();
                        if (destroyAtFinish) GameObject.Destroy(gameObject, ps.main.duration);
                        return gameObject;
                    }
                    if (hasVFX)
                    {
                        if (clearBeforePlay) vfx.Reinit();
                        else vfx.Play();
                        if (destroyAtFinish) GameObject.Destroy(gameObject, vfxDestroyTime);
                        return gameObject;
                    }
                }
                else
                {
                    Debug.LogError(gameObject.name + " has no ParticleSystem component attached!");
                }
            }
            Debug.Log("Prefab named " + _name + " not found at category: " + _category + " !");
            return null;
        }
    }

}

