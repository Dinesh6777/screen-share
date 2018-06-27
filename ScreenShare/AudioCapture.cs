using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenShare
{
    class AudioCapture
    {
        WasapiLoopbackCapture capture;
        WaveFileWriter writer;
        private void AudioPlaybackCapture()
        {
            var outputFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "NAudio");
            Directory.CreateDirectory(outputFolder);
            var outputFilePath = Path.Combine(outputFolder, "recorded.wav");
            capture = new WasapiLoopbackCapture();
            writer = new WaveFileWriter(outputFilePath, capture.WaveFormat);
            capture.DataAvailable += Capture_DataAvailable;
            capture.RecordingStopped += Capture_RecordingStopped;
            capture.StartRecording();
        }

        private void Capture_RecordingStopped(object sender, StoppedEventArgs e)
        {
            writer.Dispose();
            writer = null;
            capture.Dispose();
        }

        private void Capture_DataAvailable(object sender, WaveInEventArgs e)
        {
            var buffer = e.Buffer;
            var cstream = new MemoryStream();

            cstream.Write(buffer, 0, e.BytesRecorded);
        }
    }
}
