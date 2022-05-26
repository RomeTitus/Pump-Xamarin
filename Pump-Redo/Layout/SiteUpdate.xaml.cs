using System;
using System.Collections.Generic;
using System.Reflection;
using EmbeddedImages;
using Pump.IrrigationController;
using Pump.SocketController;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SiteUpdate : ContentPage
    {
        private readonly List<Equipment> _equipmentList;
        private readonly List<Sensor> _sensorList;
        private readonly Site _site;
        private readonly SocketPicker _socketPicker;

        public SiteUpdate(List<Sensor> sensorList, List<Equipment> equipmentList, SocketPicker socketPicker,
            Site site = null)
        {
            InitializeComponent();
            _socketPicker = socketPicker;
            if (site == null)
            {
                site = new Site();
                ButtonUpdateSite.Text = "Create";
            }

            _site = site;
            _sensorList = sensorList;
            _equipmentList = equipmentList;
            PopulateScreen();
        }

        private async void ButtonUpdateSite_OnClicked(object sender, EventArgs e)
        {
            _site.NAME = SiteName.Text;
            _site.Description = SiteDescription.Text;
            _site.Attachments.Clear();
            foreach (var view in ScrollViewSiteSelection.Children)
            {
                var stackLayout = (StackLayout)view;
                var checkBox = (CheckBox)stackLayout.Children[0];
                if (checkBox.IsChecked)
                    _site.Attachments.Add(stackLayout.AutomationId);
            }

            await _socketPicker.SendCommand(_site);
            await Navigation.PopModalAsync();
        }

        private void ButtonBack_OnClicked(object sender, EventArgs e)
        {
            Navigation.PopModalAsync();
        }

        private void PopulateScreen()
        {
            if (!string.IsNullOrEmpty(_site.NAME))
                SiteName.Text = _site.NAME;
            if (!string.IsNullOrEmpty(_site.Description))
                SiteDescription.Text = _site.Description;

            foreach (var equipment in _equipmentList)
            {
                PopulateSelectionView(equipment);
            }

            foreach (var sensor in _sensorList)
            {
                PopulateSelectionView(sensor);
            }
        }

        private void PopulateSelectionView(Equipment equipment)
        {
            bool isSelected = _site.Attachments.Contains(equipment.ID);
            var image = equipment.isPump ? SavedImage.Pump : SavedImage.zone;
            ScrollViewSiteSelection.Children.Add(new StackLayout
                {
                    AutomationId = equipment.ID,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    Children =
                    {
                        new CheckBox { HorizontalOptions = LayoutOptions.Start, IsChecked = isSelected },
                        new Image
                        {
                            HorizontalOptions = LayoutOptions.Start,
                            HeightRequest = 40,
                            Source = ImageSource.FromResource(
                                image,
                                typeof(ImageResourceExtention).GetTypeInfo().Assembly)
                        },
                        new Label
                        {
                            VerticalOptions = LayoutOptions.Center,
                            Text = equipment.NAME
                        }
                    }
                }
            );
        }

        private void PopulateSelectionView(Sensor sensor)
        {
            bool isSelected = _site.Attachments.Contains(sensor.ID);
            var image = SavedImage.Sensor;
            ScrollViewSiteSelection.Children.Add(new StackLayout
                {
                    AutomationId = sensor.ID,
                    Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.StartAndExpand,
                    Children =
                    {
                        new CheckBox { HorizontalOptions = LayoutOptions.Start, IsChecked = isSelected },
                        new Image
                        {
                            HorizontalOptions = LayoutOptions.Start,
                            HeightRequest = 40,
                            Source = ImageSource.FromResource(
                                image,
                                typeof(ImageResourceExtention).GetTypeInfo().Assembly)
                        },
                        new Label
                        {
                            VerticalOptions = LayoutOptions.Center,
                            Text = sensor.NAME
                        }
                    }
                }
            );
        }
    }

    public static class SavedImage
    {
        public const string Pump = "Pump-Redo.Icons.activePump.png";
        public const string zone = "Pump-Redo.Icons.sprinkler.jpg";
        public const string SubController = "Pump-Redo.Icons.SwitchController.png";
        public const string Sensor = "Pump-Redo.Icons.PressureLow.png";
    }
}