const p = s => console.error(s);
const readInt = () => parseInt(readline());

const distance = (a, b) => (a.x - b.x) + (a.y - b.y);

p(`Distance: ${distance({ x: 0, y: 0 }, { x: 11, y: 11 })}`);

class GameState {
    constructor() {
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
        this.myGold = readInt();
        this.myIncome = readInt();
        p(`ME Gold: ${this.myGold} Income: ${this.myIncome}`);

        this.enemyGold = readInt();
        this.enemyIncome = readInt();
        p(`THINE ENEMY Gold: ${this.enemyGold} Income: ${this.enemyIncome}`);

        for (let i = 0; i < 12; i++) {
            const line = readline();
            p(line);
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
        return {
            mine: () => this.entities.units.filter(x => x.isMine),
            myHQ: () => this.entities.buildings.find(x => x.isMine && x.isHQ),
            enemyHQ: () => this.entities.buildings.find(x => x.isEnemy && x.isHQ)
        }
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
    
    const cmds = [];
    const cmd = {
        move: (id, x, y) => cmds.push(`MOVE ${id} ${x} ${y}`),
        train: (level, x, y) => cmds.push(`TRAIN ${level} ${x} ${y}`),
        wait: () => cmds.push(`WAIT`),
        msg: s => cmds.push(`MSG ${s}`)
    };

    var mine = find.mine();
    var upkeep = mine.reduce((a, x) => a + x.upkeep, 0);
    p(`Upkeep: ${upkeep} Avail to Spend: ${(gold + income) - upkeep}`);
    if ((gold + income) - upkeep >= 1) {
        p(`Able to Purchase Units...`);
        // Hacky deployment slot close to HQ
        const trainX = Math.max(1, find.myHQ().x - 1);
        const trainY = Math.max(0, find.myHQ().y);

        cmd.train(1, trainX, trainY);
    }

    if (mine.length === 0)
        cmd.wait;
    else /* Hack: Charge the enemy HQ. TODO: Land grab! */
        mine.forEach(x => cmd.move(x.id, find.enemyHQ().x, find.enemyHQ().y));

    if (cmds.length === 0) cmd.wait();

    // TODO: Upkeep > Income? We're going to start dying!

    console.log(cmds.join(";"))
}