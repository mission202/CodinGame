export default class Unit {
    constructor(input) {
        var inputs = input.split(' ');
        this.owner = parseInt(inputs[0]);
        this.buildingType = parseInt(inputs[1]);
        this.x = parseInt(inputs[2]);
        this.y = parseInt(inputs[3]);
    }
}