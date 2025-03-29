using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

public class Mp3Player
{
    private IWavePlayer _waveOutDevice;
    private AudioFileReader _audioFileReader;

    public float Volume
    {
        get => _audioFileReader.Volume;
        set => _audioFileReader.Volume = value;
    }

    public Mp3Player(string filePath)
    {
        _waveOutDevice = new WaveOutEvent();
        _audioFileReader = new AudioFileReader(filePath);
        _waveOutDevice.Init(_audioFileReader);
    }

    public void Play()
    {
        _waveOutDevice.Play();
    }

    public void Stop()
    {
        _waveOutDevice.Stop();
    }

    public void Dispose()
    {
        _waveOutDevice.Dispose();
        _audioFileReader.Dispose();
    }
}
