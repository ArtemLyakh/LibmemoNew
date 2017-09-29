using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Android.Widget;
using FFImageLoading;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(Libmemo.CustomElements.CustomMap.Map), typeof(Libmemo.Droid.Renderers.CustomMapRenderer))]
namespace Libmemo.Droid.Renderers
{
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback
    {
        private CustomElements.CustomMap.IMapRendererCallable RendererCall => (CustomElements.CustomMap.IMapRendererCallable)this.Element;
        private CustomElements.CustomMap.Map FormMap => (CustomElements.CustomMap.Map)this.Element;

        private GoogleMap _googleMap;
        private bool IsDrawn;


        private Dictionary<CustomElements.CustomMap.Pin, Marker> PinsMarkers { get; } = new Dictionary<CustomElements.CustomMap.Pin, Marker>();
		private Polyline _route;

        private Libmemo.CustomElements.CustomMap.Pin _selectedPin;

        private Dictionary<Uri, Android.Graphics.Bitmap> MarkerIconsDownloaded = new Dictionary<Uri, Android.Graphics.Bitmap>();

        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.Maps.Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var formsMap = (Libmemo.CustomElements.CustomMap.Map)e.NewElement;

                formsMap.PropertyChanged -= FormMapPropertyChanged;

                //googleMap events
                _googleMap.MapClick -= OnMapClicked;
                _googleMap.CameraChange -= OnCameraPositionChanged;
                _googleMap.MarkerClick -= OnMarkerClicked;
                _googleMap.InfoWindowClose -= OnInfoWindowClosed;
                _googleMap.MyLocationChange -= OnUserLocationChanged;
                _googleMap.InfoWindowClick += OnInfoWindowClicked;
            }

