using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 설정 팝업. 시작 메뉴 위에도, 업적 화면 위에도 겹칠 수 있다.
/// 값 자체는 SettingsData 가 들고 있고, 이 팝업은 보여주고 수정할 뿐이다.
/// </summary>
public class SettingsPopup : Popup
{
    [Header("Controls")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button closeButton;

    protected override void Awake()
    {
        base.Awake();

        bgmSlider.onValueChanged.AddListener(v => SettingsData.BgmVolume = v);
        sfxSlider.onValueChanged.AddListener(v => SettingsData.SfxVolume = v);
        fullscreenToggle.onValueChanged.AddListener(v => Screen.fullScreen = v);

        closeButton.onClick.AddListener(() => PopupManager.Instance.CloseTop());
    }

    public override void Open()
    {
        base.Open();

        // .value = x 로 넣으면 onValueChanged 가 발화해서
        // 여는 순간 저장값이 덮어써지거나 사운드가 끊긴다. 반드시 WithoutNotify.
        bgmSlider.SetValueWithoutNotify(SettingsData.BgmVolume);
        sfxSlider.SetValueWithoutNotify(SettingsData.SfxVolume);
        fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
    }

    public override void Close(System.Action onComplete = null)
    {
        PlayerPrefs.Save();
        base.Close(onComplete);
    }
}

public static class SettingsData
{
    private const string BgmKey = "settings.bgm";
    private const string SfxKey = "settings.sfx";

    public static float BgmVolume
    {
        get => PlayerPrefs.GetFloat(BgmKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(BgmKey, value);
            // AudioManager.ApplyBgm(value);
        }
    }

    public static float SfxVolume
    {
        get => PlayerPrefs.GetFloat(SfxKey, 1f);
        set
        {
            PlayerPrefs.SetFloat(SfxKey, value);
            // AudioManager.ApplySfx(value);
        }
    }
}