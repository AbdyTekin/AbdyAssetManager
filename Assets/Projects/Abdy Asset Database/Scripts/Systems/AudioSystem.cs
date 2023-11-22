using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AbdyManagement
{
    using static ResourceSystem;

    public static class AudioSystem
    {
        /// <summary>
        /// Plays Audioclips which are stored in ResourceManger at related category and returns AudioSource attached.
        /// </summary>
        public static AudioSource Play(string name, string category, float volume = 1f, float pitch = 1f, AudioSource audioSource = default, bool isOneShot = default, bool destroyAtFinish = default, Vector3 point = default, Transform parent = default)
        {
            List<AudiosWithCategory> _audios = ResourceSystem.Instance.Audios;
            if (_audios.Any(x => x.category == category) && _audios.Any(x => x.audios.Any(y => y.name == name)))
            {
                AudioClip _audioClip = _audios.Find(x => x.category == category).audios.Find(x => x.name == name).audio;

                if (audioSource == null) audioSource = AddObjectWithAudioSource(category, parent, point);
                audioSource.pitch = pitch;
                if (isOneShot) audioSource.PlayOneShot(_audioClip, volume);
                else
                {
                    audioSource.clip = _audioClip;
                    audioSource.volume = volume;
                    audioSource.Play();
                }
                if (destroyAtFinish) UnityEngine.Object.Destroy(audioSource.gameObject, _audioClip.length);
                return audioSource;
            }
            Debug.LogError("AudioClip named " + name + " not found at category: " + category + " !");
            return null;
        }

        /// <summary>
        /// Plays Audioclips which are stored in ResourceManger at related category and returns AudioSource attached.
        /// </summary>
        public static AudioSource Play<T>(T audioEnum, float volume = 1f, float pitch = 1f, AudioSource audioSource = default, bool isOneShot = default, bool destroyAtFinish = default, Vector3 point = default, Transform parent = default) where T : Enum, IConvertible
        {
            string _name = audioEnum.ToString();
            string _category = typeof(T).ToString();

            List<AudiosWithCategory> _audios = ResourceSystem.Instance.Audios;
            if (_audios.Any(x => x.category == _category) && _audios.Any(x => x.audios.Any(y => y.name == _name)))
            {
                AudioClip _audioClip = _audios.Find(x => x.category == _category).audios.Find(x => x.name == _name).audio;

                if (audioSource == null) audioSource = AddObjectWithAudioSource(_category, parent, point);
                audioSource.pitch = pitch;
                if (isOneShot) audioSource.PlayOneShot(_audioClip, volume);
                else
                {
                    audioSource.clip = _audioClip;
                    audioSource.volume = volume;
                    audioSource.Play();
                }
                if (destroyAtFinish) UnityEngine.Object.Destroy(audioSource.gameObject, _audioClip.length);
                return audioSource;
            }
            Debug.LogError("AudioClip named " + _name + " not found at category: " + _category + " !");
            return null;
        }

        private static AudioSource AddObjectWithAudioSource(string name, Transform parent, Vector3 point)
        {
            AudioSource audioSource = new GameObject(name).AddComponent<AudioSource>();
            audioSource.transform.parent = parent;
            audioSource.transform.localPosition = point;
            return audioSource;
        }
    }
}
