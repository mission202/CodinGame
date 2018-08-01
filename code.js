/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

const state = {
    turn: 0
};

const ACTIONS = {
    summon: (id, message) => `SUMMON ${id} ${message}`,
    attack: (myId, theirId, message) => `ATTACK ${myId} ${theirId} ${message}`,
    directAttack: (myId, message) => `ATTACK ${myId} -1 ${message}`
};

let turn = 0;

// game loop
while (true) {
    turn++;

    for (var i = 0; i < 2; i++) {
        var inputs = readline().split(' ');
        var playerHealth = parseInt(inputs[0]);
        var playerMana = parseInt(inputs[1]);
        var playerDeck = parseInt(inputs[2]);
        var playerRune = parseInt(inputs[3]);
    }
    var opponentHand = parseInt(readline());

    let cards = [];
    var cardCount = parseInt(readline());
    for (var i = 0; i < cardCount; i++) {
        var inputs = readline().split(' ');
        var cardNumber = parseInt(inputs[0]);
        var instanceId = parseInt(inputs[1]);
        var location = parseInt(inputs[2]);
        var cardType = parseInt(inputs[3]);
        var cost = parseInt(inputs[4]);
        var attack = parseInt(inputs[5]);
        var defense = parseInt(inputs[6]);
        var abilities = inputs[7];
        var myHealthChange = parseInt(inputs[8]);
        var opponentHealthChange = parseInt(inputs[9]);
        var cardDraw = parseInt(inputs[10]);

        cards.push({
            inputs: inputs,
            cardNumber: cardNumber,
            instanceId: instanceId,
            location: location,
            cardType: cardType,
            cost: cost,
            attack: attack,
            defense: defense,
            abilities: abilities,
            myHealthChange: myHealthChange,
            opponentHealthChange: opponentHealthChange,
            cardDraw: cardDraw
        });
    }

    // Write an action using print()
    // To debug: printErr('Debug messages...');

    // draft phase
    if (turn < 31) {
        print('PASS');
        continue;
    };

    let actions = [];
    let inhand = cards.filter(x => x.location === 0);

    printErr(`Cards in Hand: ${inhand.length}`);

    // TODO: Enough Mana for Summon - SUMMON!

    let canSummon = inhand.filter(x => x.cost <= playerMana).sort((a, b) => a.cost < b.cost);
    let manaLeft = playerMana;
    for (let i = 0; i < canSummon.length; i++) {
        let summoning = canSummon[i];
        if (summoning.cost > manaLeft) break;
        manaLeft -= summoning.cost;
        actions.push(ACTIONS.summon(summoning.instanceId, `RELEASE THE ${summoning.instanceId}!`));
    }

    // TODO: Anything on Board - ATTACCCCKKK!!
    let onboard = cards.filter(x => x.location === 1);
    for(let creature of onboard) {
        actions.push(ACTIONS.directAttack(creature.instanceId, 'ATTAACCCKK!!'));
    }

    print(actions.join(";"));
}