﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Map
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class Search : ContentPage
	{
		private ViewModel Model
		{
			get => (ViewModel)BindingContext;
			set => BindingContext = value;
		}

		public Search(List<Models.DeadPerson> data, Action<int> cb)
		{
			InitializeComponent();
			BindingContext = new ViewModel(data, cb);
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
			private Action<int> CallBack { get; set; }

			public ViewModel(List<Models.DeadPerson> data, Action<int> cb) : base()
			{
				FullData = data.Select(i => new Entry(i.Id, i.FIO)
				{
					IsAlive = !(i is Models.DeadPerson),
					Image = (i.PreviewImage != null)
						? ImageSource.FromUri(i.PreviewImage)
						: ImageSource.FromFile("no_img.png")
				}).ToList();
				CallBack = cb;

				FilterCommand.Execute(null);
			}


			public class Entry
			{
				public int Id { get; set; }
				public bool IsAlive { get; set; }
				public string Text { get; set; }
				public ImageSource Image { get; set; }

				public Entry(int id, string text)
				{
					Id = id;
					Text = text;
				}
			}

			private List<Entry> FullData { get; set; }

			private List<Entry> _data;
			public List<Entry> Data
			{
				get => _data;
				set
				{
					_data = value;
					OnPropertyChanged(nameof(Data));
				}
			}

            private string _search;
            public string Search
			{
				get => _search;
				set
				{
					if (_search != value)
					{
						_search = value;
						OnPropertyChanged(nameof(Search));
					}
				}
			}


			public ICommand FilterCommand => new Command(() => {
				if (string.IsNullOrWhiteSpace(this.Search))
				{
					this.Data = this.FullData;
				}
				else
				{
					this.Data = FullData
						.Where(i => i.Text.ToLowerInvariant().IndexOf(this.Search.ToLowerInvariant(), StringComparison.Ordinal) != -1)
						.ToList();
				}
			});

			public ICommand ItemSelectedCommand => new Command<object>(selected => {
				var entry = (Entry)selected;
				CallBack.Invoke(entry.Id);
			});
		}
	}
}