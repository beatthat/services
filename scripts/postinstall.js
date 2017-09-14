const unpm = require('unity-npm-utils');
const path = require('path');

const pkgRoot = path.join(__dirname, '..');

unpm.installPlugin(pkgRoot);
