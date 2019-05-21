module.exports = function (wallaby) {
    process.env.NODE_ENV = "test";

    return {
        files: [
            'src/**/*.js'
        ],

        tests: [
            'tests/**/*.tests.js'
        ],

        env: {
            type: 'node',
            runner: 'node'
        },

        testFramework: 'jest'
    };
};