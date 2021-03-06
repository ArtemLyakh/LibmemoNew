﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Map
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Map : ContentPage
    {
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Map(List<Models.DeadPerson> persons)
		{
			BindingContext = new ViewModel(persons);
			InitializeComponent();
		}


		protected override void OnAppearing()
		{
			base.OnAppearing();
			Model.OnAppearing();
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();
			Model.OnDisappearing();
		}


        public class ViewModel : BaseViewModel
        {
            private Dictionary<int, Models.DeadPerson> Data;

            private CustomElements.CustomMap.Pin GetPin(Models.DeadPerson person) => new CustomElements.CustomMap.Pin(
                person.Id,
                person.FIO,
                person.DateBirth.HasValue && person.DateDeath.HasValue
                    ? $"{person.DateBirth.Value.ToString("dd.MM.yyyy")}\u2014{person.DateDeath.Value.ToString("dd.MM.yyyy")}"
                    : string.Empty,
                person.Icon,
                new Position(person.Latitude, person.Longitude),
                !string.IsNullOrWhiteSpace(person.Text) ? CustomElements.CustomMap.PinImage.Speakable : CustomElements.CustomMap.PinImage.Default
            );

            public ViewModel(List<Models.DeadPerson> persons) : base()
            {
                Data = persons.ToDictionary(i => i.Id);

                Pins = persons.Select(i => GetPin(i)).ToList();


                FollowUser = true;
                UserPositionChanged += (sender, e) => UserPosition = e;


                SelectedPinChanged += (sender, e) => IsHideAllPinsVisible = !IsShowAllPinsVisible && e != null;
                SelectedPinChanged += (sender, e) => IsShowSpeakBtn = e != null && !string.IsNullOrWhiteSpace(Data[e.Id].Text);
                SelectedPinChanged += (sender, e) => IsSetRouteVisible = e != null;

                RouteChanged += ViewModel_RouteChanged;
            }

			private void ViewModel_RouteChanged(object sender, List<Position> e)
			{
				if (e == null)
				{
					Title = null;
					return;
				}

				var routeDistance = e
					.Take(e.Count - 1)
					.Zip(e.Skip(1), (curr, next) => (Current: curr, Next: next))
					.AsParallel()
					.Select(i => CalculateDistance(i.Current, i.Next))
					.Sum();

				Title = $"\u2248 {Math.Round(routeDistance)}м";
			}

            public override void OnAppearing()
            {
                base.OnAppearing();
                Helpers.TextToSpeech.Current.TTSStopped += OnTextToSpeechStopedSpeaking;
            }

            public override void OnDisappearing()
            {
                base.OnDisappearing();
                Helpers.TextToSpeech.Current.Stop();
                Helpers.TextToSpeech.Current.TTSStopped -= OnTextToSpeechStopedSpeaking;
            }

            private const string DefaultTitle = "Карта";

            private string _title = DefaultTitle;
            public string Title
            {
                get => _title;
                set
                {
                    if (value != _title)
                    {
                        _title = string.IsNullOrWhiteSpace(value) ? DefaultTitle : value;
                        OnPropertyChanged(nameof(Title));
                    }
                }
            }

            private MapType _mapType = MapType.Street;
            public MapType MapType
            {
                get => _mapType;
                set
                {
                    if (_mapType != value)
                    {
                        _mapType = value;
                        OnPropertyChanged(nameof(MapType));
                    }
                }
            }

			public ICommand SpaceMapCommand => new Command(() => this.MapType = MapType.Hybrid);

			public ICommand StreetMapCommand => new Command(() => this.MapType = MapType.Street);

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

            private List<CustomElements.CustomMap.Pin> _pins = new List<CustomElements.CustomMap.Pin>();
            public List<CustomElements.CustomMap.Pin> Pins
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

            private bool _isGesturesEnabled = true;
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

            private event EventHandler<CustomElements.CustomMap.Pin> SelectedPinChanged;
            private CustomElements.CustomMap.Pin _selectedPin;
            public CustomElements.CustomMap.Pin SelectedPin
            {
                get => _selectedPin;
                set
                {
                    if (_selectedPin != value)
                    {
                        _selectedPin = value;
                        OnPropertyChanged(nameof(SelectedPin));
                        SelectedPinChanged?.Invoke(this, value);
                    }
                }
            }

            public ICommand InfoWindowClickedCommand => new Command<CustomElements.CustomMap.Pin>(async pin =>
            {
                await App.GlobalPage.Push(new Pages.Detail(pin.Id));
            });

            private event EventHandler<Position> UserPositionChanged;
            public ICommand UserPositionChangedCommand => new Command<Position>(position => UserPositionChanged?.Invoke(this, position));


            private Position? UserPosition { get; set; }


			private bool _followUser;
			public bool FollowUser
			{
				get => _followUser;
				set
				{
                    if (_followUser != value) {
                        _followUser = value;

                        if (value) {
                            UserPositionChanged += FixCamera;
                            IsGesturesEnabled = false;
                        } else {
                            UserPositionChanged -= FixCamera;
                            IsGesturesEnabled = true;
                        }

                        OnPropertyChanged(nameof(FollowUser));                   
                    }
				}
			}

            private void FixCamera(object sender, Position position)
            {
                CameraPosition = position;
            }

			public ICommand FollowUserToogleCommand => new Command(() => {
                if (FollowUser)
                    FollowUser = false;
                else 
                {
					if (UserPosition.HasValue) CameraPosition = UserPosition.Value;
                    FollowUser = true;
                }
			});



            private bool _isShowAllPinsVisible = false;
            public bool IsShowAllPinsVisible
            {
                get => _isShowAllPinsVisible;
                set {
                    if (_isShowAllPinsVisible != value) {
                        _isShowAllPinsVisible = value;
                        OnPropertyChanged(nameof(IsShowAllPinsVisible));
                    }
                }
            }

            private bool _isHideAllPinnVisible = false;
			public bool IsHideAllPinsVisible
			{
				get => _isHideAllPinnVisible;
				set
				{
					if (_isHideAllPinnVisible != value)
					{
						_isHideAllPinnVisible = value;
						OnPropertyChanged(nameof(IsHideAllPinsVisible));
					}
				}
			}


			public ICommand HidePinsCommand => new Command(() => {
                if (SelectedPin == null) return;

                Pins = new List<CustomElements.CustomMap.Pin>() {
                    SelectedPin
                };
                IsShowAllPinsVisible = true;
			});


			public ICommand ShowPinsCommand => new Command(() => {
                Pins = Data.Select(i => GetPin(i.Value)).ToList();

                IsShowAllPinsVisible = false;
			});









			private bool _isShowSpeakBtn = false;
			public bool IsShowSpeakBtn
			{
				get => _isShowSpeakBtn;
				set
				{
					if (_isShowSpeakBtn != value)
					{
						_isShowSpeakBtn = value;
						OnPropertyChanged(nameof(IsShowSpeakBtn));
					}
				}
			}

			private bool _isTTSPlaying = false;
			public bool IsTTSPlaying
			{
				get => _isTTSPlaying;
				set
				{
					if (_isTTSPlaying != value)
					{
						_isTTSPlaying = value;
						OnPropertyChanged(nameof(IsTTSPlaying));
					}
				}
			}

			public ICommand StartTTSCommand => new Command(() => {
                if (SelectedPin == null || string.IsNullOrWhiteSpace(Data[SelectedPin.Id].Text)) return;

                if (IsTTSPlaying)
                    Helpers.TextToSpeech.Current.Stop();

                Helpers.TextToSpeech.Current.Speak(Data[SelectedPin.Id].Text);
                IsTTSPlaying = true;
			});
            public ICommand StopTTSCommand => new Command(() => {
                Helpers.TextToSpeech.Current.Stop();
                IsTTSPlaying = false;
            });

			private void OnTextToSpeechStopedSpeaking(object sender, EventArgs e)
			{
				IsTTSPlaying = false;
			}







			private event EventHandler<List<Position>> RouteChanged;
			private List<Position> _route;
			public List<Position> Route
			{
				get => _route;
				set
				{
					if (_route != value)
					{
						_route = value;
						RouteChanged?.Invoke(this, value);
						OnPropertyChanged(nameof(Route));
					}
				}
			}

            private bool _isSetRouteVisible;
            public bool IsSetRouteVisible
            {
                get => _isSetRouteVisible;
                set {
                    if (_isSetRouteVisible != value) {
                        _isSetRouteVisible = value;
                        OnPropertyChanged(nameof(IsSetRouteVisible));
                    }
                }
            }

			private bool _isRouteActive;
			public bool IsRouteActive
			{
				get => _isRouteActive;
				set
				{
					if (_isRouteActive != value)
					{
						_isRouteActive = value;
						OnPropertyChanged(nameof(IsRouteActive));
					}
				}
			}

            public ICommand SetLinearRouteCommand => new Command(() =>
            {
                if (SelectedPin == null || !UserPosition.HasValue) return;

                List<Position> route = new List<Position>() {
                    UserPosition.Value,
                    SelectedPin.Position
                };

                Route = route;

                UserPositionChanged -= UpdateRoute;
                UserPositionChanged += UpdateRoute;

                IsRouteActive = true;
            });

            public ICommand SetCalculatedRouteCommand => new Command(async () =>
            {
                if (SelectedPin == null || !UserPosition.HasValue) return;

                List<Position> route = await Helpers.RouteBuilder.GetRoute(UserPosition.Value, SelectedPin.Position);
                if (route == null) {
                    App.ToastNotificator.Show("Не удалось построить маршрут");
                    return;
                }

                Route = new Position[] { UserPosition.Value }.Concat(route).Concat(new Position[] { SelectedPin.Position }).ToList();

				UserPositionChanged -= UpdateRoute;
				UserPositionChanged += UpdateRoute;

                IsRouteActive = true;
            });

            public ICommand DeleteRouteCommand => new Command(() =>
            {
                UserPositionChanged -= UpdateRoute;
                Route = null;
                IsRouteActive = false;
            });

			private const double RoutePinsChangeDistance = 15;
			private bool routeUpdating = false;
            private void UpdateRoute(object sender, Position e)
            {
				if (routeUpdating) return;
				routeUpdating = true;

				var route = Route.ToList();
				route[0] = e;

				if (CalculateDistance(route[0], route[1]) < RoutePinsChangeDistance)
				{
					if (Route.Count > 2)
					{
						route[1] = route[0];
						route = route.Skip(1).ToList();
					}
					else
					{
						DeleteRouteCommand.Execute(null);
						return;
					}
				}

				Route = route;

				routeUpdating = false;
            }

			private static double CalculateDistance(Position A, Position B)
			{
				double d1 = A.Latitude * 0.017453292519943295;
				double d2 = A.Longitude * 0.017453292519943295;
				double d3 = B.Latitude * 0.017453292519943295;
				double d4 = B.Longitude * 0.017453292519943295;
				double d5 = d4 - d2;
				double d6 = d3 - d1;
				double d7 = Math.Pow(Math.Sin(d6 / 2.0), 2.0) + ((Math.Cos(d1) * Math.Cos(d3)) * Math.Pow(Math.Sin(d5 / 2.0), 2.0));
				double d8 = 2.0 * Math.Atan2(Math.Sqrt(d7), Math.Sqrt(1.0 - d7));
				return (6376500.0 * d8);
			}











			public ICommand SearchCommand => new Command(async () => {
				await App.GlobalPage.Push(new Pages.Map.Search(Data.Select(i => i.Value).ToList(), async id =>
				{
					await App.GlobalPage.Pop();
					ShowPinsCommand.Execute(null);
					var pin = Pins.First(i => i.Id == id);
					this.SelectedPin = pin;
					this.FollowUser = false;
					CameraPosition = pin.Position;
				}));
			});







			#region Routes

			//public enum RouteType
			//{
			//	None, Linear, Calculated
			//}

			//private RouteType _currentRoute = RouteType.None;
			//private RouteType CurrentRoute
			//{
			//	get => _currentRoute;
			//	set
			//	{
			//		if (_currentRoute != value)
			//		{
			//			_currentRoute = value;
			//			OnPropertyChanged(nameof(CurrentRoute));
			//			OnPropertyChanged(nameof(IsRouteActive));
			//		}
			//	}
			//}

			//public bool IsRouteActive => CurrentRoute != RouteType.None;

			//private Position? RouteFrom { get; set; }
			//private Position? RouteTo { get; set; }
			//private CustomPin RouteToPin { get; set; }

			//private bool _routeProcessing = false;

			//public ICommand SetLinearRouteCommand => new Command(async () => {
			//	if (this._routeProcessing) return;

			//	DeleteRouteCommand.Execute(null);

			//	if (this.UserPosition != default(Position) && this.SelectedPin != null)
			//	{
			//		RouteToPin = SelectedPin;

			//		var from = this.UserPosition;
			//		var to = this.SelectedPin.Position;

			//		try
			//		{
			//			_routeProcessing = true;
			//			await this.MapFunctions.SetLinearRouteAsync(from, to);
			//		}
			//		catch
			//		{
			//			App.ToastNotificator.Show("Ошибка построения маршрута");
			//			return;
			//		}
			//		finally
			//		{
			//			_routeProcessing = false;
			//		}

			//		this.CurrentRoute = RouteType.Linear;
			//		this.RouteFrom = from;
			//		this.RouteTo = to;

			//		var distance = CalculateDistance(RouteFrom.Value, RouteTo.Value);
			//		distance = Math.Round(distance);
			//		this.Title = $"~ {distance.ToString("N0")} м";

			//		UserPositionChanged -= OnUserPositionChangedUpdateLinearRoute;
			//		UserPositionChanged += OnUserPositionChangedUpdateLinearRoute;

			//		UserPositionChanged += OnUserPositionCloseToRouteEnd;
			//	}
			//});
			//private void OnUserPositionCloseToRouteEnd(object sender, Position position)
			//{
			//	if (!RouteTo.HasValue || RouteToPin == null || CurrentRoute != RouteType.Linear) return;
			//	if (CalculateDistance(position, RouteTo.Value) <= 15)
			//	{
			//		SelectedPin = RouteToPin;
			//		UserPositionChanged -= OnUserPositionCloseToRouteEnd;
			//	}
			//}
			//private void OnUserPositionChangedUpdateLinearRoute(object sender, Position position)
			//{
			//	UpdateLinearRoute.Execute(position);
			//}
			//public ICommand UpdateLinearRoute => new Command<Position>(async position => {
			//	if (_routeProcessing || CurrentRoute != RouteType.Linear || !RouteTo.HasValue) return;

			//	var from = position;
			//	var to = RouteTo.Value;

			//	try
			//	{
			//		_routeProcessing = true;
			//		await this.MapFunctions.SetLinearRouteAsync(from, to);
			//	}
			//	catch
			//	{
			//		App.ToastNotificator.Show("Ошибка построения маршрута");
			//		UserPositionChanged -= OnUserPositionChangedUpdateLinearRoute;
			//		return;
			//	}
			//	finally
			//	{
			//		_routeProcessing = false;
			//	}

			//	this.RouteFrom = from;

			//	var distance = CalculateDistance(RouteFrom.Value, RouteTo.Value);
			//	distance = Math.Round(distance);
			//	this.Title = $"~ {distance.ToString("N0")} м";
			//});



			//public ICommand SetCalculatedRouteCommand => new Command(async () => {
			//	if (this._routeProcessing) return;

			//	DeleteRouteCommand.Execute(null);

			//	if (this.UserPosition != default(Position) && this.SelectedPin != null)
			//	{
			//		RouteToPin = SelectedPin;

			//		var from = this.UserPosition;
			//		var to = this.SelectedPin.Position;

			//		try
			//		{
			//			_routeProcessing = true;
			//			await this.MapFunctions.SetCalculatedRouteAsync(from, to);
			//		}
			//		catch
			//		{
			//			App.ToastNotificator.Show("Ошибка построения маршрута");
			//			return;
			//		}
			//		finally
			//		{
			//			_routeProcessing = false;
			//		}

			//		this.CurrentRoute = RouteType.Calculated;
			//		this.RouteFrom = from;
			//		this.RouteTo = to;
			//	}
			//});


			//public ICommand DeleteRouteCommand => new Command(() => {
			//	this.MapFunctions.DeleteRoute();
			//	this.CurrentRoute = RouteType.None;

			//	RouteToPin = null;
			//	UserPositionChanged -= OnUserPositionCloseToRouteEnd;
			//	UserPositionChanged -= OnUserPositionChangedUpdateLinearRoute;
			//	SetDefaultTitle();
			//});

			#endregion






		}
    }
}
