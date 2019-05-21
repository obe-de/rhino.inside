using System;
using InsideNode.Core;

namespace TestInsideNode
{
  class Program
  {
    static void Main(string[] args)
    {
      Console.WriteLine("Hello World!");
      var rhinoMethods = new RhinoMethods();
      rhinoMethods.StartGrasshopperNow(null);

    }
  }
}
