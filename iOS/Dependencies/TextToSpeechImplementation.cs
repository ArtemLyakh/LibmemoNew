using System;
using AVFoundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(Libmemo.iOS.Dependencies.TextToSpeechImplementation))]
namespace Libmemo.iOS.Dependencies
{
    public class TextToSpeechImplementation : Libmemo.Dependencies.ITextToSpeech
    {
        private static AVSpeechSynthesizer _speachSynthesizer;

        private EventHandler onStoppedSpeaking = null;
        private void StoppedSpeakingHandler(object sender, AVSpeechSynthesizerUteranceEventArgs e)
        {
            onStoppedSpeaking?.Invoke(this, null);
        }
        event EventHandler Libmemo.Dependencies.ITextToSpeech.StoppedSpeaking
        {
            add
            {
                onStoppedSpeaking = value;
                SpeachSynthesizer.DidFinishSpeechUtterance += StoppedSpeakingHandler;
            }
            remove
            {
                onStoppedSpeaking = null;
                SpeachSynthesizer.DidFinishSpeechUtterance -= StoppedSpeakingHandler;
            }
        }

        private static AVSpeechSynthesizer SpeachSynthesizer
		{
            get {
                if (_speachSynthesizer == null) {
                    _speachSynthesizer = new AVSpeechSynthesizer();
                }
                    
                return _speachSynthesizer;
            }
        }

        public TextToSpeechImplementation() { }



        public void Speak(string text)
		{
			var speechUtterance = new AVSpeechUtterance(text)
			{
				Rate = AVSpeechUtterance.MaximumSpeechRate / 2,
				Voice = AVSpeechSynthesisVoice.FromLanguage("ru-Ru"),
				Volume = 0.75f,
				PitchMultiplier = 1.0f
			};

            SpeachSynthesizer.SpeakUtterance(speechUtterance);
		}

        public void Stop()
        {
            if (SpeachSynthesizer.Speaking)
                SpeachSynthesizer.StopSpeaking(new AVSpeechBoundary());
        }

    }
}
