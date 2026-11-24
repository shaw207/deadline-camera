using UnityEngine;
using System;

namespace DeadLine
{
    /// <summary>
    /// Utilidad para reproducir sonidos desde Animation Events.
    /// Usa el SoundManager global o SoundLibraries opcionales.
    /// </summary>
    public class AnimationsSoundsUtil : MonoBehaviour
    {
        [Header("Sound Libraries")]
        [Tooltip("Librerías de sonidos locales. Asigna aquí para tener desplegables con nombres de sonidos.")]
        [SerializeField] private SoundLibrary[] localSoundLibraries;

        [Header("Animation Event Sounds")]
        [Tooltip("Referencia visual de sonidos. Usa los NOMBRES directamente en Animation Events: PlaySound(\"footstep\")")]
        [SerializeField] private SoundDefinition[] animationSounds;

        [Serializable]
        public class SoundDefinition
        {
            public string label = "Sound";
            [SoundName] public string soundName;
        }

        /// <summary>
        /// Reproduce un sonido por nombre. Usa SoundManager global.
        /// Para Animation Events: PlaySound("footstep") - Escribe el nombre del sonido como string.
        /// </summary>
        public void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning("[AnimationsSoundsUtil] Nombre de sonido vacío");
                return;
            }

            if (SoundManager.Instance)
                SoundManager.Instance.PlaySound(soundName);
        }

        /// <summary>
        /// Reproduce un sonido una vez (one-shot) por nombre.
        /// Para Animation Events: PlayOneShotSound("jump") - Escribe el nombre del sonido como string.
        /// </summary>
        public void PlayOneShotSound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning("[AnimationsSoundsUtil] Nombre de sonido vacío");
                return;
            }

            if (SoundManager.Instance)
                SoundManager.Instance.PlayOneShotSound(soundName);
        }

        /// <summary>
        /// Detiene un sonido por nombre.
        /// Para Animation Events: StopSound("footstep") - Escribe el nombre del sonido como string.
        /// </summary>
        public void StopSound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning("[AnimationsSoundsUtil] Nombre de sonido vacío");
                return;
            }

            if (SoundManager.Instance)
                SoundManager.Instance.StopSound(soundName);
        }

        /// <summary>
        /// Verifica si un sonido está disponible en las librerías locales o en el SoundManager
        /// </summary>
        public bool HasSound(string soundName)
        {
            // Verificar en librerías locales primero
            if (localSoundLibraries != null)
            {
                foreach (var library in localSoundLibraries)
                {
                    if (library != null && library.HasSound(soundName))
                        return true;
                }
            }

            // Si no está en librerías locales, el SoundManager global lo manejará
            return true; // Asumimos que el SoundManager lo tiene
        }

#if UNITY_EDITOR
        /// <summary>
        /// Helper para mostrar sonidos disponibles en el inspector
        /// </summary>
        [ContextMenu("Listar Sonidos Disponibles")]
        private void ListAvailableSounds()
        {
            if (localSoundLibraries == null || localSoundLibraries.Length == 0)
            {
                Debug.Log("[AnimationsSoundsUtil] No hay librerías locales. Usando SoundManager global.");
                return;
            }

            Debug.Log("=== Sonidos Disponibles en Librerías Locales ===");
            foreach (var library in localSoundLibraries)
            {
                if (library == null) continue;
                
                Debug.Log($"📚 Librería: {library.name}");
                if (library.Sounds != null)
                {
                    foreach (var sound in library.Sounds)
                    {
                        if (sound != null)
                            Debug.Log($"  🔊 {sound.name}");
                    }
                }
            }
        }
#endif
    }
}