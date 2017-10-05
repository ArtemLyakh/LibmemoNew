using System;
using Xamarin.Forms;

namespace Libmemo.Helpers
{
    public class TextToSpeech
    {
        private static TextToSpeech _current;
        public static TextToSpeech Current {
            get {
                if (_current == null)
                    _current = new TextToSpeech();

                return _current;
            }
        }

        private Dependencies.ITextToSpeech tts;
        private TextToSpeech()
        {
            tts = DependencyService.Get<Dependencies.ITextToSpeech>();
            tts.StoppedSpeaking += (sender, e) => TTSStopped?.Invoke(this, null);
        }


        public void Speak(string text)
        {
            tts.Speak(text);
        }

        public void Stop()
        {
            tts.Stop();
        }

        public event EventHandler TTSStopped;
    }
}
