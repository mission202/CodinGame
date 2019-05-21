const GameState = require('../src/GameState');

let input = [];
let output = [];

const io = {
    readline: () => input.shift(),
    writeline: s => output.push(s)
};

describe('GameState', function () {

    const gs = new GameState(io);

    describe('.init()', () => {

        it('should read in the mine locations', () => {
            input = [
                '3',
                '0 0',
                '1 1',
                '2 2',
            ];

            gs.init();

            expect(gs.mines.length).toBe(3);
        });

    });

});