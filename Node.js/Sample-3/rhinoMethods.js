const path = require('path');
var version = 'core';
var namespace = 'InsideNode.' + version.charAt(0).toUpperCase() + version.substr(1);
if(version === 'core') version = 'coreapp';

const baseNetAppPath = path.join(__dirname, namespace +'/bin/Debug/net'+ version +'2.0');

process.env.EDGE_USE_CORECLR = 1;
process.env.EDGE_APP_ROOT = baseNetAppPath;

var edge = require('electron-edge-js');

var baseDll = path.join(baseNetAppPath, namespace + '.dll');

var rhinoTypeName = namespace + '.RhinoMethods';

var startRhino = edge.func({
    assemblyFile: baseDll,
    typeName: rhinoTypeName,
    methodName: 'StartRhino'
});

var doSomething = edge.func({
    assemblyFile: baseDll,
    typeName: rhinoTypeName,
    methodName: 'DoSomething'
});

// wait for the rhino3dm web assembly to load asynchronously
rhino3dm().then(function(m) {
    rhino = m; // global
    run();
  });

function run() {

    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera( 75, window.innerWidth/window.innerHeight, 0.1, 1000 );
    var controls = new THREE.OrbitControls( camera );

    var renderer = new THREE.WebGLRenderer({antialias: true});
    renderer.setSize( window.innerWidth, window.innerHeight );
    document.body.appendChild( renderer.domElement );

    camera.position.z = 5;

    startRhino('', function(error, result) {
        //if (error) throw JSON.stringify(error);
        //document.getElementById("Rhino").innerHTML = result;
        console.log('Rhino has started.');
    });

    doSomething('', function(error, result) {
        //if (error) throw JSON.stringify(error);
        //document.getElementById("RhinoDo").innerHTML = result;
  
        //convert this to object
        var obj = JSON.parse(result);
        var rhinoMesh = rhino.CommonObject.decode(obj);
  
        let material = new THREE.MeshBasicMaterial( {wireframe: true, color: 0x00ff00 } );
        var threeMesh = meshToThreejs(rhinoMesh, material);

        scene.add(threeMesh);

    });

    var animate = function () {
        requestAnimationFrame( animate );

        controls.update();

        renderer.render( scene, camera );
    };

    animate();

};

function meshToThreejs(mesh, material) {
    var geometry = new THREE.BufferGeometry();
    var vertices = mesh.vertices();
    var vertexbuffer = new Float32Array(3 * vertices.count);
    for( var i=0; i<vertices.count; i++) {
      pt = vertices.get(i);
      vertexbuffer[i*3] = pt[0];
      vertexbuffer[i*3+1] = pt[1];
      vertexbuffer[i*3+2] = pt[2];
    }
    // itemSize = 3 because there are 3 values (components) per vertex
    geometry.addAttribute( 'position', new THREE.BufferAttribute( vertexbuffer, 3 ) );
  
    indices = [];
    var faces = mesh.faces();
    for( var i=0; i<faces.count; i++) {
      face = faces.get(i);
      indices.push(face[0], face[1], face[2]);
      if( face[2] != face[3] ) {
        indices.push(face[2], face[3], face[0]);
      }
    }
    geometry.setIndex(indices);
  
    var normals = mesh.normals();
    var normalBuffer = new Float32Array(3*normals.count);
    for( var i=0; i<normals.count; i++) {
      pt = normals.get(i);
      normalBuffer[i*3] = pt[0];
      normalBuffer[i*3+1] = pt[1];
      normalBuffer[i*3+2] = pt[1];
    }
    geometry.addAttribute( 'normal', new THREE.BufferAttribute( normalBuffer, 3 ) );
    return new THREE.Mesh( geometry, material );
  }
