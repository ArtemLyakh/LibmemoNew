using System;
using System.Collections.Generic;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Libmemo.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Start : ContentPage
    {
        private ViewModel Model
        {
            get => (ViewModel)BindingContext;
            set => BindingContext = value;
        }

        public Start()
        {
            InitializeComponent();
            BindingContext = new ViewModel();
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
            
            public ViewModel() : base()
            {
                CheckAuth();
                AuthHelper.AuthChanged += (sender, e) => CheckAuth();
            }

            private bool _isShowAuth;
            public bool IsShowAuth
            {
                get => _isShowAuth;
                set {
                    if (_isShowAuth != value) {
                        _isShowAuth = value;
                        OnPropertyChanged(nameof(IsShowAuth));
                    }
                }
            }

            private bool _isShowAccount;
            public bool IsShowAccount
            {
                get => _isShowAccount;
                set
                {
                    if (_isShowAccount != value)
                    {
                        _isShowAccount = value;
                        OnPropertyChanged(nameof(IsShowAccount));
                    }
                }
            }

            private void CheckAuth() 
            {
                IsShowAuth = !AuthHelper.IsLogged;
                IsShowAccount = AuthHelper.IsLogged && !AuthHelper.IsAdmin;
            }

            public ICommand MapCommand => new Command(async () =>
            {
                await App.GlobalPage.Push(new Pages.Map.Filter());
            });

            public ICommand AuthCommand => new Command(async () =>
            {
                await App.GlobalPage.Push(new Pages.Auth.Login());
            });

            public ICommand AccountCommand => new Command(async () =>
            {
                await App.GlobalPage.Push(new Pages.Account.Account());
            });
        }
    }


}
