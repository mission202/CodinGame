/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

 const CMD = {
     wait: "WAIT",
     build: (site, type) => `BUILD ${site.id} BARRACKS-${type}`,        // Will move if necessary.
     train: (sites) => `TRAIN ${sites.map(x => x.id).join(' ')}`.trim() // List of ID's to train at
 };

const distance = (a, b) => {
    var x = a.x - b.x;
    var y = a.y - b.y;
    return Math.round(Math.sqrt(x * x + y * y));
};

const addDistance = (site, unit) => { return { distance: distance({ x: unit.x, y: unit.y}, {x: site.x, y: site.y}), ...site }; };
const isEmpty = site => site.structureType === -1;
const friendly = site => site.owner === 0;
const canTrain = site => friendly(site) && site.canBuild;
const creepType = type => {
    switch(type) {
        case 0: return 'KNIGHT';
        case 1: return 'ARCHER';
        default: return 'NONE';
    }
}

// HERE BE SITES!
var numSites = parseInt(readline());

const sites = new Map();

Array(numSites).fill({}).map(() => {
    var inputs = readline().split(' ');
    sites.set(parseInt(inputs[0]), {
        id: parseInt(inputs[0]),
        x: parseInt(inputs[1]),
        y: parseInt(inputs[2]),
        radius: parseInt(inputs[3])
    });
});

// game loop
while (true) {
    var inputs = readline().split(' ');

    const gs = {
        gold: parseInt(inputs[0]),
        touchedSite: parseInt(inputs[1]),   // -1 if none
    }

    for (var i = 0; i < numSites; i++) {
        var inputs = readline().split(' ');
        var siteId = parseInt(inputs[0]);
        let s = sites.get(siteId);

        sites.set(siteId, {...s,
            ignore1: parseInt(inputs[1]),           // used in future leagues
            ignore2: parseInt(inputs[2]),           // used in future leagues
            structureType: parseInt(inputs[3]),     // -1 = No structure, 2 = Barracks
            owner: parseInt(inputs[4]),             // -1 = No structure, 0 = Friendly, 1 = Enemy
            canBuild: parseInt(inputs[5]) === 0,    // -1 No Structure, != 0 Cooldown
            type: creepType(parseInt(inputs[6]))    // -1 No Structure, 0 for KNIGHT, 1 for ARCHER
        });
        printErr(JSON.stringify(sites.get(siteId)));
    }

    var numUnits = parseInt(readline());
    for (var i = 0; i < numUnits; i++) {
        var inputs = readline().split(' ');
        var x = parseInt(inputs[0]);
        var y = parseInt(inputs[1]);
        var owner = parseInt(inputs[2]);
        var unitType = parseInt(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
        var health = parseInt(inputs[4]);

        if (owner == 0 && unitType == -1) gs.myQueen = { x:x, y:y, health:health };
    }

    printErr(`Queen: ${JSON.stringify(gs.myQueen)}`);

    var s = Array.from(sites.values());

    gs.sitesOwned = s.filter(friendly).length

    if (gs.sitesOwned < 3) {
        var closest = s
        .filter(isEmpty)
        .map(site => addDistance(site, gs.myQueen))
        .reduce((acc, curr) => (curr.distance < acc.distance ? curr : acc));
        print(CMD.build(closest, 'KNIGHT'));
    } else {
        print(CMD.wait);
    }

    var mySites = Array.from(sites.values()).filter(canTrain);
    print(CMD.train(mySites));
}