            if (e.NewElement != null)
            {
                var formsMap = (Libmemo.CustomElements.CustomMap.Map)e.NewElement;

                formsMap.PropertyChanged += FormMapPropertyChanged;

                Control.GetMapAsync(this);
            }
        }

        private bool _onMapReadyPerformed = false;
        void IOnMapReadyCallback.OnMapReady(GoogleMap googleMap)
        {
			if (_onMapReadyPerformed) return;
			_onMapReadyPerformed = true;

            InvokeOnMapReadyBaseClassHack(googleMap);

            _googleMap = googleMap;
            _googleMap.SetInfoWindowAdapter(this);

            //googleMap events
            _googleMap.MapClick += OnMapClicked;
            _googleMap.CameraChange += OnCameraPositionChanged;
            _googleMap.MarkerClick += OnMarkerClicked;
            _googleMap.InfoWindowClose += OnInfoWindowClosed;
            _googleMap.MyLocationChange += OnUserLocationChanged;
            _googleMap.InfoWindowClick += OnInfoWindowClicked;
        }
		private void InvokeOnMapReadyBaseClassHack(GoogleMap googleMap)
		{
			System.Reflection.MethodInfo onMapReadyMethodInfo = null;

			Type baseType = typeof(MapRenderer);
			foreach (var currentMethod in baseType.GetMethods(System.Reflection.BindingFlags.NonPublic |
															 System.Reflection.BindingFlags.Instance |
															  System.Reflection.BindingFlags.DeclaredOnly))
			{

				if (currentMethod.IsFinal && currentMethod.IsPrivate)
				{
					if (string.Equals(currentMethod.Name, "OnMapReady", StringComparison.Ordinal))
					{
						onMapReadyMethodInfo = currentMethod;

						break;
					}

					if (currentMethod.Name.EndsWith(".OnMapReady", StringComparison.Ordinal))
					{
						onMapReadyMethodInfo = currentMethod;

						break;
					}
				}
			}

			if (onMapReadyMethodInfo != null)
			{
				onMapReadyMethodInfo.Invoke(this, new[] { googleMap });
			}
		}


        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            base.OnLayout(changed, l, t, r, b);

            if (changed) IsDrawn = false;
        }

        protected override void OnElementPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (_googleMap != null && !IsDrawn)
            {
                Clear();
                Draw();
                IsDrawn = true;
            }
        }

        private void FormMapPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_googleMap == null) return;

            var map = (Libmemo.CustomElements.CustomMap.Map)sender;

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.CameraPositionProperty.PropertyName) {
                var latLng = new LatLng(map.CameraPosition.Latitude, map.CameraPosition.Longitude);
                var zoom = _googleMap.CameraPosition.Zoom;
                _googleMap.AnimateCamera(CameraUpdateFactory.NewLatLngZoom(latLng, zoom));
                return;
            }

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.IsGesturesEnabledProperty.PropertyName) {
				_googleMap.UiSettings.TiltGesturesEnabled = map.IsGesturesEnabled;
                _googleMap.UiSettings.ZoomGesturesEnabled = map.IsGesturesEnabled;
                _googleMap.UiSettings.RotateGesturesEnabled = map.IsGesturesEnabled;
                _googleMap.UiSettings.ScrollGesturesEnabled = map.IsGesturesEnabled;
                return;
            }

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.PinsProperty.PropertyName) {
                foreach(var pinMarker in PinsMarkers) {
                    pinMarker.Key.PropertyChanged -= PinPropertyChanged;
                    pinMarker.Value.Remove();
                }
                PinsMarkers.Clear();

                if (map.Pins != null) {
					foreach (var pin in map.Pins)
					{
						pin.PropertyChanged += PinPropertyChanged;
						var marker = GetMarker(pin);
                        PinsMarkers[pin] = marker;
					}
                }
                return;
            }

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.SelectedPinProperty.PropertyName) {
                if (map.SelectedPin == null && _selectedPin != null) {
                    PinsMarkers[_selectedPin].HideInfoWindow();
                } else {
                    PinsMarkers[map.SelectedPin].ShowInfoWindow();
                }

                return;
            }

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.RouteProperty.PropertyName) {
                if (_route != null) {
                    _route.Remove();
                    _route = null;
                }

                if (map.Route != null && map.Route.Count >= 2) {
                    _route = GetRoute(map.Route);
                }
            }
        }
        private void PinPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            var pin = (Libmemo.CustomElements.CustomMap.Pin)sender;
            var marker = PinsMarkers[pin];

            marker.Position = new LatLng(pin.Position.Latitude, pin.Position.Longitude);
        }

        private void Clear()
        {
			foreach (var pinMarker in PinsMarkers)
			{
				pinMarker.Key.PropertyChanged -= PinPropertyChanged;
				pinMarker.Value.Remove();
			}
			PinsMarkers.Clear();
        }

        private void Draw()
        {
            _googleMap.MyLocationEnabled = true;

			_googleMap.UiSettings.MyLocationButtonEnabled = false;
			_googleMap.UiSettings.MapToolbarEnabled = false;
			_googleMap.UiSettings.CompassEnabled = false;
			_googleMap.UiSettings.ZoomControlsEnabled = false;
        }



        private void OnCameraPositionChanged(object sender, GoogleMap.CameraChangeEventArgs e)
        {
            RendererCall.RaiseCameraPositionChange(new Xamarin.Forms.Maps.Position(e.Position.Target.Latitude, e.Position.Target.Longitude));
        }



        private void OnMapClicked(object sender, GoogleMap.MapClickEventArgs e)
        {
            RendererCall.RaiseMapClick(new Xamarin.Forms.Maps.Position(e.Point.Latitude, e.Point.Longitude));
        }


        private void OnMarkerClicked(object sender, GoogleMap.MarkerClickEventArgs e)
        {
            if (!FormMap.IsShowInfoWindow) return;

            var pin = PinsMarkers.First(i => i.Value.Id == e.Marker.Id).Key;
            _selectedPin = pin;
            RendererCall.RaiseSelectedPinSelect(pin);

            e.Marker.ShowInfoWindow();
        }

        private void OnInfoWindowClosed(object sender, GoogleMap.InfoWindowCloseEventArgs e)
        {
            var pin = PinsMarkers.First(i => i.Value.Id == e.Marker.Id).Key;

            if (FormMap.SelectedPin == pin) {
                RendererCall.RaiseSelectedPinSelect(null);
                _selectedPin = null;
            }
        }

        private void OnUserLocationChanged(object sender, GoogleMap.MyLocationChangeEventArgs e)
        {
            RendererCall.RaiseUserPositionChange(new Xamarin.Forms.Maps.Position(e.Location.Latitude, e.Location.Longitude));
        }

        private void OnInfoWindowClicked(object sender, GoogleMap.InfoWindowClickEventArgs e)
        {
            var pin = PinsMarkers.First(i => i.Value.Id == e.Marker.Id).Key;
            
            RendererCall.RaiseInfoWindowClick(pin);
        }







        Android.Views.View GoogleMap.IInfoWindowAdapter.GetInfoContents(Marker marker)
        {
            var inflater = Android.App.Application.Context.GetSystemService(Android.Content.Context.LayoutInflaterService) as Android.Views.LayoutInflater;
			if (inflater != null)
			{
				Android.Views.View view;

                var pin = PinsMarkers.First(i => i.Value.Id == marker.Id).Key;

				view = inflater.Inflate(Resource.Layout.MapInfoWindow, null);

				var infoTitle = view.FindViewById<TextView>(Resource.Id.InfoWindowTitle);
				var infoSubtitle = view.FindViewById<TextView>(Resource.Id.InfoWindowSubtitle);
				var infoWindowButton = view.FindViewById<ImageButton>(Resource.Id.InfoWindowButton);

				if (infoTitle != null)
				{
                    infoTitle.Text = pin.Title;
				}
				if (infoSubtitle != null)
				{
                    infoSubtitle.Text = pin.Text;
				}

                if (pin.Icon != null)
				{
					if (MarkerIconsDownloaded.ContainsKey(pin.Icon))
					{
						infoWindowButton.SetMaxHeight(infoWindowButton.Height);
						infoWindowButton.SetMaxWidth(infoWindowButton.Width);
						infoWindowButton.SetBackgroundColor(Android.Graphics.Color.Transparent);
						infoWindowButton.SetImageBitmap(MarkerIconsDownloaded[pin.Icon]);
					}
					else
					{
						Task.Run(async () => {
							Android.Graphics.Bitmap bitmap;

							try
							{
                                var request = (HttpWebRequest)WebRequest.Create(pin.Icon);
                                request.Method = "GET";
                                request.Timeout = 5000;

                                using (var responce = await request.GetResponseAsync()) {
                                    var download = responce.GetResponseStream();
                                    						
									try
									{
										bitmap = await Android.Graphics.BitmapFactory.DecodeStreamAsync(download);
									}
									catch
									{
										bitmap = null;
									}
                                }
							}
							catch (Exception e)
							{
                                bitmap = null;
							}

							if (bitmap != null)
							{
                                MarkerIconsDownloaded[pin.Icon] = bitmap;

								if (this._selectedPin != null && this._selectedPin == pin)
								{
									Device.BeginInvokeOnMainThread(() => marker.ShowInfoWindow());
								}
							}
						});
					}
				}

				return view;
			}
			return null;
        }

        Android.Views.View GoogleMap.IInfoWindowAdapter.GetInfoWindow(Marker marker)
        {
            return null;
        }











        private Marker GetMarker(CustomElements.CustomMap.Pin pin)
        {
			var marker = new MarkerOptions();

			marker.SetPosition(new LatLng(pin.Position.Latitude, pin.Position.Longitude));
			marker.SetTitle(pin.Title);
			marker.SetSnippet(pin.Text);

			if (pin.PinImage == CustomElements.CustomMap.PinImage.Default)
			{
				marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.default_pin));
			}
            else if (pin.PinImage == CustomElements.CustomMap.PinImage.Speakable)
			{
				marker.SetIcon(BitmapDescriptorFactory.FromResource(Resource.Drawable.speaker_pin));
			}
			else
			{
                throw new NotSupportedException();
			}

            return _googleMap.AddMarker(marker);		
        }

        private Polyline GetRoute(List<Xamarin.Forms.Maps.Position> route)
        {
			var line = new PolylineOptions();
			line.Clickable(false);
			line.Visible(true);
			line.InvokeColor(Color.Red.ToAndroid());
			line.InvokeWidth(5);

            foreach(var position in route) {
                line.Add(new LatLng(position.Latitude, position.Longitude));
            }

            return _googleMap.AddPolyline(line);
        }
    }
}
