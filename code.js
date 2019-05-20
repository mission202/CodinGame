const p = s => console.error(s);
const readInt = () => parseInt(readline());

const distance = (a, b) => Math.abs((a.x - b.x) + (a.y - b.y));

p(`Distance: ${distance({ x: 0, y: 0 }, { x: 11, y: 11 })}`);

const cmd = {
    move: (id, x, y) => `MOVE ${id} ${x} ${y}`,
    train: (level, x, y) => `TRAIN ${level} ${x} ${y}`,
    wait: () => `WAIT`,
    msg: s => `MSG ${s}`
};

class GameState {
    constructor() {
        this.map = [];
        this.cmds = [];
        this.myGold = 0;
        this.myIncome = 0;
        this.enemyGold = 0;
        this.enemyIncome = 0;
        this.entities = {
            units: [],
            buildings: [],
            mines: []
        }
    }

    newTurn() {
        this.map = Array(144);
        this.cmds = [];
        this.myGold = readInt();
        this.myIncome = readInt();
        p(`ME Gold: ${this.myGold} Income: ${this.myIncome}`);

        this.enemyGold = readInt();
        this.enemyIncome = readInt();
        p(`THINE ENEMY Gold: ${this.enemyGold} Income: ${this.enemyIncome}`);

        for (let y = 0; y < 12; y++) {
            const cells = readline().split();
            p(cells.join());
            cells.forEach((cell, x) => {
                this.map.push({
                    isVoid: cell === '#',
                    isNeutral: cell === '.',
                    ownedCaptured: cell === 'O',
                    ownedInactive: cell === 'o',
                    enemyCaptured: cell === 'X',
                    enemyInactive: cell === 'x',
                    x: x,
                    y: y
                });
            });
        }

        const buildingCount = readInt();
        this.entities.buildings = [];
        for (let i = 0; i < buildingCount; i++) {
            const inputs = readline().split(' ');
            const owner = parseInt(inputs[0]);
            const buildingType = parseInt(inputs[1]);
            const x = parseInt(inputs[2]);
            const y = parseInt(inputs[3]);
            this.entities.buildings.push({
                isMine: owner === 0,
                isEnemy: owner === 1,
                isHQ: buildingType === 0,
                x: x,
                y: y
            });
        }

        const unitCount = readInt();
        this.entities.units = [];
        for (let i = 0; i < unitCount; i++) {
            var inputs = readline().split(' ');
            const owner = parseInt(inputs[0]);
            this.entities.units.push({
                id: parseInt(inputs[1]),
                isMine: owner === 0,
                isEnemy: owner === 1,
                level: parseInt(inputs[2]),
                upkeep: 1,  /* TODO - Upkeep Cost Will Change for Higher Levels */
                x: parseInt(inputs[3]),
                y: parseInt(inputs[4])
            });
        }
    }

    find() {

        const emptyEnemy = cell => !cell.isEnemy && this.entities.units.filter(x => x.isEnemy && x.x === cell.x && x.y == cell.y);

        return {
            mine: () => this.entities.units.filter(x => x.isMine),
            myHQ: () => this.entities.buildings.find(x => x.isMine && x.isHQ),
            enemyHQ: () => this.entities.buildings.find(x => x.isEnemy && x.isHQ),
            closestCapturable: () => {
                const hq = this.entities.buildings.find(x => x.isMine && x.isHQ);
                p(`My HQ: ${hq.x},${hq.y}`);
                const capturable = this.map
                    .filter(x => !(x.x === hq.x && x.y === hq.y))
                    .filter(x => x.isNeutral || emptyEnemy(x))
                    .map(x => ({ x: x.x, y: x.y, dist: distance(x, hq) }))
                    .sort((a, b) => a.dist - b.dist);

                // return capturable[0];

                p(capturable.map(x => `${x.x},${x.y}@${x.dist}`).join(`  `));
            }
        }
    }

    do(command) {
        this.cmds.push(command);
    }

    makeItSo() {
        return (this.cmds.length === 0)
            ? cmd.wait()
            : this.cmds.join(";");
    }
}

const state = new GameState();

const numberMineSpots = parseInt(readline());
for (let i = 0; i < numberMineSpots; i++) {
    var inputs = readline().split(' ');
    const x = parseInt(inputs[0]);
    const y = parseInt(inputs[1]);
}

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