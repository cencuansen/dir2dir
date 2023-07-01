using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace dir2dir
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            var options = new CoreWebView2EnvironmentOptions("--disable-web-security");
            var environment = await CoreWebView2Environment.CreateAsync(null, null, options);
            await webView21.EnsureCoreWebView2Async(environment);
            var vueAppStaticPath = "D:\\ProjectSpace\\dir2dir\\vue-app\\dist";
            var vueAppRunningPath = "http://localhost:5173";
            webView21.CoreWebView2.SetVirtualHostNameToFolderMapping("app", vueAppStaticPath, CoreWebView2HostResourceAccessKind.Allow);
            webView21.CoreWebView2.Navigate(vueAppRunningPath);

            // 监听
            webView21.CoreWebView2.WebMessageReceived += ReceivedProcess;

            var mainWinObject = new MainWinObject(webView21);
            webView21.CoreWebView2.AddHostObjectToScript("host", mainWinObject);
        }


        void ReceivedProcess(object sender, CoreWebView2WebMessageReceivedEventArgs args)
        {

        }
    }
}
