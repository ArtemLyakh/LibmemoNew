using System;
using Android.Gms.Maps;
using Android.Gms.Maps.Model;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Maps.Android;

[assembly: ExportRenderer(typeof(Libmemo.CustomElements.CustomMap.Map), typeof(Libmemo.Droid.Renderers.CustomMapRenderer))]
namespace Libmemo.Droid.Renderers
{
    public class CustomMapRenderer : MapRenderer, GoogleMap.IInfoWindowAdapter, IOnMapReadyCallback
    {
        private CustomElements.CustomMap.IMapRendererCallable RendererCall => (CustomElements.CustomMap.IMapRendererCallable)this.Element;
        private CustomElements.CustomMap.Map FormMap => (CustomElements.CustomMap.Map)this.Element;

        private GoogleMap _googleMap;
        private bool IsDrawn;







        protected override void OnElementChanged(Xamarin.Forms.Platform.Android.ElementChangedEventArgs<Xamarin.Forms.Maps.Map> e)
        {
            base.OnElementChanged(e);

            if (e.OldElement != null)
            {
                var formsMap = (Libmemo.CustomElements.CustomMap.Map)e.NewElement;

                formsMap.PropertyChanged -= FormMapPropertyChanged;
                formsMap.Pins.CollectionChanged -= FormsMapPinChanged;

            }

            if (e.NewElement != null)
            {
                var formsMap = (Libmemo.CustomElements.CustomMap.Map)e.NewElement;

                formsMap.PropertyChanged += FormMapPropertyChanged;
                formsMap.Pins.CollectionChanged += FormsMapPinChanged;


                Control.GetMapAsync(this);
            }
        }

        void IOnMapReadyCallback.OnMapReady(GoogleMap googleMap)
        {
            _googleMap = googleMap;
            _googleMap.SetInfoWindowAdapter(this);


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

        }
        private void FormsMapPinChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {

        }


        private void Clear()
        {
            
        }

        private void Draw()
        {
            
        }


        Android.Views.View GoogleMap.IInfoWindowAdapter.GetInfoContents(Marker marker)
        {
            throw new NotImplementedException();
        }

        Android.Views.View GoogleMap.IInfoWindowAdapter.GetInfoWindow(Marker marker)
        {
            throw new NotImplementedException();
        }
    }
}
