using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CefSharp;
using CefSharp.WinForms;

namespace InsideCEF.WinForms
{
  public partial class CefWindow : Form
  {

    public static ChromiumWebBrowser Browser;
    public Interop Interop;

    public CefWindow()
    {
      InitializeComponent();

      if (!Cef.IsInitialized)
        InitializeCef();

      // initialise one browser instance
      InitializeBrowser();

      Controls.Add(Browser);

    }

    private void InitializeCef()
    {
      Cef.EnableHighDPISupport();

      var assemblyLocation = Assembly.GetExecutingAssembly().Location;
      var assemblyPath = Path.GetDirectoryName(assemblyLocation);
      var pathSubprocess = Path.Combine(assemblyPath, "CefSharp.BrowserSubprocess.exe");
      CefSharpSettings.LegacyJavascriptBindingEnabled = true;
      var settings = new CefSettings
      {
        LogSeverity = LogSeverity.Verbose,
        LogFile = "ceflog.txt",
        BrowserSubprocessPath = pathSubprocess,

      };

      settings.CefCommandLineArgs.Add("allow-file-access-from-files", "1");
      settings.CefCommandLineArgs.Add("disable-web-security", "1");
      Cef.Initialize(settings);

    }

    private void InitializeBrowser()
    {
#if DEBUG
      //use localhost
      // Browser = new ChromiumWebBrowser(@"http://localhost:7070/");
      // Browser = new ChromiumWebBrowser(@"http://rhino3d.com");
      Browser = new ChromiumWebBrowser(@"C:\dev\mcneel\Rhino.Inside\Node.js\Sample-5\InsideCEF.WebApp\index.html");
#else
      //use dist files
      var path = Directory.GetParent(Assembly.GetExecutingAssembly().Location);
      Debug.WriteLine(path, "InsideCEF");

      var indexPath = string.Format(@"{0}\app\index.html", path);

      if (!File.Exists(indexPath))
        Debug.WriteLine("Error. The html file doesn't exists : {0}", "InsideCEF");

      indexPath = indexPath.Replace("\\", "/");

      Browser = new ChromiumWebBrowser(indexPath);
#endif
      // Allow the use of local resources in the browser
      Browser.BrowserSettings = new BrowserSettings
      {
        FileAccessFromFileUrls = CefState.Enabled,
        UniversalAccessFromFileUrls = CefState.Enabled
      };

      Browser.Dock = System.Windows.Forms.DockStyle.Fill;

      Interop = new Interop(Browser);
      Browser.RegisterAsyncJsObject("Interop", Interop);

      Browser.IsBrowserInitializedChanged += Browser_IsBrowserInitializedChanged;

      Browser.LoadingStateChanged += Browser_LoadingStateChanged;
    }

    private void Browser_IsBrowserInitializedChanged(object sender, IsBrowserInitializedChangedEventArgs e)
    {
      Debug.WriteLine("Browser Initialized");
    }

    private void Browser_LoadingStateChanged(object sender, LoadingStateChangedEventArgs e)
    {
      //Wait for the Page to finish loading
      if (e.IsLoading == false)
      {

        Browser.ShowDevTools();

        Interop.StartGrasshopper(null);
        //Debug.WriteLine("V4D Content loaded to UI", "V4D");


      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      Browser.Dispose();
      Cef.Shutdown();

      base.OnClosing(e);
    }
  }
}
