using System.Collections.Generic;
using UnityEngine;

namespace Watermelon
{
    [CreateAssetMenu(fileName = "Audio Clips", menuName = "Data/Core/Audio Clips")]
    public class AudioClips : ScriptableObject
    {
        [BoxGroup("UI", "UI")]
        public AudioClip buttonSound;

        [BoxGroup("Gameplay", "Gameplay")]
        public AudioClip swipeClip;
        [BoxGroup("Gameplay")]
        public AudioClip squishClip;
        [BoxGroup("Gameplay")]
        public AudioClip gameWinClip;
    }
}

// -----------------
// Audio Controller v 0.4
// -----------------