using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

//HACK: Currently there is a bug in VS 2012 RTM: which causes a XAML parse exception when a XAML-based user control is loaded
//from a class lib, and the class lib dll or namespace of the control has a '.' in the name
//https://connect.microsoft.com/VisualStudio/feedback/details/758592/xamlparseexception-if-referenced-xaml-control-defined-in-class-library-has-its-output-assembly-name-or-namespace-contain-a-period
namespace MetroControls
{
    public sealed partial class WebViewHostControl : UserControl, INotifyPropertyChanged
    {
        private bool _showHeader = true;
        private string _headerText;
        private string _failedMessage;

        private string _uri;
        private string _localFallbackUri;

        private bool _webViewNavigationFailed;

        public event PropertyChangedEventHandler PropertyChanged;

        public WebViewHostControl()
        {
            this.InitializeComponent();

            this.DataContext = this;

            webView.NavigationFailed += OnWebViewNavigationFailed;
            webView.LoadCompleted += OnWebViewLoadCompleted;

            this.Loaded += OnControlLoaded;
            this.Unloaded += OnControlUnLoaded;
        }

        private void OnControlUnLoaded(object sender, RoutedEventArgs e)
        {
            if (webView != null)
            {
                webView.LoadCompleted -= OnWebViewLoadCompleted;
                webView.NavigationFailed -= OnWebViewNavigationFailed;
            }
        }

        public bool ShowHeader
        {
            get { return _showHeader; }
            set { SetProperty(ref _showHeader, value); }
        }

        public string HeaderText
        {
            get { return _headerText; }
            set
            {
                SetProperty(ref _headerText, value);
                OnPropertyChanged("FailedMessage");
            }
        }

        public string FailedMessage
        {
            get
            {
                //show a default failed message.
                if (string.IsNullOrEmpty(_failedMessage))
                    return string.Format("Could not load '{0}'", _headerText ?? string.Empty);

                return _failedMessage;
            }
            set
            {
                SetProperty(ref _failedMessage, value);
            }
        }

        public string Uri
        {
            get { return _uri; }
            set
            {
                SetProperty(ref _uri, value);
            }
        }

        public string LocalFallbackUri
        {
            get { return _localFallbackUri; }
            set { SetProperty(ref _localFallbackUri, value); }
        }

        private void OnControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Uri))
                NavigateAsync(Uri, false);
        }

        private void OnWebViewLoadCompleted(object sender, NavigationEventArgs e)
        {
            if (!_webViewNavigationFailed)
            {
                //Debug.WriteLine("Load completed, _webViewNavigationFailed is false");
                ShowWebView();
            }
        }

        private void OnWebViewNavigationFailed(object sender, WebViewNavigationFailedEventArgs e)
        {
            //try to get it from the local fallback uri
            _webViewNavigationFailed = true;
            //Debug.WriteLine("Nav failed: " + e.Uri + ", reason: " + e.WebErrorStatus);

            if (e.Uri.ToString() != LocalFallbackUri && !string.IsNullOrWhiteSpace(LocalFallbackUri))
                NavigateAsync(LocalFallbackUri, isLocal: true);
            else
                ShowLoadingFailedMessage();
        }

        private void ShowLoadingFailedMessage()
        {
            HideWebView(showProgress: false);
            failedMessage.Visibility = Visibility.Visible;
        }

        private async void NavigateAsync(string uri, bool isLocal = false)
        {
            HideWebView();
            try
            {
                //Debug.WriteLine("Navigating to " + uri);

                if (!isLocal)
                    webView.Navigate(new Uri(uri));
                else
                {
                    var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(uri));
                    var randomAccessStream = await file.OpenReadAsync();
                    using (var stream = randomAccessStream.AsStreamForRead())
                    {
                        using (var streamReader = new StreamReader(stream))
                        {
                            var fileContents = await streamReader.ReadToEndAsync();
                            if (!string.IsNullOrWhiteSpace(fileContents))
                            {
                                webView.NavigateToString(fileContents);
                            }
                        }
                    }
                }
                _webViewNavigationFailed = false;
            }
            catch (Exception ex)
            {
                //This should be replaced with proper runtime logging
                Debug.WriteLine("Error navigating: " + ex);
                ShowLoadingFailedMessage();
            }
        }

        private void ShowWebView()
        {
            webView.Visibility = Visibility.Visible;
            blockingRect.Visibility = Visibility.Collapsed;
            failedMessage.Visibility = Visibility.Collapsed;
            progressRing.IsActive = false;
        }

        private void HideWebView(bool showProgress = true)
        {
            webView.Visibility = Visibility.Collapsed;
            blockingRect.Visibility = Visibility.Visible;
            progressRing.IsActive = showProgress;
        }

        private bool SetProperty<T>(ref T storage, T value,
            [CallerMemberName] 
            String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(
            [CallerMemberName] 
            string propertyName = null)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
