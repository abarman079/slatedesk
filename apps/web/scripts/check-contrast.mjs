const pairs = [
  {
    name: "Light body / canvas",
    foreground: "#142033",
    background: "#F5F3EE",
  },
  {
    name: "Light secondary / canvas",
    foreground: "#4E5867",
    background: "#F5F3EE",
  },
  {
    name: "Light cobalt / surface",
    foreground: "#2B5CE6",
    background: "#FFFEFB",
  },
  {
    name: "Light success badge",
    foreground: "#11745E",
    background: "#E5F5F0",
  },
  {
    name: "Light warning badge",
    foreground: "#9A5411",
    background: "#FFF0D9",
  },
  {
    name: "Light danger badge",
    foreground: "#B83240",
    background: "#FCE8EA",
  },
  {
    name: "Light primary button",
    foreground: "#FFFFFF",
    background: "#2B5CE6",
  },
  {
    name: "Dark body / surface",
    foreground: "#F4F7FB",
    background: "#121B2C",
  },
  {
    name: "Dark secondary / surface",
    foreground: "#B8C2D1",
    background: "#121B2C",
  },
  {
    name: "Dark cobalt / surface",
    foreground: "#9EB8FF",
    background: "#121B2C",
  },
  {
    name: "Dark success badge",
    foreground: "#74D5B8",
    background: "#133A32",
  },
  {
    name: "Dark warning badge",
    foreground: "#F3BE71",
    background: "#402C15",
  },
  {
    name: "Dark danger badge",
    foreground: "#FF9DA7",
    background: "#431F27",
  },
  {
    name: "Dark primary button",
    foreground: "#08101D",
    background: "#9EB8FF",
  },
  {
    name: "Dark danger button",
    foreground: "#08101D",
    background: "#FF9DA7",
  },
];

function hexToRgb(hex) {
  const normalized =
    hex.replace("#", "");

  return [
    Number.parseInt(
      normalized.slice(0, 2),
      16,
    ) / 255,

    Number.parseInt(
      normalized.slice(2, 4),
      16,
    ) / 255,

    Number.parseInt(
      normalized.slice(4, 6),
      16,
    ) / 255,
  ];
}

function linearize(value) {
  return value <= 0.04045
    ? value / 12.92
    : Math.pow(
        (value + 0.055) /
          1.055,
        2.4,
      );
}

function luminance(hex) {
  const [r, g, b] =
    hexToRgb(hex).map(
      linearize,
    );

  return (
    0.2126 * r +
    0.7152 * g +
    0.0722 * b
  );
}

function contrastRatio(
  foreground,
  background,
) {
  const first =
    luminance(foreground);

  const second =
    luminance(background);

  const lighter =
    Math.max(first, second);

  const darker =
    Math.min(first, second);

  return (
    (lighter + 0.05) /
    (darker + 0.05)
  );
}

const results =
  pairs.map((pair) => {
    const ratio =
      contrastRatio(
        pair.foreground,
        pair.background,
      );

    return {
      name: pair.name,
      ratio:
        ratio.toFixed(2),
      result:
        ratio >= 4.5
          ? "PASS"
          : "FAIL",
    };
  });

console.table(results);

const failures =
  results.filter(
    (result) =>
      result.result ===
      "FAIL",
  );

if (failures.length > 0) {
  console.error(
    "\nContrast verification failed.",
  );

  process.exit(1);
}

console.log(
  "\nAll normal-text token pairs pass the 4.5:1 target.",
);