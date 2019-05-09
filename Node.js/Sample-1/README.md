# Rhino Inside Node.js
The Rhino Inside® technology allows Rhino and Grasshopper to be embedded within other products.

## Sample 1
This sample shows how to start Rhino from Node.js.
The sample has been tested on Windows 10, Rhino 7 WIP, and Node.js 8.11.12 (though should work on more recent versions of Node.js)

There are two projects:
- `insideNode.csproj` - Compiles to a .net class library with one class and one method. This class contains the code to start Rhino.
- `insideNodeApp.njsproj` - Contains the Node.js code which calls the dotnet class library. Uses [edge.js](https://github.com/agracio/edge-js) to call into dotnet.

### Dependencies
- Rhino WIP (7.0.19127.235, 5/7/2019)
- Node.js (8.11.2)
- edge.js (^11.3.1)

### Running this sample
This assumes you've installed Node.js for Windows.
1. Once you've cloned the Rhino.Inside repository, open the `insideNode.sln` in Visual Studio and build `insideNode.csproj`. This builds the .dll which is referenced in the Node.js project.
2. Open a console from the `insideNodeApp` directory.
2. Run `npm install` to install any dependencies.
3. Run `node app.js` to run the sample.

