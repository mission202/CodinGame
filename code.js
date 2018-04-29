const UNIT = {
    queen: 'QUEEN',
    knight: 'KNIGHT',
    archer: 'ARCHER',
    giant: 'GIANT'
};
const CMD = {
    wait: "WAIT",
    barracks: (site, type) => `BUILD ${site.id} BARRACKS-${type}`,            // Will move if necessary.
    train: (sites) => `TRAIN ${sites.map(x => x.id).join(' ')}`.trim(),    // List of ID's to train at
    tower: (site) => `BUILD ${site.id} TOWER`
};
const barracksType = type => {
    switch (type) {
        case 0: return UNIT.knight;
        case 1: return UNIT.archer;
        case 2: return UNIT.giant;
        default: return 'NONE';
    }
};
const unitType = type => {
    switch (type) {
        case -1: return UNIT.queen;
        case 0: return UNIT.knight;
        case 1: return UNIT.archer;
        case 2: return UNIT.giant;
        default: return 'UNKNOWN';
    }
};

const distance = (a, b) => {
    var x = a.x - b.x;
    var y = a.y - b.y;
    return Math.round(Math.sqrt(x * x + y * y));
};

const addDistance = (site, unit) => { return { distance: distance({ x: unit.x, y: unit.y }, { x: site.x, y: site.y }), ...site }; };
const isEmpty = site => site.structureType === -1;
const friendly = entity => entity.owner === 0;
const enemy = entity => entity.owner === 1;
const unit = (entity, type) => entity.type === type;
const canTrain = site => friendly(site) && site.canBuild;

const surveyState = (sites, state) => {
    const result = {
        my : {
            queen: state.units.find(x => x.isFriendly && x.type === UNIT.queen),
            units: {
                all: state.units.filter(x => x.isFriendly),
                giants: state.units.filter(x => x.isFriendly && x.type === UNIT.giant),
                knights: state.units.filter(x => x.isFriendly && x.type === UNIT.knight),
                archers: state.units.filter(x => x.isFriendly && x.type === UNIT.archer),
            },
            sites: {
                canTrain: sites.filter(canTrain),
                giants: sites.filter(x => canTrain(x) && x.type === UNIT.giant),
                archers: sites.filter(x => canTrain(x) && x.type === UNIT.archer),
                knights: sites.filter(x => canTrain(x) && x.type === UNIT.knight),
                towers: sites.filter(x => friendly(x) && x.structureType === 1),
            }
        },
        enemy: {
            queen: state.units.find(x => !x.isFriendly && x.type === UNIT.queen),
            sites: {
                giants: sites.filter(x => !friendly(x) && x.type === UNIT.giant),
                archers: sites.filter(x => !friendly(x) && x.type === UNIT.archer),
                knights: sites.filter(x => !friendly(x) && x.type === UNIT.knight),
                towers: sites.filter(x => !friendly(x) && x.structureType === 1),
            }
        },
    };

    return result;
};


// HERE BE SITES!
var numSites = parseInt(readline());

const sites = Array(numSites).fill({}).map(() => {
    var inputs = readline().split(' ');
    return {
        id: parseInt(inputs[0]),
        x: parseInt(inputs[1]),
        y: parseInt(inputs[2]),
        radius: parseInt(inputs[3])
    };
});

// game loop
while (true) {
    let inputs = readline().split(' ');

    const gs = {
        units: [],
        gold: parseInt(inputs[0]),
        touchedSite: parseInt(inputs[1]),   // -1 if none
    };

    for (var i = 0; i < numSites; i++) {
        let inputs = readline().split(' ');
        let siteId = parseInt(inputs[0]);
        let index = sites.findIndex(s => s.id === siteId);
        let base = sites[index];

        sites[index] = {
            ...base,
            ignore1: parseInt(inputs[1]),           // used in future leagues
            ignore2: parseInt(inputs[2]),           // used in future leagues
            structureType: parseInt(inputs[3]),     // -1 = No structure, 1 = Tower, 2 = Barracks
            owner: parseInt(inputs[4]),             // -1 = No structure, 0 = Friendly, 1 = Enemy
            canBuild: parseInt(inputs[5]) === 0,    // -1 No Structure, != 0 Cooldown
            type: barracksType(parseInt(inputs[6])) // -1 No Structure, 0 for KNIGHT, 1 for ARCHER, 2 for GIANT
        };
        printErr(`Site: ${JSON.stringify(sites.find(s => s.id === siteId))}`);
    }

    var numUnits = parseInt(readline());
    for (var i = 0; i < numUnits; i++) {
        inputs = readline().split(' ');
        let unit = {
            x: parseInt(inputs[0]),
            y: parseInt(inputs[1]),
            owner: parseInt(inputs[2]),             // 0 = Friendly, 1 = Enemy
            isFriendly: parseInt(inputs[2]) === 0,
            type: unitType(parseInt(inputs[3])),
            health: parseInt(inputs[4])
        };

        gs.units.push(unit);
    }

    var survey = surveyState(sites, gs);

    printErr(`Units:`)
    printErr(` Queen: ${JSON.stringify(survey.my.queen)}`);
    printErr(` Enemy Queen: ${JSON.stringify(survey.enemy.queen)}`);

    gs.mySites = sites.filter(friendly);
    gs.sitesOwned = gs.mySites.length;
    gs.emptySites = sites.filter(isEmpty);
    gs.enemyKnights = gs.units.filter(x => x.type === UNIT.knight);

    if (gs.emptySites.length === 0) {
        print(CMD.wait);
    } else {
        let closest = sites
            .filter(isEmpty)
            .map(site => addDistance(site, survey.my.queen))
            .reduce((acc, curr) => (curr.distance < acc.distance ? curr : acc));

        if (gs.sitesOwned < 1) {
            // Always Build GIANTs First
            print(CMD.barracks(closest, UNIT.giant));
        } else if (gs.sitesOwned < 2) {
            // Then some KNIGHTs
            print(CMD.barracks(closest, UNIT.knight));
        } else if (gs.sitesOwned < 3) {
            // Then some ARCHERs
            print(CMD.barracks(closest, UNIT.archer));
        } else {
            // Spam Towers
            print(CMD.tower(closest));
        }
    }

    /* TODO: Want to Build Based on Risk Assessment
        Lots of:
        KNIGHTS - Need Archers
        TOWERS - Need Giants
        ARCHERS - Need Towers?
    */

    let giants = survey.my.sites.giants;
    let archers = survey.my.sites.archers;
    let knights = survey.my.sites.knights;

    printErr(`Avail Sites:: GIANTS ${JSON.stringify(giants)} ARCHERS ${JSON.stringify(archers)} KNIGHTS ${JSON.stringify(knights)}`);
    printErr(`Friendly Units:`);
    printErr(`  GIANTS ${JSON.stringify(survey.my.units.giants)}`);
    printErr(`  KNIGHTS ${JSON.stringify(survey.my.units.knights)}`);
    printErr(`  ARCHERS ${JSON.stringify(survey.my.units.archers)}`);

    let toTrain = [ ];

    if (survey.enemy.sites.towers.length > 0 && survey.my.units.giants.length < 1) Array.prototype.push.apply(toTrain, giants);
    if (survey.my.units.archers.length < 3) Array.prototype.push.apply(toTrain, archers);    
    /* if (survey.my.units.knights.length < 2) */ Array.prototype.push.apply(toTrain, knights);

    printErr(`Sites to Train: ${JSON.stringify(toTrain)}`);

    print(CMD.train(toTrain));
}