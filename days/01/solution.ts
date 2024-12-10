import { readFileSync } from "node:fs";

type DataFetcher<T> = () => T
type ParsedData = {
  left: number[];
  right: number[];
}

dayOne();

function dayOne() {
  const one = partOne(getActualData);
  const two = partTwo(getActualData);
  console.log({
    one,
    two
  })
}

function partOne(fetcher: DataFetcher<ParsedData>) {
  const data = fetcher();

  const sortedLeft = data.left.toSorted();
  const sortedRight = data.right.toSorted();
  let distance = 0;

  for (let i = 0; i < sortedLeft.length; i += 1) {
    const left = sortedLeft[i];
    const right = sortedRight[i];
    distance += Math.abs(left - right);
  }

  return { distance }
}

function partTwo(fetcher: DataFetcher<ParsedData>) {
  const data = fetcher();

  const occurrenceCountMap = new Map<number, number>();
  for (const left of data.left) {
    for (const right of data.right) {
      if (left === right) {
        occurrenceCountMap.set(
          left,
          (occurrenceCountMap.get(left) ?? 0) + 1
        )
      }
    }
  }

  let similarityScore = 0;
  for (const [k, v] of occurrenceCountMap.entries()) {
    similarityScore += k * v;
  }

  return { similarityScore }
}

function parseRaw(data: string): ParsedData {
  return data.split('\n')
    .filter((line) => !!line)
    .map((line) => line.split(/\W+/))
    .reduce((acc, [left, right]) => {
      acc.left.push(parseInt(left));
      acc.right.push(parseInt(right));
      return acc;
    }, { left: [] as number[], right: [] as number[]});
}

function getTestData() {
  return parseRaw(`3   4
4   3
2   5
1   3
3   9
3   3
`);
}

function getActualData() {
  return parseRaw(readFileSync("./input.txt").toString());
}