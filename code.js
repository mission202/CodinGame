const bracketsClosed = function (string) {
    let chars = string.split('');

    const tracker = { '} {': 0, ') (': 0, '] [': 0 };

    for (let i = 0; i < chars.length; i++) {
        Object.entries(tracker).forEach(element => {
            var idx = element[0].indexOf(chars[i]);
            if (idx === -1) return;

            let shift = tracker[element[0]] + (idx - 1);
            if (shift === -1 && tracker[element[0]] === 0) return;
            tracker[element[0]] = shift;
        });
    }

    return Object.values(tracker).every(x => x === 0);
};


print(bracketsClosed(readline()));