using UnityEngine;

namespace Maze
{
    public class SoundManager : MonoBehaviour
    {
        public AudioSource OneShotSource;
        public GameObject IngameMusic;
        public AudioClip wallHit;

        public void PlayHitWall()
        {
            OneShotSource.PlayOneShot(wallHit);
        }
        
        public void PlayEffect(AudioClip clip)
        {
            OneShotSource.PlayOneShot(clip);
        }

        public void PlayIngameMusic(bool isActive)
        {
            IngameMusic.gameObject.SetActive(isActive);
        }
    }
}