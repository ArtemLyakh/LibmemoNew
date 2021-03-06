﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages.Core
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Menu : ContentPage
    {
		public ListView ListView { get { return this.listView; } }

		public class MenuItem
		{
			public ImageSource Image { get; set; }
			public string Text { get; set; }
			public Func<Task> Action { get; set; }
		}

		public ObservableCollection<MenuItem> MenuList { get; set; } = new ObservableCollection<MenuItem>();


		public Menu()
		{
			InitializeComponent();
			SetMenuPage();
			this.BindingContext = this;
		}

		public void SetMenuPage()
		{
			MenuList.Clear();
			foreach (var item in GetMenuList())
			{
				MenuList.Add(item);
			}
			OnPropertyChanged(nameof(IsLogged));
			OnPropertyChanged(nameof(IsAdmin));
		}



		private static IEnumerable<MenuItem> GetMenuList()
		{
			yield return new MenuItem
			{
				Text = "Карта",
				Image = ImageSource.FromFile("menu_map"),
                Action = () => App.GlobalPage.PushRoot(new Pages.Map.Filter())
			};

			if (!AuthHelper.IsLogged)
			{
                yield return new MenuItem
                {
                    Text = "Авторизация",
                    Image = ImageSource.FromFile("menu_login"),
                    Action = () => App.GlobalPage.PushRoot(new Pages.Auth.Login())
				};
				yield return new MenuItem
				{
					Text = "Регистрация",
					Image = ImageSource.FromFile("menu_reg"),
                    Action = () => App.GlobalPage.PushRoot(new Pages.Auth.Register())
				};
			}
			else
			{
				if (AuthHelper.IsAdmin)
				{
					yield return new MenuItem
					{
						Text = "Список пользователей",
						Image = ImageSource.FromFile("menu_login"),
						Action = () => App.GlobalPage.Push(new Pages.Admin.Account.List(async id => {
							await App.GlobalPage.Push(new Pages.Admin.Account.Account(id));
						}))
					};
					yield return new MenuItem
					{
						Text = "Родственники пользователей",
						Image = ImageSource.FromFile("menu_rel"),
						Action = () => App.GlobalPage.PushRoot(new Pages.Admin.Relatives.List(async id => {
							await App.GlobalPage.Push(new Pages.Admin.Relatives.Edit(id));
						}))
					};
					yield return new MenuItem
					{
						Text = "Деревья пользователей",
						Image = ImageSource.FromFile("menu_tree"),
						Action = () => App.GlobalPage.PushRoot(new Pages.Admin.Account.List(async id => {
							await App.GlobalPage.Push(new Pages.Admin.Tree.Tree(id));
						}))
					};
				}
				else
				{
					yield return new MenuItem
					{
						Text = "Родственники",
						Image = ImageSource.FromFile("menu_rel"),
                        Action = () => App.GlobalPage.PushRoot(new Pages.Relatives.Relatives())
					};
					yield return new MenuItem
					{
						Text = "Древо",
						Image = ImageSource.FromFile("menu_tree"),
                        Action = () => App.GlobalPage.PushRoot(new Pages.Tree.Tree())
					};
				}

				yield return new MenuItem
				{
					Text = "Выйти",
					Image = ImageSource.FromFile("menu_exit"),
					Action = () => AuthHelper.Logout()
				};
			}
		}

		public ICommand BackCommand => new Command(() => App.SetShowMenu(false));

		public ICommand LKCommand => new Command(async () => {
			if (!AuthHelper.IsLogged || AuthHelper.IsAdmin) return;
            await App.GlobalPage.PushRoot(new Pages.Account.Account());
			App.SetShowMenu(false);
		});

		public bool IsLogged => AuthHelper.IsLogged;
		public bool IsAdmin => AuthHelper.IsAdmin;
    }
}
