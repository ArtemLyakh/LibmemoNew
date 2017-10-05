using System;
using Android.Runtime;
using Android.Speech.Tts;
using Libmemo.Dependencies;
using Xamarin.Forms;

[assembly: Dependency(typeof(Libmemo.Droid.Dependencies.TextToSpeechImplementation))]
namespace Libmemo.Droid.Dependencies
{
	public class TextToSpeechImplementation : Java.Lang.Object, ITextToSpeech, TextToSpeech.IOnInitListener
	{
		TextToSpeech speaker;
		string toSpeak;
		string toId;
		TTSListener listener;

        public event EventHandler StoppedSpeaking;

        public TextToSpeechImplementation()
		{
			var ctx = Forms.Context; // useful for many Android SDK features
			speaker = new TextToSpeech(ctx, this);
			speaker.SetLanguage(new Java.Util.Locale("ru-RU"));

			listener = new TTSListener();
            listener.Done += (s, e) => { this.StoppedSpeaking?.Invoke(this, null); };
			listener.Stop += (s, e) => { this.StoppedSpeaking?.Invoke(this, null); };
			speaker.SetOnUtteranceProgressListener(listener);
		}

		public void Speak(string text)
		{
            toSpeak = text;
            toId = new Guid().ToString();

			speaker.Speak(toSpeak, QueueMode.Flush, null, toId);
		}

		public void Stop()
		{
			if (speaker != null && speaker.IsSpeaking)
				speaker.Stop();
		}

		public bool IsSpeaking()
		{
			if (speaker != null && speaker.IsSpeaking) return true;
			else return false;
		}


		public void OnInit(OperationResult status)
		{
			if (status.Equals(OperationResult.Success))
			{
				speaker.Speak(toSpeak, QueueMode.Flush, null, toId);
			}
		}
    }

	class TTSListener : UtteranceProgressListener
	{
		public event EventHandler<string> Done;
		public override void OnDone(string utteranceId)
		{
			Done?.Invoke(this, utteranceId);
		}

		public event EventHandler<string> Error;
		public override void OnError(string utteranceId)
		{
			Error?.Invoke(this, utteranceId);
		}
		public override void OnError(string utteranceId, [GeneratedEnum] TextToSpeechError errorCode)
		{
			Error?.Invoke(this, utteranceId);
		}

		public event EventHandler<string> Start;
		public override void OnStart(string utteranceId)
		{
			Start?.Invoke(this, utteranceId);
		}

		public event EventHandler<string> Stop;
		public override void OnStop(string utteranceId, bool interrupted)
		{
			Stop?.Invoke(this, utteranceId);
		}
	}
}
