using System;
using System.Collections.Generic;
using System.Linq;
using Microcharts;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Pump.Layout
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChartEntryFilterScreen : ContentPage
    {
        private readonly List<ChartEntry> _chartEntries = new List<ChartEntry>();
        private readonly List<CheckBox> _checkBoxes = new List<CheckBox>();
        private readonly List<string> _excludedEntries = new List<string>();

        public ChartEntryFilterScreen(List<ChartEntry> chartEntries, List<string> excludedEntries)
        {
            InitializeComponent();
            _chartEntries = chartEntries;
            _excludedEntries = excludedEntries;
            Populate();
        }

        private void Populate()
        {
            foreach (var chartEntry in _chartEntries)
            {
                
                var stackLayout = new StackLayout
                {
                    HorizontalOptions = LayoutOptions.FillAndExpand, Orientation = StackOrientation.Horizontal,
                    VerticalOptions = LayoutOptions.Start,
                    Children =
                    {
                        new Label{HorizontalOptions = LayoutOptions.StartAndExpand, Text = chartEntry.Label, VerticalOptions = LayoutOptions.CenterAndExpand},
                        new Label{HorizontalOptions = LayoutOptions.EndAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand, Text = chartEntry.ValueLabel + "  "},
                        }
                };
                var filterCheckBox = new CheckBox
                {
                    HorizontalOptions = LayoutOptions.EndAndExpand, VerticalOptions = LayoutOptions.CenterAndExpand,
                    Color = Color.Black,
                    AutomationId = chartEntry.Label
                };
                if (!_excludedEntries.Contains(filterCheckBox.AutomationId))
                    filterCheckBox.IsChecked = true;
                stackLayout.Children.Add(filterCheckBox);
                ChartEntryFilterLayout.Children.Add(stackLayout);
                _checkBoxes.Add(filterCheckBox);
            }
        }

        public StackLayout GetLayout()
        {
            return ChartEntryFilterLayout;
        }

        private void Button_OnClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PopAsync();
        }

        public List<CheckBox> GetCheckBoxes()
        {
            return _checkBoxes;
        }
    }
}