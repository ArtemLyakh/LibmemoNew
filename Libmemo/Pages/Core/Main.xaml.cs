using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Libmemo.Pages.Core
{
    public partial class Main : MasterDetailPage
    {
        public Main()
        {
            InitializeComponent();

            (Master as Menu).ListView.ItemSelected += OnMenuItemSelected;
        }

		private NavigationPage NavStack
		{
			get => (NavigationPage)this.Detail;
		}

		public async Task PopToRootPage() => await NavStack.Navigation.PopToRootAsync();
		public async Task Pop() => await NavStack.Navigation.PopAsync();
		public async Task Push(Page page) => await NavStack.Navigation.PushAsync(page);
		public async Task PushRoot(Page page)
		{
			await PopToRootPage();
			await NavStack.Navigation.PushAsync(page);
		}

		private async void OnMenuItemSelected(object sender, SelectedItemChangedEventArgs e)
		{
            var item = e.SelectedItem as Menu.MenuItem;
			if (item != null)
			{
                (Master as Menu).ListView.SelectedItem = null;

				await item.Action?.Invoke();
				IsPresented = false;
			}
		}
    }
}
