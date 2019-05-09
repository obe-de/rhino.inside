// require edge.js: https://github.com/agracio/edge-js
var edge = require('edge-js');

// link to dll with Rhino.Inside code
var rhinoInside = edge.func('../insideNode/bin/Debug/insideNode.dll');

// run Rhino.Inside code
rhinoInside('', function (error, result) {

    if (error) throw error;

    console.log(result);

});
