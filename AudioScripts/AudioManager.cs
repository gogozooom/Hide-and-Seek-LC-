using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Debug = Debugger.Debug;

namespace HideAndSeek.AudioScripts
{
    public static class AudioManager
    {
        public static bool LoadedAudio = false;

        public static List<AudioClip> audioClips;

        public static IEnumerator LoadAudioCoroutine()
        {
            audioClips = new();

            var dllFolderPath = Path.GetDirectoryName(Plugin.instance.Info.Location);
            var soundDirectory = Path.Combine(dllFolderPath, "Sounds");

            if (!Directory.Exists(soundDirectory))
            {
                Directory.CreateDirectory(soundDirectory);
            }

            var files = Directory.GetFiles(dllFolderPath, "*.wav", SearchOption.AllDirectories);

            Debug.LogWarning($"Now loading all sounds! Path = '{dllFolderPath}', Files found = '{files.Length}'");
            foreach (var fName in files)
            {
                string url = string.Format("file://{0}", fName);
                
                UnityWebRequest web = UnityWebRequestMultimedia.GetAudioClip(url, AudioType.WAV);

                yield return web.SendWebRequest();
                if (!web.isNetworkError && !web.isHttpError)
                {
                    var splitString = fName.Split("\\");
                    var fileName = splitString[splitString.Length - 1];
                    var soundName = fileName.Replace(".wav", "");

                    var clip = DownloadHandlerAudioClip.GetContent(web);
                    if (clip != null)
                    {
                        clip.name = soundName;

                        audioClips.Add(clip);

                        Debug.Log($"Loaded clip name '{clip.name}'");
                    }
                    File.Move(fName, Path.Combine(soundDirectory, fileName));
                }
            }
            Debug.LogWarning($"All Audio Loaded!");
            LoadedAudio = true;
        }
        public static AudioClip GetSound(string name, bool fullName = false, float random = -1f)
        {
            List<AudioClip> clipsFound = new();

            foreach (var clip in audioClips)
            {
                if (fullName)
                {
                    if (string.Equals(clip.name, name, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        clipsFound.Add(clip);
                    }
                }
                else // Adds randomness thingy
                {
                    if (string.Equals(clip.name.Split("-")[0], name, System.StringComparison.CurrentCultureIgnoreCase))
                    {
                        clipsFound.Add(clip);
                    }
                }
            }

            if (clipsFound.Count == 1)
            {
                return clipsFound[0];
            }
            else if(clipsFound.Count > 1)
            {
                if (random == -1f)
                {
                    random = Random.Range(0f, 1f);
                }
                random *= clipsFound.Count;

                if (random < 0)
                {
                    random = 0f;
                }

                int index = Mathf.FloorToInt(random);

                return clipsFound[index];
            }

            Debug.LogError($"AudioManager; GetSound({name}) Could not find audio clip!");
            return null;
        }

        public static OneTimeAudio PlaySound(string name, float volume = 1f, float pitch = 1f, Vector3 position = new(), float spatialBend = 0, float minDistance = 1, float maxDistance = 500, float random = -1f, bool isFullName = false, Transform parent = null)
        {
            AudioClip clip = GetSound(name, isFullName, random);

            return PlaySound(clip, volume, pitch, position, spatialBend, minDistance, maxDistance, parent);
        }
        public static OneTimeAudio PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f, Vector3 position = new(), float spatialBend = 0, float minDistance = 1, float maxDistance = 500, Transform parent = null)
        {
            GameObject oneTimeUse = new($"Playing({clip.name}) ONCE");
            if (parent != null)
                oneTimeUse.transform.parent = parent;

            OneTimeAudio oTAudio = oneTimeUse.AddComponent<OneTimeAudio>();

            oTAudio.PlayAudioClip(clip, volume, pitch, position, spatialBend, minDistance, maxDistance);

            return oTAudio;           
        }
    }
}
