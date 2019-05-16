# Rhino Inside Node.js
The Rhino Inside® technology allows Rhino and Grasshopper to be embedded within other products.

## Sample 4
This sample shows how to run Rhino from `Electron`.
This sample extends Sample 1, 2, and 3 by using Electron for UI. Geometry rendered with [three.js](https://threejs.org).
The sample has been tested on Windows 10, Rhino 7 WIP, and Node.js 8.11.12 (though should work on more recent versions of Node.js)

There is one project:
- `InsideNode.Core.csproj` - Compiles to a .net Core 2.0 class library with one class and several method. This class contains the code to start Rhino.

### Dependencies
- Rhino WIP (7.0.19127.235, 5/7/2019)
- Node.js (8.11.2)
- edge.js (^11.3.1)
- Electron
- Three.js

### Running this sample
This assumes you've installed Node.js for Windows.
1. Open a console from the `Sample-4` directory.
2. Run `npm install` to install any dependencies.
open the `InsideNode.Core.sln` in Visual Studio and build the solution. This builds the .dll which is referenced in the Node.js project.
3. Run `npm start` to run the sample.

