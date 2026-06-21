using ETS2LA.Audio;
using Avalonia.Controls;
using System.IO;
using System.Threading.Tasks;

namespace ETS2LA.UI.Views.Settings;

public partial class AudioSettings : UserControl
{
    private AudioHandler _audioHandler;
    private bool _soundPlaying = false;
    private bool _skipFirstPlay = true;

    public AudioSettings()
    {
        InitializeComponent();
        _audioHandler = AudioHandler.Current;
        VolumeSlider.Value = _audioHandler.GetVolume() * 100.0;
    }

    private async void OnVolumeChanged(object? sender, Avalonia.Controls.Primitives.RangeBaseValueChangedEventArgs e)
    {
        float volume = (float)(e.NewValue / 100.0);
        _audioHandler.SetVolume(volume);

        if (_skipFirstPlay)
        {
            _skipFirstPlay = false;
            return;
        }

        if (_soundPlaying)
            return;

        _soundPlaying = true;

        var engageSound = Path.Combine(AppContext.BaseDirectory, "Assets", "Sounds", "engage.mp3");
        if (File.Exists(engageSound))
        {
            _audioHandler.Queue(engageSound, overrideCurrent: true);
        }

        await Task.Delay(1500);
        _soundPlaying = false;
    }
}