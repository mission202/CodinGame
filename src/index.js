const GameState = require('./GameState');

const p = s => console.error(s);

const cmd = {
    move: (id, x, y) => `MOVE ${id} ${x} ${y}`,
    train: (level, x, y) => `TRAIN ${level} ${x} ${y}`,
    wait: () => `WAIT`,
    msg: s => `MSG ${s}`
};

const io = {
    readline: () => readline(),
    writeline: console.log
};

const state = new GameState(io);

state.init();

// game loop
while (true) {
    state.newTurn();

    var mine = state.find().mine();
    var upkeep = mine.reduce((a, x) => a + x.upkeep, 0);
    if ((state.myGold + state.myIncome) - upkeep >= 1) {
        p(`Able to Purchase Units...`);
        // Hacky deployment slot close to HQ
        // TODO: Find closest I can deploy to HQ
        const capturable = state.find().closestCapturable();

        const trainX = Math.max(1, state.find().myHQ().x - 1);
        const trainY = Math.max(0, state.find().myHQ().y);

        state.do(cmd.train(1, trainX, trainY));
    }

    if (mine.length > 0) /* Hack: Charge the enemy HQ. TODO: Land grab! */
        mine.forEach(x => state.do(cmd.move(x.id, state.find().enemyHQ().x, state.find().enemyHQ().y)));

    // TODO: Upkeep > Income? We're going to start dying!

    console.log(state.makeItSo());
}