/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/


// game loop
while (true) {
    for (var i = 0; i < 2; i++) {
        var inputs = readline().split(' ');
        var playerHealth = parseInt(inputs[0]);
        var playerMana = parseInt(inputs[1]);
        var playerDeck = parseInt(inputs[2]);
        var playerRune = parseInt(inputs[3]);
    }
    var opponentHand = parseInt(readline());
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
    }

    // Write an action using print()
    // To debug: printErr('Debug messages...');

    print('PASS');
}