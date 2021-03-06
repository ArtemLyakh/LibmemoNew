﻿using System;
using System.Windows.Input;
using Xamarin.Forms;

namespace Libmemo
{

	public class CustomListView : ListView
	{

		public static readonly BindableProperty ItemClickCommandProperty = BindableProperty.Create(
			nameof(ItemClickCommand),
			typeof(Command<object>),
			typeof(CustomListView));

		public ICommand ItemClickCommand
		{
			get { return (ICommand)this.GetValue(ItemClickCommandProperty); }
			set { this.SetValue(ItemClickCommandProperty, value); }
		}

		public CustomListView()
		{
			this.ItemTapped += this.OnItemTapped;
		}

		public CustomListView(ListViewCachingStrategy strategy) : base(strategy)
		{
			this.ItemTapped += this.OnItemTapped;
		}


		private void OnItemTapped(object sender, ItemTappedEventArgs e)
		{
			if (e.Item != null && this.ItemClickCommand != null && this.ItemClickCommand.CanExecute(e.Item))
			{
				this.ItemClickCommand.Execute(e.Item);
				this.SelectedItem = null;
			}
		}
	}
}
