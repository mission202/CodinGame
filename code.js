const N = parseInt(readline());
const horses = [ ];
for (let i = 0; i < N; i++) {
    horses.push(parseInt(readline()));
}

horses.sort((a, b) => a-b);

var previous = 0;
var delta = Number.MAX_SAFE_INTEGER;
horses.forEach((val, idx) => {
    if (val - previous < delta) delta = val - previous;
    previous = val;
});

print(delta);