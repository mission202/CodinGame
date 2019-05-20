const p = s => console.error(s);
const readInt = () => parseInt(readline());

const numberMineSpots = parseInt(readline());
for (let i = 0; i < numberMineSpots; i++) {
    var inputs = readline().split(' ');
    const x = parseInt(inputs[0]);
    const y = parseInt(inputs[1]);
}

// game loop
while (true) {
    const gold = readInt();
    const income = readInt();

    p(`Gold: ${gold} Income: ${income}`);

    const opponentGold = readInt();
    const opponentIncome = readInt();
    for (let i = 0; i < 12; i++) {
        const line = readline();
        p(line);
    }
    const buildingCount = readInt();
    const buildings = [ ];
    for (let i = 0; i < buildingCount; i++) {
        var inputs = readline().split(' ');
        const owner = parseInt(inputs[0]);
        const buildingType = parseInt(inputs[1]);
        const x = parseInt(inputs[2]);
        const y = parseInt(inputs[3]);

        buildings.push({
            isMine: owner === 0,
            isEnemy: owner === 1,
            isHQ: buildingType === 0,
            x: x,
            y: y
        });
    }

    const unitCount = readInt();
    const units = [];

    const find = {
        mine: () => units.filter(x => x.isMine),
        myHQ: () => buildings.find(x => x.isMine && x.isHQ),
        enemyHQ: () => buildings.find(x => x.isEnemy && x.isHQ)
    };

    for (let i = 0; i < unitCount; i++) {
        var inputs = readline().split(' ');
        const owner = parseInt(inputs[0]);
        const unitId = parseInt(inputs[1]);
        const level = parseInt(inputs[2]);
        const x = parseInt(inputs[3]);
        const y = parseInt(inputs[4]);

        units.push({
            id: unitId,
            isMine: owner === 0,
            isEnemy: owner === 1,
            level: level,
            upkeep: 1,  /* TODO - Upkeep Cost Will Change for Higher Levels */
            x: x,
            y: y
        });
    }

    // Gamestate Init'd - Let's get to work!
    const cmds = [ ];
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