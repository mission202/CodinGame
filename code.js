const bracketsClosed = function (string) {
    const tracker = { '} {': 0, ') (': 0, '] [': 0 };

    for (let char of string) {
        for (let bracketType of Object.entries(tracker)) {
            let value = bracketType[0].indexOf(char) - 1; // -1=Close 1=Open
            if (value === -2) continue; // not this bracket type
            if (bracketType[1] < 1 && value === -1) return false; // Trying to close unopened
            tracker[bracketType[0]] = bracketType[1] + value;
        }
    }

    return Object.values(tracker).every(x => x <= 0);
};

print(bracketsClosed(readline()));