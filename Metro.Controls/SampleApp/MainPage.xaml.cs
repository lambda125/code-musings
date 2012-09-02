using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Metro.Controls.Demo
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

        private void OnSampleMenuItemClick(object sender, RoutedEventArgs e)
        {
            //Demo code only: Don't write stuff in code-behind like this :P
            var button = sender as Button;
            if (button != null)
            {
                var typeName = (string)button.Tag; //for types in the current assembly this will work ok
                var type = Type.GetType(typeName);
                SampleFrame.Navigate(type);
            }
        }
    }
}
