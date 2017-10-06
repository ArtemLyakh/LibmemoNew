using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Libmemo.iOS.Renderers;
using MapKit;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Maps.iOS;

[assembly: ExportRenderer(typeof(Libmemo.CustomElements.CustomMap.Map), typeof(CustomMapRenderer))]
namespace Libmemo.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class CustomMapRenderer : MapRenderer
    {
        private MKMapView Map => this.Control as MKMapView;
		private CustomElements.CustomMap.IMapRendererCallable RendererCall => (CustomElements.CustomMap.IMapRendererCallable)this.Element;
		private CustomElements.CustomMap.Map FormMap => (CustomElements.CustomMap.Map)this.Element;

        private Dictionary<Libmemo.CustomElements.CustomMap.Pin, MKIdPointAnnotation> PinAnnotations { get; } = new Dictionary<CustomElements.CustomMap.Pin, MKIdPointAnnotation>();

        private Dictionary<Uri, Foundation.NSData> AnnotationsIconsDownloaded { get; } = new Dictionary<Uri, Foundation.NSData>();

        private CustomElements.CustomMap.Pin _selectedPin = null;
        private bool selectedPinChanging = false;

        private UIGestureRecognizer _tapGestureRecognizer;


        private MKPolylineRenderer polylineRenderer;

		protected override void OnElementChanged(Xamarin.Forms.Platform.iOS.ElementChangedEventArgs<View> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null && Map != null) {

                Map.RegionChanged -= OnRegionChanged;
                Map.DidUpdateUserLocation -= OnDidUpdateUserLocation;

				if (Map.Overlays != null && Map.Overlays.Length > 0)
				{
					Map.RemoveOverlays(Map.Overlays);
				}
                Map.OverlayRenderer = null;
                polylineRenderer = null;

                ClearAnnotations();
                Map.GetViewForAnnotation = null;
                Map.CalloutAccessoryControlTapped -= OnCalloutAccessoryControlTapped;
                Map.DidSelectAnnotationView -= OnDidSelectAnnotationView;
                Map.DidDeselectAnnotationView -= OnDidDeselectAnnotationView;

                Map.RemoveGestureRecognizer(_tapGestureRecognizer);
                _tapGestureRecognizer.Dispose();

                e.OldElement.PropertyChanged -= OnPropertyChanged;
            }

			if (e.NewElement != null)
			{
                Map.RegionChanged += OnRegionChanged;
                Map.DidUpdateUserLocation += OnDidUpdateUserLocation;

                Map.OverlayRenderer = GetOverlayRenderer;

                Map.GetViewForAnnotation = GetViewForAnnotation;
                Map.CalloutAccessoryControlTapped += OnCalloutAccessoryControlTapped;
                Map.DidSelectAnnotationView += OnDidSelectAnnotationView;
                Map.DidDeselectAnnotationView += OnDidDeselectAnnotationView;

				_tapGestureRecognizer = new UITapGestureRecognizer(OnMapClicked);
				_tapGestureRecognizer.ShouldReceiveTouch = (recognizer, touch) => !(touch.View is MKIdAnnotationView);
                Map.AddGestureRecognizer(_tapGestureRecognizer);

                e.NewElement.PropertyChanged += OnPropertyChanged;



                var formsMap = e.NewElement as CustomElements.CustomMap.Map;

                SetGestures(formsMap.IsGesturesEnabled);
                if (formsMap.CameraPosition != default(Xamarin.Forms.Maps.Position))
                    SetCameraPosition(formsMap.CameraPosition, false);
                SetPins(formsMap.Pins);

                if (Map.UserLocation != null) {
                    RendererCall.RaiseUserPositionChange(new Xamarin.Forms.Maps.Position(Map.UserLocation.Coordinate.Latitude, Map.UserLocation.Coordinate.Longitude));
                }
			}

        }

        private void ClearAnnotations() 
        {
			foreach(var item in PinAnnotations)
            {
                item.Key.PropertyChanged -= OnPinPropertyChanged;
                Map.RemoveAnnotation(item.Value);
            }
            PinAnnotations.Clear();
        }

        private void OnCalloutAccessoryControlTapped(object sender, MKMapViewAccessoryTappedEventArgs e) 
        {
            if (!(e.View is MKIdAnnotationView)) return;

            var pin = PinAnnotations.First(i => i.Key.Id == int.Parse(e.View.ReuseIdentifier)).Key;
            RendererCall.RaiseInfoWindowClick(pin);
        }

		private void OnDidSelectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!(e.View is MKIdAnnotationView)) return;

            var pin = PinAnnotations.First(i => i.Key.Id == int.Parse(e.View.ReuseIdentifier)).Key;

            _selectedPin = pin;
            if (!selectedPinChanging) RendererCall.RaiseSelectedPinSelect(pin);
        }

        private void OnDidDeselectAnnotationView(object sender, MKAnnotationViewEventArgs e)
        {
            if (!(e.View is MKIdAnnotationView)) return;

            _selectedPin = null;
            if (!selectedPinChanging) RendererCall.RaiseSelectedPinSelect(null);
        }

        private void OnRegionChanged(object sender, MKMapViewChangeEventArgs e)
        {
            var position = new Xamarin.Forms.Maps.Position(Map.CenterCoordinate.Latitude, Map.CenterCoordinate.Longitude);
            RendererCall.RaiseCameraPositionChange(position);
        }

        private void OnDidUpdateUserLocation(object sender, MKUserLocationEventArgs e)
        {
            RendererCall.RaiseUserPositionChange(new Xamarin.Forms.Maps.Position(e.UserLocation.Coordinate.Latitude, e.UserLocation.Coordinate.Longitude));
        }

		private void OnMapClicked(UITapGestureRecognizer recognizer)
		{
			if (recognizer.State != UIGestureRecognizerState.Ended) return;

			var pixelLocation = recognizer.LocationInView(this.Map);
			var coordinate = this.Map.ConvertPoint(pixelLocation, this.Map);

            RendererCall.RaiseMapClick(new Xamarin.Forms.Maps.Position(coordinate.Latitude, coordinate.Longitude));
		}


        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (Map == null) return;

            var formsMap = (Libmemo.CustomElements.CustomMap.Map)sender;

			if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.CameraPositionProperty.PropertyName) {
                SetCameraPosition(formsMap.CameraPosition);
				return;
			}

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.IsGesturesEnabledProperty.PropertyName) {
                SetGestures(formsMap.IsGesturesEnabled);
                return;
            }

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.PinsProperty.PropertyName) {
                SetPins(formsMap.Pins);
				return;
            }

			if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.SelectedPinProperty.PropertyName)
			{
                
                if (FormMap.SelectedPin == null) 
                {
                    if (_selectedPin == null)
                    {
                        return;
                    }
                    else 
                    {
                        selectedPinChanging = true;
                        var selectedAnno = PinAnnotations[_selectedPin];
						_selectedPin = null;
                        Map.DeselectAnnotation(selectedAnno, true);
                        selectedPinChanging = false;
                        return;
                    }
                }
                else 
                {
                    if (FormMap.SelectedPin == _selectedPin)
                    {
                        return;
                    }
                    else 
                    {
                        selectedPinChanging = true;
						var selectedAnno = PinAnnotations[FormMap.SelectedPin];
						_selectedPin = FormMap.SelectedPin;
                        Map.SelectAnnotation(selectedAnno, true);
                        selectedPinChanging = false;
                        return;
                    }
                }
			}

            if (e.PropertyName == Libmemo.CustomElements.CustomMap.Map.RouteProperty.PropertyName)
            {
                if (Map.Overlays != null && Map.Overlays.Length > 0) 
                {
                    Map.RemoveOverlays(Map.Overlays);
                }

                if (formsMap.Route != null && formsMap.Route.Count >= 2)
				{
                    polylineRenderer = null;
                    var coords = FormMap.Route.Select(i => new CoreLocation.CLLocationCoordinate2D(i.Latitude, i.Longitude)).ToArray();
                    var route = MKPolyline.FromCoordinates(coords);
                    Map.AddOverlay(route);               
                }

            }

        }

        private void OnPinPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
			var pin = (Libmemo.CustomElements.CustomMap.Pin)sender;
            var annotation = PinAnnotations[pin];

            Map.RemoveAnnotation(annotation);
            var newAnnotation = GetAnnotation(pin);
            Map.AddAnnotation(newAnnotation);
            PinAnnotations[pin] = newAnnotation;
        }

		private MKAnnotationView GetViewForAnnotation(MKMapView mapView, IMKAnnotation annotation)
		{
			MKAnnotationView annotationView = null;

            if (!(annotation is MKIdPointAnnotation)) 
                return null;

			var anno = annotation as MKIdPointAnnotation;

            var pin = PinAnnotations.First(i => i.Value.Id == anno.Id).Key;

            annotationView = mapView.DequeueReusableAnnotation(pin.Id.ToString());
			if (annotationView == null)
			{
                var icon = new UIImageView(UIImage.FromFile("info.png"));
                if (pin.Icon != null)
                {
                    if (AnnotationsIconsDownloaded.ContainsKey(pin.Icon))
                    {
                        icon.Image = UIImage.LoadFromData(AnnotationsIconsDownloaded[pin.Icon]);
                    }
                    else 
                    {
						Task.Run(async () => 
                        {
                            Foundation.NSData data;
							try
							{
								var request = (HttpWebRequest)WebRequest.Create(pin.Icon);
								request.Method = "GET";
								request.Timeout = 5000;

								using (var responce = await request.GetResponseAsync())
								{
                                    var download = responce.GetResponseStream();
                                    data = Foundation.NSData.FromStream(download);
								}
							}
							catch
							{
								data = null;
							}

							if (data != null)
							{
                                AnnotationsIconsDownloaded[pin.Icon] = data;
                                Device.BeginInvokeOnMainThread(() => 
                                {
                                    icon.Image = UIImage.LoadFromData(data);
                                });
							}
						});
                    }
                }

                annotationView = new MKIdAnnotationView(annotation, pin.Id.ToString())
                {
                    Id = pin.Id,
                    Image = pin.PinImage == CustomElements.CustomMap.PinImage.Speakable
                               ? UIImage.FromFile("speaker_pin.png")
                               : UIImage.FromFile("default_pin.png"),
                    CalloutOffset = new CoreGraphics.CGPoint(0, 0),
                    LeftCalloutAccessoryView = icon,
                    RightCalloutAccessoryView = UIButton.FromType(UIButtonType.DetailDisclosure)
				};
			}
            annotationView.CanShowCallout = FormMap.IsShowInfoWindow;
			return annotationView;
		}




        private MKIdPointAnnotation GetAnnotation(CustomElements.CustomMap.Pin pin)
        {
			return new MKIdPointAnnotation
			{
				Id = pin.Id,
				Title = pin.Title,
				Subtitle = pin.Text,
				Coordinate = new CoreLocation.CLLocationCoordinate2D(pin.Position.Latitude, pin.Position.Longitude)
			};
        }

		private MKOverlayRenderer GetOverlayRenderer(MKMapView mapView, IMKOverlay overlayWrapper)
		{
			if (polylineRenderer == null && !Equals(overlayWrapper, null))
			{
                var overlay = ObjCRuntime.Runtime.GetNSObject(overlayWrapper.Handle) as IMKOverlay;
				polylineRenderer = new MKPolylineRenderer(overlay as MKPolyline)
				{
					FillColor = UIColor.Blue,
					StrokeColor = UIColor.Red,
					LineWidth = 3,
					Alpha = 0.4f
				};
			}
			return polylineRenderer;
		}



        private void SetGestures(bool isEnabled)
        {
            Map.ZoomEnabled = isEnabled;
			Map.PitchEnabled = isEnabled;
			Map.RotateEnabled = isEnabled;
			Map.ScrollEnabled = isEnabled;
        }

        private void SetCameraPosition(Xamarin.Forms.Maps.Position position, bool animate = true)
        {
            Map.SetCenterCoordinate(new CoreLocation.CLLocationCoordinate2D(position.Latitude, position.Longitude), animate);
        }

        private void SetPins(IEnumerable<CustomElements.CustomMap.Pin> pins)
        {
			ClearAnnotations();

			if (pins != null)
			{
				foreach (var pin in pins)
				{
					pin.PropertyChanged += OnPinPropertyChanged;
					var anno = GetAnnotation(pin);
					Map.AddAnnotation(anno);
					PinAnnotations[pin] = anno;
				}
			}
        }
    }

    public class MKIdPointAnnotation : MKPointAnnotation
    {
        public int Id { get; set; }
    }

    public class MKIdAnnotationView : MKAnnotationView
    {
        public int Id { get; set; }

        public MKIdAnnotationView(IMKAnnotation annotation, string reuseIdentifier) : base(annotation, reuseIdentifier) {}
    }
}
