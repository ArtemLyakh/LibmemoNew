using System;
namespace Libmemo.Dependencies
{
    public interface ITextToSpeech
    {
        void Speak(string text);
        void Stop();
        event EventHandler StoppedSpeaking;
    }
}
