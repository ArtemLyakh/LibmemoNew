using System;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo.CustomElements.CustomMap
{
    public class Pin : BindableObject
    {
        public int Id { get; private set; }
        public string Title { get; private set; }
        public Uri Icon { get; private set; }
        public string Text { get; private set; }
        public PinImage PinImage { get; private set; }

        private Position _position;
        public Position Position {
            get => _position;
            set {
                if (_position != value) {
                    _position = value;
                    OnPropertyChanged(nameof(Position));
                }
            }
        }

        public Pin(int id, string title, string text, Uri icon, Position position, PinImage pinImage = PinImage.Default)
        {
            this.Id = id;
            this.Title = title;
            this.Text = text;
            this.Icon = icon;
            this.Position = position;
            this.PinImage = PinImage;
        }

		public override int GetHashCode()
		{
            return this.Id;
		}

		public override bool Equals(object obj)
		{
            if (!(obj is Pin)) return false;
			return Id == ((Pin)obj).Id;
		}

    }
}
