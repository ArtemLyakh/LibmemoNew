
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
            IsShowInfoWindow = true;

			Pins = new List<CustomElements.CustomMap.Pin>()
			{
                new CustomElements.CustomMap.Pin(245,
                                                 "title",
                                                 "text", 
                                                 new Uri(@"http://panor.ru/upload/resize_cache/iblock/463/1980_1200_1/463b3cbe41bdfce98f7e5585f66d4630.jpg"), 
                                                 new Position(41.89, 12.49)
                                                ),
                new CustomElements.CustomMap.Pin(555, "t2", null, null, new Position(41.891, 12.491))
			};
        }

        private bool sp;
        public ICommand TestCommand => new Command(() =>
        {
            SelectedPin = Pins[sp ? 0 : 1];
            sp = !sp;
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
                new CustomElements.CustomMap.Pin(++i, "title", "text", null, position)
            };
            Pins = Pins?.Concat(newEl).ToList() ?? newEl;
		});

    }
}
