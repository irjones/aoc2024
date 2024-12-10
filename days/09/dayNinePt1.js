const chars = require("node:fs").readFileSync("./input.txt").toString();

function expand(chars) {
  let arr = [];
  let currentId = 0;
  let isFreeSpace = false;
  for (let i = 0; i < chars.length; i += 1) {
    const value = parseInt(chars[i]);
    if (isFreeSpace) {
      arr = arr.concat(new Array(value).fill("."));
    } else {
      arr = arr.concat(new Array(value).fill(currentId))
      currentId += 1;
    }
    isFreeSpace = !isFreeSpace;
  }
  return arr;
}

const expanded = expand(chars);

for (let left = 0, right = expanded.length - 1; left < right;) {
  if (expanded[left] !== ".") {
    left += 1;
    continue;
  }
  if (expanded[right] === ".") {
    right -= 1;
    continue;
  }
  expanded[left] = expanded[right];
  expanded[right] = '.';
}

let sum = 0;
for (let i = 0; i < expanded.length; i += 1) {
  if (expanded[i] === ".") break;
  sum += i * expanded[i];
}

console.log({
  partOne: sum,
});
