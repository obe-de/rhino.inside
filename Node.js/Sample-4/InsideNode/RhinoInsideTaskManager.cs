using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Rhino;
using Rhino.PlugIns;
using Rhino.Runtime.InProcess;
using Grasshopper;

namespace InsideNode
{
  /// <summary>
  /// Custom task manager inspired by https://www.infoworld.com/article/3063560/building-your-own-task-scheduler-in-c.html.
  /// </summary>
  public sealed class RhinoInsideTaskManager : TaskScheduler, IDisposable
  {
    private ConcurrentQueue<Task> tasksCollection = new ConcurrentQueue<Task>();
    //private BlockingCollection<Task> tasksCollection = new BlockingCollection<Task>();
    private readonly Thread mainThread = null;
    RhinoCore rhinoCore;

    static readonly Guid GrasshopperGuid = new Guid(0xB45A29B1, 0x4343, 0x4035, 0x98, 0x9E, 0x04, 0x4E, 0x85, 0x80, 0xD9, 0xCF);

    public RhinoInsideTaskManager()
    {

      ResolveEventHandler OnRhinoCommonResolve = null;
      ResolveEventHandler OnGrasshopperCommonResolve = null;

      AppDomain.CurrentDomain.AssemblyResolve += OnRhinoCommonResolve = (sender, args) =>
      {
        const string rhinoCommonAssemblyName = "RhinoCommon";
        var assemblyName = new AssemblyName(args.Name).Name;

        if (assemblyName != rhinoCommonAssemblyName)
          return null;

        AppDomain.CurrentDomain.AssemblyResolve -= OnRhinoCommonResolve;

        string rhinoSystemDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", "System");
        return Assembly.LoadFrom(Path.Combine(rhinoSystemDir, rhinoCommonAssemblyName + ".dll"));
      };

      AppDomain.CurrentDomain.AssemblyResolve += OnGrasshopperCommonResolve = (sender, args) =>
      {
        const string grasshopperAssemblyName = "Grasshopper";
        var assemblyName = new AssemblyName(args.Name).Name;

        if (assemblyName != grasshopperAssemblyName)
          return null;

        AppDomain.CurrentDomain.AssemblyResolve -= OnGrasshopperCommonResolve;

        string rhinoGHDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Rhino WIP", @"Plug-ins\Grasshopper");
        return Assembly.LoadFrom(Path.Combine(rhinoGHDir, grasshopperAssemblyName + ".dll"));
      };

      mainThread = new Thread(new ThreadStart(Execute));
      mainThread.TrySetApartmentState(ApartmentState.STA);
      if (!mainThread.IsAlive)
        mainThread.Start();
    }

    async Task<object> StartRhino(dynamic input)
    {
      // WindowStyle.Hidden: OK, with threaded solution.
      // WindowStyle.Normal: OK, with threaded solution.
      // WindowStyle.NoWindow: OK

      try
      {
        // Start Rhino
        // TODO: use input argument variables here
        rhinoCore = new RhinoCore(new string[] { "/NOSPLASH" }, WindowStyle.Hidden);

        // Subscribe to events
        RhinoApp.Idle += RhinoApp_Idle;
        RhinoApp.Initialized += RhinoApp_Initialized;

        return null;
      }
      catch (Exception ex)
      {
        return ex;
      }
    }

    private void RhinoApp_Initialized(object sender, EventArgs e)
    {
      Console.WriteLine("Rhino Initialized");
    }

    async Task<object> StartGrasshopper(dynamic input)
    {
      if (!PlugIn.LoadPlugIn(GrasshopperGuid))
        return null;

      return  Rhino.RhinoApp.RunScript("!_-Grasshopper _W _T ENTER", false) ? true : false;
    }

    /// <summary>
    /// Task to start Rhino.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>TODO: Add more meaningful return object.</returns>
    public async Task<object> StartRhinoTask(dynamic input)
    {
      await Task.Factory.StartNew(() => StartRhino(null), CancellationToken.None, TaskCreationOptions.None, this);
      return null;
    }

    /// <summary>
    /// Task to start Grasshopper.
    /// </summary>
    /// <param name="input"></param>
    /// <returns>TODO: Add more meaningful return object.</returns>
    public async Task<object> StartGrasshopperTask(dynamic input)
    {
      await Task.Factory.StartNew(() => StartGrasshopper(null), CancellationToken.None, TaskCreationOptions.None, this).ContinueWith(t => Console.WriteLine("Grasshopper Loaded."));
      return null;
    }

    public async Task<object> GrasshopperSubscribeTask(dynamic input)
    {
      // This will fail
      await Task.Factory.StartNew(() => OnGrasshopperLoaded(), CancellationToken.None, TaskCreationOptions.None, this);
      return null;
    }

    async Task<object> OnGrasshopperLoaded()
    {
      try
      {
        // This fails due to not finding System.Windows.Forms
        var canvas = Grasshopper.Instances.ActiveCanvas;
        canvas.DocumentChanged += Canvas_DocumentChanged;

      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        
      }
      return null;
    }

    private void Instances_CanvasCreated(Grasshopper.GUI.Canvas.GH_Canvas canvas)
    {
      Console.WriteLine("GH Canvas Created");
    }

    private void DocServer_DocumentAdded(Grasshopper.Kernel.GH_DocumentServer sender, Grasshopper.Kernel.GH_Document doc)
    {
      Console.WriteLine("GH Doc Added.");
    }

    private void Canvas_DocumentChanged(Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.Canvas.GH_CanvasDocumentChangedEventArgs e)
    {
      Console.WriteLine("GH: Doc Changed");
    }

    private void Execute()
    {
      if (rhinoCore == null)
      {
        StartRhino(null);
      }

      rhinoCore.Run();
    }

    private void RhinoApp_Idle(object sender, EventArgs e)
    {
      //foreach (var task in tasksCollection.GetConsumingEnumerable())
      //foreach (var task in tasksCollection)
      //  TryExecuteTask(task);

      while (tasksCollection.TryDequeue(out var t))
      {
        TryExecuteTask(t);
      }
    }

    //Other methods

    protected override IEnumerable<Task> GetScheduledTasks()
    {
      return tasksCollection.ToArray();
    }

    protected override void QueueTask(Task task)
    {
      if (task != null)
        tasksCollection.Enqueue(task);
    }

    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
      return false;
    }

    private void Dispose(bool disposing)
    {
      if (!disposing) return;
      //tasksCollection.CompleteAdding();
      //tasksCollection.Dispose();
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
  }
}
