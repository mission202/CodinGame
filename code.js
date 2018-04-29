const UNIT = {
    queen: 'QUEEN',
    knight: 'KNIGHT',
    archer: 'ARCHER',
    giant: 'GIANT'
};
const CMD = {
    wait: "WAIT",
    move: (coord) => `MOVE ${coord.x} ${coord.y}`,
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

const sortByDistance = (a, b) => {
    if (a.distance < b.distance) return -1;
    if (a.distance > b.distance) return 1;
    return 0;
};

const addDistance = (site, unit) => { return { distance: distance({ x: unit.x, y: unit.y }, { x: site.x, y: site.y }), ...site }; };
const isEmpty = site => site.structureType === -1;
const friendly = entity => entity.owner === 0;
const enemy = entity => entity.owner === 1;
const unit = (entity, type) => entity.type === type;
const canTrain = site => friendly(site) && site.canBuild;

const surveyState = (sites, state) => {
    const result = {
        emptySites: sites.filter(isEmpty),
        my: {
            queen: state.units.find(x => x.isFriendly && x.type === UNIT.queen),
            units: {
                all: state.units.filter(x => x.isFriendly),
                giants: state.units.filter(x => x.isFriendly && x.type === UNIT.giant),
                knights: state.units.filter(x => x.isFriendly && x.type === UNIT.knight),
                archers: state.units.filter(x => x.isFriendly && x.type === UNIT.archer),
            },
            sites: {
                all: sites.filter(friendly),
                canTrain: sites.filter(canTrain),
                giants: sites.filter(x => canTrain(x) && x.type === UNIT.giant),
                archers: sites.filter(x => canTrain(x) && x.type === UNIT.archer),
                knights: sites.filter(x => canTrain(x) && x.type === UNIT.knight),
                towers: sites.filter(x => friendly(x) && x.structureType === 1),
            }
        },
        enemy: {
            queen: state.units.find(x => !x.isFriendly && x.type === UNIT.queen),
            units: {
                all: state.units.filter(x => !x.isFriendly),
                giants: state.units.filter(x => !x.isFriendly && x.type === UNIT.giant),
                knights: state.units.filter(x => !x.isFriendly && x.type === UNIT.knight),
                archers: state.units.filter(x => !x.isFriendly && x.type === UNIT.archer),
            },
            sites: {
                all: sites.filter(enemy),
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

let turn = 0;
let gs = {};

while (true) {
    turn++;

    let inputs = readline().split(' ');
    gs = {
        ...gs,
        turn: turn,
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

        // printErr(`Site: ${JSON.stringify(sites.find(s => s.id === siteId))}`);
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

    // Prioritise Sites Closest to Start (Distance from Queen on Turn #1)
    if (turn === 1) {
        let clone = Array.from(sites).map(x => addDistance(x, survey.my.queen));
        clone.sort(sortByDistance)
        gs.priorities = clone.map(x => x.id);
    }

    const getQueenCommand = () => {
        if (survey.emptySites.length === 0)
            return CMD.wait;

        // Make sure Queenie isn't about to get her royal butt kicked.
        if (survey.my.queen.health <= 50 && survey.enemy.units.knights.length >= 4) {
            // RUN HOME QUEENIE!
            let evac = sites.find(x => x.id === gs.priorities[0]);

            if (typeof(evac) !== 'undefined')
                return CMD.move(evac);
        }

        // Iterate through priorities on gs, if Empty, Archer, Knight, Giant then Towers
        for (let i = 0; i < gs.priorities.length; i++) {
            const site = sites.find(x => x.id === gs.priorities[i]);

            if (isEmpty(site)) {
                if (i < 2) return CMD.barracks(site, UNIT.archer);
                if (i < 3) return CMD.barracks(site, UNIT.knight);
                // if (i < 4) return CMD.barracks(site, UNIT.giant);
                return CMD.tower(site);
            }
        }

        return CMD.wait;
    };

    print(getQueenCommand());

    /* TODO: Want to Build Based on Risk Assessment
        Lots of:
        KNIGHTS - Need Archers
        TOWERS - Need Giants
        ARCHERS - Need Towers?
    */

    let toTrain = [];

    // if (survey.enemy.sites.towers.length > 0 && survey.my.units.giants.length < 1) Array.prototype.push.apply(toTrain, survey.my.sites.giants);
    if (survey.my.units.archers.length < 3) Array.prototype.push.apply(toTrain, survey.my.sites.archers);
    /* if (survey.my.units.knights.length < 2) */ Array.prototype.push.apply(toTrain, survey.my.sites.knights);

    printErr(`Sites to Train: ${JSON.stringify(toTrain.map(x => x.id))}`);
    print(CMD.train(toTrain));
}