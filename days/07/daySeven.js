/**
 * Note to the reader: This is an incredibly rough draft. It works, but needs cleaned up.
 */

const fs =  require("node:fs");

const fac = factorial();
const fetcher = testFetcher
console.log({
    pt1: partOne(fetcher),
    pt2: partTwo(fetcher, fac)
});

function generateCombosForTwo(arr, n) {
    const out = [];
    let start = parseInt(''.padStart(n, '0'), 2);
    const pad = ''.padStart(n, '1')
    const end = n | parseInt(pad, 2);
    const combos = [];
    for (; start <= end; start += 1) {
        const mask = start.toString(2).padStart(n, '0');
        ;
        combos.push(mask.split('').map((c) => arr[parseInt(c)]))
    }
    return combos;
}

function partOne(fetcher) {
    const data = parser(fetcher());

    return data.filter(datum => hasCorrectSolutionForTwoOperands(datum))
        .reduce((acc, next) => acc + next.result, 0n);
}

function hasCorrectSolutionForTwoOperands(datum) {
    const operands = [
        function mult(x, y) {
            return x * y;
        },
        function add(x, y) {
            return x + y;
        }
    ];
    const combinations = generateCombosForTwo(operands, datum.operatorsRequiredCount);

    for (const combo of combinations) {
        const ops = combo;
        const numsToProcess = [...datum.equation];
        let acc = numsToProcess.shift();
        while (numsToProcess.length > 0) {
            const next = numsToProcess.shift();
            acc = ops.shift()(acc, next);
        }
        if (acc === datum.result) {
            return true;
        }
    }
    return false;
}

function partTwo(fetcher, fac) {
    const data = parser(fetcher());
    return data.filter(datum => hasCorrectSolutionForThreeOperands(datum, fac))
        .reduce((acc, next) => acc + next.result, 0n)
}

function hasCorrectSolutionForThreeOperands(datum, fac) {
    const operands = [
        function mult(x, y) {
            return x * y;
        },
        function add(x, y) {
            return x + y;
        },
        function concat(x, y) {
            return BigInt(`${x}${y}`);
        }
    ];
    const combinations = generateCombos(operands, datum.operatorsRequiredCount, fac);
    for (const combo of combinations) {
        const ops = combo;
        const numsToProcess = [...datum.equation];
        let acc = numsToProcess.shift();
        while (numsToProcess.length > 0) {
            const next = numsToProcess.shift();
            acc = ops.shift()(acc, next);
        }
        if (acc === datum.result) {
            return true;
        }
    }
    return false;
}

/** Generates all combos from `arr` using the array length as the radix for the increment/array access via bitwise chicanery **/
function generateCombos(arr, n, fac) {

    const r = arr.length;
    const totalCombos = possibleCombos(arr, n, fac);
    let start = parseInt(''.padStart(totalCombos, '0'), r);
    const end = totalCombos
    const combos = [];

    for (; start <= end; start += 1) {
        const mask = start.toString(r).padStart(n, '0');
        combos.push(mask.split('').map((c) => arr[parseInt(c, r)]))
    }
    return combos.filter(c => c.length === n);
}

function factorial() {
    const cache = {};
    cache[1] = 1;
    cache[2] = 2;

    return function fac(n) {
        if (cache[n]) return cache[n];
        const cacheKeys = Object.keys(cache);
        const highestCalculatedKey = cacheKeys.toSorted()[cacheKeys.length - 1];
        let result = cache[highestCalculatedKey];
        for (let i = parseInt(highestCalculatedKey) + 1; i <= n; i += 1) {
            result *= i;
            cache[i] = result;
        }
        return result;
    }
}

function possibleCombos(arr, r, fac) {
    // // nCr = n! / (n - r)!r! did not work for some reason.
    // TODO: troubleshoot the combinations algo. This works but it's dicey and inefficient. Need to determine the exact amount of possible combos
    return 1000000
}

function parser(data) {
    return data.split('\n')
        .filter((line) => !!line)
        .map((line) => line.split(": "))
        .map(([result, equationParts]) => {
            const equation = equationParts.split(' ').map(BigInt);
            return {
                result: BigInt(result),
                equation,
                operatorsRequiredCount: equation.length - 1
            }
        });
}


function testFetcher() {
    return fs.readFileSync("./data/testinput.txt").toString();
}

function inputFetcher() {
    return fs.readFileSync("./data/input.txt").toString();
}