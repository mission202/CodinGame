// require ('Unit');
import Unit from '../src/Unit';

describe('Unit Class', function () {
    test('it should initialise from a string', () => {
        let u = new Unit();

        expect(u.x).toBe(1);
    });
})