using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Rhino.PlugIns;
using Rhino.Runtime.InProcess;

namespace InsideNode.Core
{
  public class RhinoMethods
  {
    static RhinoInsideTaskManager rhinoInsideTaskManager;
    
    static RhinoMethods()
    {
      rhinoInsideTaskManager = new RhinoInsideTaskManager();
    }

    public async Task<object> StartRhinoNow(dynamic input)
    {
      return rhinoInsideTaskManager.StartRhinoTask(input);
    }

    public async Task<object> StartGrasshopperNow(dynamic input)
    {
      
      return rhinoInsideTaskManager.StartGrasshopperTask(input);
    }

    public async Task<object> SubscribeToGH(dynamic input)
    {
      // Subscribe to events
      /*
      var editor = Grasshopper.Instances.DocumentEditor;
      var canvas = Grasshopper.Instances.ActiveCanvas;

      canvas.DocumentChanged += (Grasshopper.GUI.Canvas.GH_Canvas sender, Grasshopper.GUI.Canvas.GH_CanvasDocumentChangedEventArgs e) =>
      {
        ((Func<object, Task<object>>) input.event_handler)(e).Start();
      };
      */
      return rhinoInsideTaskManager.GrasshopperSubscribeTask(input);
    }


    public async Task<object> Subscribe(dynamic input)
    {
      var timer = new System.Timers.Timer(input.interval);
      timer.Elapsed += (Object source, System.Timers.ElapsedEventArgs e) =>
      {
        ((Func<object, Task<object>>)input.event_handler)(e).Start();
      };

      timer.Enabled = true;

      return (Func<object, Task<object>>) (async (dynamic data) =>
      {
        timer.Enabled = false;
        return null;
      });
    }

  }

  // from Compute: https://github.com/mcneel/compute.rhino3d/blob/master/src/compute.geometry/GeometryEndPoint.cs#L568-L671
  class ArchivableDictionaryResolver : JsonConverter
  {
    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(Rhino.Collections.ArchivableDictionary);
    }

    public override bool CanRead => true;
    public override bool CanWrite => true;

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      string encoded = (string) reader.Value;
      var dh = JsonConvert.DeserializeObject<DictHelper>(encoded);
      return dh.SerializedDictionary;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      string json = JsonConvert.SerializeObject(new DictHelper((Rhino.Collections.ArchivableDictionary) value));
      writer.WriteValue(json);
    }


    [Serializable]
    class DictHelper : ISerializable
    {
      public Rhino.Collections.ArchivableDictionary SerializedDictionary { get; set; }
      public DictHelper(Rhino.Collections.ArchivableDictionary d) { SerializedDictionary = d; }
      public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        SerializedDictionary.GetObjectData(info, context);
      }
      protected DictHelper(SerializationInfo info, StreamingContext context)
      {
        Type t = typeof(Rhino.Collections.ArchivableDictionary);
        var constructor = t.GetConstructor(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance,
          null, new Type[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);
        SerializedDictionary = constructor.Invoke(new object[] { info, context }) as Rhino.Collections.ArchivableDictionary;
      }
    }
  }

  public class GeometryResolver : DefaultContractResolver
  {
    static JsonSerializerSettings _settings;
    public static JsonSerializerSettings Settings
    {
      get
      {
        if (_settings == null)
        {
          _settings = new JsonSerializerSettings { ContractResolver = new GeometryResolver() };
          // return V6 ON_Objects for now
          var options = new Rhino.FileIO.SerializationOptions();
          options.RhinoVersion = 6;
          options.WriteUserData = true;
          _settings.Context = new System.Runtime.Serialization.StreamingContext(System.Runtime.Serialization.StreamingContextStates.All, options);
          _settings.Converters.Add(new ArchivableDictionaryResolver());
        }
        return _settings;
      }
    }

    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
      JsonProperty property = base.CreateProperty(member, memberSerialization);
      if (property.DeclaringType == typeof(Rhino.Geometry.Circle))
      {
        property.ShouldSerialize = _ =>
        {
          return property.PropertyName != "IsValid" && property.PropertyName != "BoundingBox" && property.PropertyName != "Diameter" && property.PropertyName != "Circumference";
        };

      }
      if (property.DeclaringType == typeof(Rhino.Geometry.Plane))
      {
        property.ShouldSerialize = _ =>
        {
          return property.PropertyName != "IsValid" && property.PropertyName != "OriginX" && property.PropertyName != "OriginY" && property.PropertyName != "OriginZ";
        };
      }

      if (property.DeclaringType == typeof(Rhino.Geometry.Point3f) ||
          property.DeclaringType == typeof(Rhino.Geometry.Point2f) ||
          property.DeclaringType == typeof(Rhino.Geometry.Vector2f) ||
          property.DeclaringType == typeof(Rhino.Geometry.Vector3f))
      {
        property.ShouldSerialize = _ =>
        {
          return property.PropertyName == "X" || property.PropertyName == "Y" || property.PropertyName == "Z";
        };
      }

      if (property.DeclaringType == typeof(Rhino.Geometry.MeshFace))
      {
        property.ShouldSerialize = _ =>
        {
          return property.PropertyName != "IsTriangle" && property.PropertyName != "IsQuad";
        };
      }
      return property;
    }
  }


}
