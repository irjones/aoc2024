const chars = require("node:fs").readFileSync("./input.txt").toString();
const memMap = expand(chars);
const compressed = compress(memMap);
let sum = 0;
for (let i = 0; i < compressed.length; i += 1) {
  if (compressed[i] !== ".") {
    sum += compressed[i] * i;
  }
}

console.log({
  partTwo: sum,
});

function compress(memMap) {
  let compressed = new Array(memMap.endIndex).fill(".");
  while (memMap.files.length) {
    // gets higher IDs first
    const nextFile = memMap.files.pop();
    let didMove = false;
    for (let i = 0; i < memMap.mem.length; i += 1) {
      const nextMemSpace = memMap.mem[i];
      if (nextMemSpace.available >= nextFile.space && nextMemSpace.startIndex < nextFile.startIndex) {
        // write to compressed
        for (let j = nextMemSpace.startIndex; j < nextMemSpace.startIndex + nextFile.space; j += 1) {
          compressed[j] = nextFile.id;
        }
        // reduce mem space here
        nextMemSpace.available -= nextFile.space;
        nextMemSpace.startIndex += nextFile.space;
        didMove = true;
        break;
      }
    }
    // there was no mem space, write to old loc
    if (!didMove) {
      for (let fi = nextFile.startIndex; fi < nextFile.startIndex + nextFile.space; fi += 1) {
        compressed[fi] = nextFile.id;
      }
    }
  }
  return compressed;
}

function expand(chars) {
  let files = [];
  let mem = [];
  let currentId = 0;
  let isFreeSpace = false;
  let sharedIndex = 0;
  for (let i = 0; i < chars.length; i += 1) {
    const value = parseInt(chars[i]);
    if (isFreeSpace) {
      mem.push({ available: value, startIndex: sharedIndex });
      sharedIndex += value;
    } else {
      const file = { id: currentId, startIndex: sharedIndex, space: value };
      sharedIndex += value; // this is a .length value, treat accordingly
      files.push(file);
      currentId += 1;
    }
    isFreeSpace = !isFreeSpace;
  }
  return { files, mem, endIndex: sharedIndex };
}
