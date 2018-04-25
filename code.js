/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

const distance = (a, b) => {
    var x = a.x - b.x;
    var y = a.y - b.y;
    return Math.sqrt(x * x + y * y);
};

// HERE BE SITES!
var numSites = parseInt(readline());

const sites = Array(numSites).fill({}).map(() => {
    var inputs = readline().split(' ');
    return {
        id: inputs[0],
        x: parseInt(inputs[1]),
        y: parseInt(inputs[2]),
        radius: parseInt(inputs[3])
    };
});

printErr(JSON.stringify(sites));

// game loop
while (true) {
    var inputs = readline().split(' ');
    var gold = parseInt(inputs[0]);
    var touchedSite = parseInt(inputs[1]); // -1 if none
    var siteBuildings = new Map();
    for (var i = 0; i < numSites; i++) {
        var inputs = readline().split(' ');
        var siteId = parseInt(inputs[0]);
        var ignore1 = parseInt(inputs[1]); // used in future leagues
        var ignore2 = parseInt(inputs[2]); // used in future leagues
        var structureType = parseInt(inputs[3]); // -1 = No structure, 2 = Barracks
        var owner = parseInt(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
        var param1 = parseInt(inputs[5]);
        var param2 = parseInt(inputs[6]);
        siteBuildings.set(siteId, { structureType: parseInt(inputs[3]) });
    }

    // QUEENIE ACTION
    // First line: A valid queen action (WAIT, MOVE {x} {y}, BUILD {siteId} BARRACKS-{type})

    // Hack - Just get to nearest empty site and build shit!
    if (touchedSite != -1 && (siteBuildings.get(touchedSite).structureType == -1)) {
        print(`BUILD ${touchedSite} BARRACKS-KNIGHT`); // TODO: Some Intelligence, Please?
    } else {
        // MOVE ALONG
        // TODO: Get closest unoccupied Site
        // Get there, innit? :)
        print('WAIT');
    }

    var myQueen;

    var numUnits = parseInt(readline());
    for (var i = 0; i < numUnits; i++) {
        var inputs = readline().split(' ');
        var x = parseInt(inputs[0]);
        var y = parseInt(inputs[1]);
        var owner = parseInt(inputs[2]);
        var unitType = parseInt(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
        var health = parseInt(inputs[4]);

        if (owner == 0 && unitType == -1) myQueen = { x:x, y:y, health:health }
    }

    var closestSite = sites
        .map(site => { distance: distance({ x: myQueen.x, y: myQueen.y}, {x: sites.x, y: sites.y}) })
        .reduce((acc, curr) => curr.distance < acc.distance ? curr : distance);

    printErr(JSON.stringify(closestSite));


    // Write an action using print()
    // To debug: printErr('Debug messages...');


    // TRAINING ACTION
    // Second line: A set of training instructions (TRAIN {listOfIds})
    print('TRAIN');
}