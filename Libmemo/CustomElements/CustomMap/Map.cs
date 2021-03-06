﻿using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using CustomMaps = Libmemo.CustomElements.CustomMap;

namespace Libmemo.CustomElements.CustomMap
{
    public class Map : Xamarin.Forms.Maps.Map, IMapRendererCallable
    {
		public static readonly BindableProperty CameraPositionProperty = BindableProperty.Create(
			nameof(CameraPosition),
			typeof(Position),
			typeof(CustomMaps.Map),
			default(Position),
			BindingMode.TwoWay
		);
        public static readonly BindableProperty IsGesturesEnabledProperty = BindableProperty.Create(
            nameof(IsGesturesEnabled),
            typeof(bool),
            typeof(CustomMaps.Map),
            false
        );
        public static readonly BindableProperty PinsProperty = BindableProperty.Create(
            nameof(Pins),
            typeof(List<CustomMaps.Pin>),
            typeof(CustomMaps.Map)
        );
        public static readonly BindableProperty SelectedPinProperty = BindableProperty.Create(
            nameof(SelectedPin),
            typeof(CustomMaps.Pin),
            typeof(CustomMaps.Map),
            default(CustomMaps.Pin),
            BindingMode.TwoWay
        );
        public static readonly BindableProperty RouteProperty = BindableProperty.Create(
            nameof(Route),
            typeof(List<Position>),
            typeof(CustomMaps.Map)
        );
        public static readonly BindableProperty IsShowInfoWindowProperty = BindableProperty.Create(
			nameof(IsShowInfoWindow),
			typeof(bool),
			typeof(CustomMaps.Map),
			false
        );

        public static readonly BindableProperty InfoWindowClickedCommandProperty = BindableProperty.Create(
            nameof(InfoWindowClickedCommand),
            typeof(ICommand),
            typeof(CustomMaps.Map)
        );
        public static readonly BindableProperty UserPositionChangedCommandProperty = BindableProperty.Create(
            nameof(UserPositionChangedCommand),
            typeof(ICommand),
            typeof(CustomMaps.Map)
        );
		public static readonly BindableProperty MapClickCommandProperty = BindableProperty.Create(
        	nameof(MapClickCommand),
            typeof(ICommand),
            typeof(CustomMaps.Map)
        );




        public Map() : base() { }
        public Map(MapSpan region) : base(region) { }

        public Map(double latitude, double longitude, double height) 
            : base(MapSpan.FromCenterAndRadius(new Position(latitude, longitude), new Distance(height))) 
        {
            
        }


        public Position CameraPosition
        {
            get => (Position)this.GetValue(CameraPositionProperty); 
            set => this.SetValue(CameraPositionProperty, value); 
        }
        public bool IsGesturesEnabled 
        {
            get => (bool)this.GetValue(IsGesturesEnabledProperty);
            set => this.SetValue(IsGesturesEnabledProperty, value);
        }
        public new List<CustomMaps.Pin> Pins
        {
            get => (List<CustomMaps.Pin>)this.GetValue(PinsProperty);
            set => this.SetValue(PinsProperty, value); 
        }
        public CustomMaps.Pin SelectedPin
        {
            get => (CustomMaps.Pin)this.GetValue(SelectedPinProperty);
            set => this.SetValue(SelectedPinProperty, value);
        }
        public List<Position> Route
        {
            get => (List<Position>)this.GetValue(RouteProperty);
            set => this.SetValue(RouteProperty, value);
        }
        public bool IsShowInfoWindow 
        {
            get => (bool)this.GetValue(IsShowInfoWindowProperty);
            set => this.SetValue(IsShowInfoWindowProperty, value);
        }

        public ICommand InfoWindowClickedCommand
        {
            get => (ICommand)this.GetValue(InfoWindowClickedCommandProperty);
            set => this.SetValue(InfoWindowClickedCommandProperty, value);
        }
		public ICommand UserPositionChangedCommand
		{
			get => (ICommand)this.GetValue(UserPositionChangedCommandProperty);
			set => this.SetValue(UserPositionChangedCommandProperty, value);
		}
		public ICommand MapClickCommand
		{
			get => (ICommand)this.GetValue(MapClickCommandProperty);
			set => this.SetValue(MapClickCommandProperty, value);
		}




		void IMapRendererCallable.RaiseCameraPositionChange(Position position)
		{
            this.CameraPosition = position; 
		}
        void IMapRendererCallable.RaiseInfoWindowClick(CustomMaps.Pin pin)
        {
            var command = this.InfoWindowClickedCommand;
            if (command != null && command.CanExecute(pin)) {
                command.Execute(pin);
            }
        }
		void IMapRendererCallable.RaiseUserPositionChange(Position position)
		{
			var command = this.UserPositionChangedCommand;
			if (command != null && command.CanExecute(position)) {
				command.Execute(position);
			}
		}
        void IMapRendererCallable.RaiseMapClick(Position position)
        {
            var command = this.MapClickCommand;
            if (command != null && command.CanExecute(position)) {
                command.Execute(position);
            }
        }

        void IMapRendererCallable.RaiseSelectedPinSelect(Pin pin)
        {
            this.SelectedPin = pin;
        }


    }

    public interface IMapRendererCallable
    {
        void RaiseCameraPositionChange(Position position);
        void RaiseSelectedPinSelect(CustomMaps.Pin pin);

        void RaiseInfoWindowClick(CustomMaps.Pin pin);
		void RaiseUserPositionChange(Position position);
		void RaiseMapClick(Position position);
    }
}
