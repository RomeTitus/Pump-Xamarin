using System.Collections.Generic;
using System.Linq;
using Pump.IrrigationController;
using Pump.Layout.Views;
using Xamarin.Forms;

namespace Pump.Layout;

public static class LayoutHelper
{
    public static void RemoveUnusedViews(this Layout<View> layoutView, List<string> itemsThatAreOnDisplay)
    {
        for (var index = 0; index < layoutView.Children.Count; index++)
        {
            var existingItems = itemsThatAreOnDisplay.FirstOrDefault(x =>
                x == layoutView.Children[index].AutomationId);
            if (existingItems != null) continue;
            layoutView.Children.RemoveAt(index);
            index--;
        }
    }
    
    public static void RemoveUnusedViews(this Layout<View> layoutView, string itemsThatAreOnDisplay)
    {
        RemoveUnusedViews(layoutView, new List<string> { itemsThatAreOnDisplay });
    }
    
    public static Layout<View> DisplayActivityLoading(this Layout<View> layoutView)
    {
        var loadingIcon = new ActivityIndicator
        {
            AutomationId = "ActivityIndicatorSiteLoading",
            HorizontalOptions = LayoutOptions.Center,
            IsEnabled = true,
            IsRunning = true,
            IsVisible = true,
            VerticalOptions = LayoutOptions.Center
        };
            
        if (layoutView.Children.Count == 0)
        {
            layoutView.Children.Add(loadingIcon);
        }

        return layoutView;
    }
    
    public static Layout<View> AddUpdateRemoveStatus(this Layout<View> layoutView, ControllerStatus status)
    {
        var viewStatus = (ViewStatus) layoutView.Children.FirstOrDefault(x => x is ViewStatus);
        
        if (status is not null && status.Complete == false && status.Steps == null)
        {
            layoutView.AddActivityIndicator();
        }
        else if (status is not null)
        {
            if (viewStatus is not null)
                viewStatus.UpdateView(status);
            else
            {
                layoutView.Children.Clear();
                layoutView.Children.Add(new ViewStatus(status));
            }
        }
        else
        {
            layoutView.Children.Clear();
        }
        
        return layoutView;
    }

    private static void AddActivityIndicator(this Layout<View> layoutView)
    {
        var existingActivityIndicator = (ActivityIndicator) layoutView.Children.FirstOrDefault(x => x is ActivityIndicator);

        if (existingActivityIndicator is not null)
            return;
        
        AddStatusActivityIndicator(layoutView);
    }
    
    public static Layout<View> AddStatusActivityIndicator(this Layout<View> layoutView)
    {
        var existingActivityIndicator = (ActivityIndicator) layoutView.Children.FirstOrDefault(x => x is ActivityIndicator);
        if(existingActivityIndicator is not null)
            return layoutView;
            
        layoutView.Children.Clear();
            
        Device.BeginInvokeOnMainThread(() =>
        {
            var loadingIcon = new ActivityIndicator
            {
                AutomationId = "ActivityIndicatorSiteLoading",
                HorizontalOptions = LayoutOptions.Center,
                IsEnabled = true,
                IsRunning = true,
                IsVisible = true,
                VerticalOptions = LayoutOptions.Center
            };
            layoutView.Children.Add(loadingIcon);

        });
        return layoutView;
    }
}