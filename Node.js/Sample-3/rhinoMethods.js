const path = require('path');
var version = 'core';
var namespace = 'InsideNode.' + version.charAt(0).toUpperCase() + version.substr(1);
if(version === 'core') version = 'coreapp';

const baseNetAppPath = path.join(__dirname, namespace +'/bin/Debug/net'+ version +'2.0');
console.log(baseNetAppPath);

process.env.EDGE_USE_CORECLR = 1;
process.env.EDGE_APP_ROOT = baseNetAppPath;

var edge = require('electron-edge-js');

var baseDll = path.join(baseNetAppPath, namespace + '.dll');
console.log(baseDll);

var rhinoTypeName = namespace + '.RhinoMethods';
console.log(rhinoTypeName);

console.log('Hi from Rhino Methods');

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

window.onload = function() {

    var scene = new THREE.Scene();
    var camera = new THREE.PerspectiveCamera( 75, window.innerWidth/window.innerHeight, 0.1, 1000 );
    var controls = new THREE.OrbitControls( camera );

    var renderer = new THREE.WebGLRenderer({antialias: true});
    renderer.setSize( window.innerWidth, window.innerHeight );
    document.body.appendChild( renderer.domElement );

    var geometry = new THREE.BoxGeometry( 1, 1, 1 );
    var material = new THREE.MeshBasicMaterial( { color: 0x00ff00 } );
    var cube = new THREE.Mesh( geometry, material );
    scene.add( cube );

    camera.position.z = 5;

    

    startRhino('', function(error, result) {
        //if (error) throw JSON.stringify(error);
        //document.getElementById("Rhino").innerHTML = result;
    });

    doSomething('', function(error, result) {
        //if (error) throw JSON.stringify(error);
        //document.getElementById("RhinoDo").innerHTML = result;
    });

    var animate = function () {
        requestAnimationFrame( animate );

        // cube.rotation.x += 0.01;
        // cube.rotation.y += 0.01;

        controls.update();

        renderer.render( scene, camera );
    };

    animate();

};
