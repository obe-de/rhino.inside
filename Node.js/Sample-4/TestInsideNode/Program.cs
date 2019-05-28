using System;

namespace TestInsideNode.Core
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      var rhinoTaskManager = new InsideNode.RhinoInsideTaskManager();
      rhinoTaskManager.StartGrasshopperTask(null);
      rhinoTaskManager.DoSomethingTask(null);
    }
  }
}
