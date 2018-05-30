/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

const dictionary = [];

var points = {
    'e, a, i, o, n, r, t, l, s, u' : 1,
    'd, g' : 2,
    'b, c, m, p': 3,
    'f, h, v, w, y': 4,
    'k': 5,
    'j, x': 8,
    'q, z': 10
};

const scoreLetter = l => {
    if ('e, a, i, o, n, r, t, l, s, u'.indexOf(l) != -1) return 1;
    if ('d, g'.indexOf(l) != -1) return 2;
    if ('b, c, m, p'.indexOf(l) != -1) return 3;
    if ('f, h, v, w, y'.indexOf(l) != -1) return 4;
    if ('k'.indexOf(l) != -1) return 5;
    if ('j, x'.indexOf(l) != -1) return 8;
    if ('q, z'.indexOf(l) != -1) return 10;
    return 0;
}

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