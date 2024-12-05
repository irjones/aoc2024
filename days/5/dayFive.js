const fs = require("node:fs");

dayFive();

function dayFive() {
  console.log({
    partOne: partOneSolver(inputFetcher),
    partTwo: partTwoSolver(inputFetcher)
  });
}

function partOneSolver(fetcher) {
  const { rules, orderings } = parser(fetcher());
  const correctOrderings = [];
  for (const ordering of orderings) {
    const verifiedOrdering = [];
    for (let page of ordering) {
      let isValid = true;
      for (let [l, r] of rules) {
        const lIndex = ordering.indexOf(l);
        const rIndex = ordering.indexOf(r);
        // if both are present and in the wrong order
        if ((lIndex !== -1 && rIndex !== -1) && lIndex > rIndex) {
          isValid = false;
        }
      }
      if (isValid) {
        verifiedOrdering.push(page);
      }
    }
    correctOrderings.push(verifiedOrdering);
  }
  let sumOfMiddlePages = 0;
  for (const ordering of correctOrderings) {
    sumOfMiddlePages += parseInt(ordering[Math.round(ordering.length / 2) - 1] ?? '0');
  }
  return sumOfMiddlePages;
}

function partTwoSolver(fetcher) {
  const { rules, orderings } = parser(fetcher());
  const correctOrderings = [];
  for (const ordering of orderings) {
    let isCorrected = false;
    // copy for mutation
    const verifiedOrdering = [...ordering];
    for (let page of ordering) {
      for (let [l, r] of rules) {
        const lIndex = verifiedOrdering.indexOf(l);
        const rIndex = verifiedOrdering.indexOf(r);
        // if both are present and in the wrong order
        if ((lIndex !== -1 && rIndex !== -1) && lIndex > rIndex) {
          // swap them
          const temp = verifiedOrdering[rIndex];
          verifiedOrdering[rIndex] = verifiedOrdering[lIndex];
          verifiedOrdering[lIndex] = temp;
          isCorrected = true;
        }
      }
    }
    if (isCorrected) {
      correctOrderings.push(verifiedOrdering);
    }
  }
  let sumOfMiddlePages = 0;
  for (const ordering of correctOrderings) {
    sumOfMiddlePages += parseInt(ordering[Math.round(ordering.length / 2) - 1] ?? '0');
  }
  return sumOfMiddlePages;
}

function inputFetcher() {
  return fs.readFileSync("./data/input.txt").toString();
}

function parser(data) {
  const [rulesSection, orderingsSection] = data.split("\n\n");
  const rules = rulesSection.split("\n")
    .filter(ruleStr => !!ruleStr)
    .map(ruleStr => ruleStr.split("|"));
  const orderings = orderingsSection.split("\n")
    .filter(ordering => !!ordering)
    .map(ordering => ordering.split(","));
  return {
    rules,
    orderings
  }
}
