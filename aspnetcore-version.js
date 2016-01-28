var jsonfile = require('jsonfile');
var semver = require('semver');

var file = './src/tinypng/project.json';
var buildVersion = process.env.APPVEYOR_BUILD_VERSION.substring(1);

var findPoint       = buildVersion.lastIndexOf(".");
var basePackageVer  = buildVersion.substring(0, findPoint);
var buildNumber     = buildVersion.substring(findPoint + 1, buildVersion.length);
var semversion      = semver.valid(basePackageVer + "." + buildNumber)

jsonfile.readFile(file, function (err, project) {
    console.error(err);
    project.version = semversion;
    jsonfile.writeFile(file, project, {spaces: 2}, function(err) {
        console.error("We've failed to update the build number");
        console.error(err);
    });
})