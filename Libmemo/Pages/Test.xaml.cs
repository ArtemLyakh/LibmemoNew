
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;

namespace Libmemo.Pages
{
    public partial class Test : ContentPage
    {
        public Test()
        {
            InitializeComponent();
            BindingContext = this;
        }

        public ICommand TestCommand => new Command(() =>
        {
            Route = Pins?.Select(i => new Position(i.Position.Latitude, i.Position.Longitude)).ToList();
        });

		private Position _cameraPosition;
		public Position CameraPosition
		{
			get => _cameraPosition;
			set
			{
				if (_cameraPosition != value)
				{
					_cameraPosition = value;
					OnPropertyChanged(nameof(CameraPosition));
				}
			}
		}

		private double _cameraZoom;
		public double CameraZoom
		{
			get => _cameraZoom;
			set
			{
				if (_cameraZoom != value)
				{
					_cameraZoom = value;
					OnPropertyChanged(nameof(CameraZoom));
				}
			}
		}

        private bool _isGesturesEnabled;
		public bool IsGesturesEnabled
		{
			get => _isGesturesEnabled;
			set
			{
				if (_isGesturesEnabled != value)
				{
					_isGesturesEnabled = value;
					OnPropertyChanged(nameof(IsGesturesEnabled));
				}
			}
		}

        private List<Libmemo.CustomElements.CustomMap.Pin> _pins = new List<CustomElements.CustomMap.Pin>();
		public List<Libmemo.CustomElements.CustomMap.Pin> Pins
		{
			get => _pins;
			set
			{
				if (_pins != value)
				{
					_pins = value;
					OnPropertyChanged(nameof(Pins));
				}
			}
		}

		private Libmemo.CustomElements.CustomMap.Pin _selectedPin;
		public Libmemo.CustomElements.CustomMap.Pin SelectedPin
		{
			get => _selectedPin;
			set
			{
				if (_selectedPin != value)
				{
					_selectedPin = value;
					OnPropertyChanged(nameof(SelectedPin));
				}
			}
		}

        private List<Position> _route;
        public List<Position> Route
		{
			get => _route;
			set
			{
				if (_route != value)
				{
					_route = value;
					OnPropertyChanged(nameof(Route));
				}
			}
		}

		private bool _isShowInfoWindow = true;
		public bool IsShowInfoWindow
		{
			get => _isShowInfoWindow;
			set
			{
				if (_isShowInfoWindow != value)
				{
					_isShowInfoWindow = value;
					OnPropertyChanged(nameof(IsShowInfoWindow));
				}
			}
		}

        public ICommand InfoWindowClickedCommand => new Command<CustomElements.CustomMap.Pin>(pin =>
        {
            var q = 1;
        });

        public ICommand UserPositionChangedCommand => new Command<Position>(position =>
		{
			var q = 1;
		});

        private int i = 0;
        public ICommand MapClickCommand => new Command<Position>(position =>
		{
            var newEl = new List<Libmemo.CustomElements.CustomMap.Pin>
            {
                new CustomElements.CustomMap.Pin(++i, "title", "text", new Uri("https://www.google.ru/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png"), position)
            };
            Pins = Pins?.Concat(newEl).ToList() ?? newEl;
		});

    }
}
