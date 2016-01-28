var jsonfile = require('jsonfile');
var semver = require('semver');

var file = './src/tinypng/project.json';

jsonfile.readFile(file, function (err, project) {
    if (err)
    {
        console.error("Couldn't open project.json");
        return;
    }
    console.log(process.env.toString());
    project.version = process.env.APPVEYOR_BUILD_VERSION;
    jsonfile.writeFile(file, project, {spaces: 2}, function(err) {
        if (err)
        {
            console.error("We've failed to update the build number");
        }
    });
})