/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

const dictionary = [];

const points = {
    'e, a, i, o, n, r, t, l, s, u': 1,
    'd, g': 2,
    'b, c, m, p': 3,
    'f, h, v, w, y': 4,
    'k': 5,
    'j, x': 8,
    'q, z': 10
};

const scoreLetter = l => {
    return Object.entries(points).reduce((acc, curr) => {
        if (curr[0].indexOf(l) != -1) acc = curr[1];
        return acc;
    }, 0);
};

const sum = (acc, curr) => acc + curr;
const scoreWord = w => w.split('').map(l => scoreLetter(l)).reduce(sum);
const byScore = (a, b) => scoreWord(b) - scoreWord(a);

var N = parseInt(readline());
for (var i = 0; i < N; i++) {
    var word = readline();
    dictionary.push(word);
}

const LETTERS = readline();

var possibleWords = dictionary.filter(word => {
    let available = Array.from(LETTERS);
    if (word.length > LETTERS.length) return false;

    for (let i = 0; i < word.length; i++) {
        let c = word.charAt(i);
        let index = available.indexOf(c);
        if (index === -1) {
            return false;
        }
        else {
            available.splice(index, 1);
        }
    }

    // Still here? We can make the word!
    return true;
});

possibleWords.sort(byScore);
print(possibleWords[0]);