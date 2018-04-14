const N = parseInt(readline()); // Number of elements which make up the association table.
const Q = parseInt(readline()); // Number Q of file names to be analyzed.
const mimes = new Map();

for (let i = 0; i < N; i++) {
    const inputs = readline().split(' ');
    mimes.set(inputs[0].toLowerCase(), inputs[1]);
}

const getMIME = (extension) => mimes.has(extension) ? mimes.get(extension) : "UNKNOWN";
const getExt = (name) => {
    const dot = name.lastIndexOf('.');
    return (dot === -1) ? "" : name.substring(dot + 1).toLowerCase();
};

for (let i = 0; i < Q; i++) {
    print(getMIME(getExt(readline())));
}