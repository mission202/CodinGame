const distance = (uLat, uLong, dLat, dLong) => {
    var x = (dLong - uLong) * Math.cos((uLat + dLat) / 2);
    var y = (dLat - uLat);
    return Math.sqrt(Math.pow(x, 2) + Math.pow(y, 2)) * 6371;
};

const toDeg = (value) => parseFloat(value.replace(',', '.'));

const LON = toDeg(readline());
const LAT = toDeg(readline());
const N = parseInt(readline());
var closest;
for (let i = 0; i < N; i++) {
    const d = readline().split(';');
    const defib = {
        name: d[1],
        distance: distance(LAT, LON, toDeg(d[05]), toDeg(d[04]))
    };

    if (typeof(closest) === 'undefined' || defib.distance < closest.distance)  closest = defib;
}

print(closest.name);