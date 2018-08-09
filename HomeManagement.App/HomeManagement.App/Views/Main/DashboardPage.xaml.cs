﻿using HomeManagement.App.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace HomeManagement.App.Views.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class DashboardPage : ContentPage
	{
        DashboardViewModel dashboardViewModel = new DashboardViewModel();

        public DashboardPage ()
		{
			InitializeComponent ();

            BindingContext = dashboardViewModel;
        }
    }
}