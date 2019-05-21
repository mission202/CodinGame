const path = require('path');

module.exports = {
    mode: 'development',
    devtool: '', // Don't need sourcemaps in CodinGame env.
    entry: "./src/index.js",
    output: {
        filename: "codingame.js",
        path: path.resolve(__dirname, 'dist')
    }
}