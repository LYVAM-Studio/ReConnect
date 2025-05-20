using UnityEngine;
using UnityEngine.UI;
using Reconnect.Audio;

public class AudioSettingsUI : MonoBehaviour
{
    [Header("Sliders")]
    public Slider interfaceSlider;
    public Slider ambianceSlider;
    public Slider movementSlider;
    public Slider musicSlider;

    private void Start()
    {
        interfaceSlider.value = AudioManager.Instance.interfaceVolume;
        ambianceSlider.value = AudioManager.Instance.ambianceVolume;
        movementSlider.value = AudioManager.Instance.movementVolume;
        musicSlider.value = AudioManager.Instance.musicVolume;

        interfaceSlider.onValueChanged.AddListener(AudioManager.Instance.SetInterfaceVolume);
        ambianceSlider.onValueChanged.AddListener(AudioManager.Instance.SetAmbianceVolume);
        movementSlider.onValueChanged.AddListener(AudioManager.Instance.SetMovementVolume);
        musicSlider.onValueChanged.AddListener(AudioManager.Instance.SetMusicVolume);
    }
}
