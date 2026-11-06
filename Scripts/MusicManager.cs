using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance;

    [Header("Snapshots")]
    public AudioMixerSnapshot snapshotZona1;
    public AudioMixerSnapshot snapshotZona2;
    public AudioMixerSnapshot snapshotZona3;
    public float transitionTime = 2.5f;

    [Header("Audio Sources")]
    public AudioSource capa1;
    public AudioSource capa2;
    public AudioSource capa3;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (capa1 != null) capa1.Play();
        if (capa2 != null) capa2.Play();
        if (capa3 != null) capa3.Play();

        SwitchToZone(1);
    }

    public void SwitchToZone(int zoneNumber)
    {
        switch (zoneNumber)
        {
            case 1:
                snapshotZona1.TransitionTo(transitionTime);
                break;
            case 2:
                snapshotZona2.TransitionTo(transitionTime);
                break;
            case 3:
                snapshotZona3.TransitionTo(transitionTime);
                break;
        }
    }
}
