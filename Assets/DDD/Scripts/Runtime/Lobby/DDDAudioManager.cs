using UnityEngine;

namespace DDD.Scripts.Lobby
{
    public class DDDAudioManager : MonoBehaviour
    {
        public static DDDAudioManager instance;

        [Header("Sound Effects")]
        public AudioClip buttonPressClip;
        public AudioClip winClip;
        public AudioClip loseClip;
        public AudioClip drawClip;

        [Header("Background Music")]
        public AudioClip backgroundMusicClip;
        public bool loopMusic = true;
        [Range(0f, 1f)]
        public float musicVolume = 0.5f;

        private AudioSource musicSource;
        private AudioSource sfxSource;

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);

                musicSource = gameObject.AddComponent<AudioSource>();
                sfxSource = gameObject.AddComponent<AudioSource>();

                musicSource.loop = loopMusic;
                musicSource.volume = musicVolume;
                if (backgroundMusicClip != null)
                {
                    musicSource.clip = backgroundMusicClip;
                    musicSource.Play();
                }
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlayButtonPressSound()
        {
            if (buttonPressClip != null)
                sfxSource.PlayOneShot(buttonPressClip);
        }

        public void PlayWinSound()
        {
            if (winClip != null)
                sfxSource.PlayOneShot(winClip);
        }

        public void PlayLoseSound()
        {
            if (loseClip != null)
                sfxSource.PlayOneShot(loseClip);
        }

        public void PlayDrawSound()
        {
            if (drawClip != null)
                sfxSource.PlayOneShot(drawClip);
        }
    }
}