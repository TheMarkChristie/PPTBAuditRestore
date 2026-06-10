/**
 * Assembles the Power Platform Toolbox distribution into ./dist.
 * PPTB loads `main` (index.html) and `icon` relative to the dist root.
 *
 * The actual tool UI is the single-file ../audit-restore.html (dual-mode: it uses
 * window.dataverseAPI when running inside Toolbox, and same-origin fetch as a web resource).
 * This script just copies the pieces into dist/ so there is one source of truth.
 *
 * Run:  node build.js
 */
const fs = require("fs");
const path = require("path");

const root = __dirname;
const dist = path.join(root, "dist");
// Prefer the in-repo copy (self-contained); fall back to the sibling dev copy.
const localHtml = path.join(root, "audit-restore.html");
const toolHtml = fs.existsSync(localHtml) ? localHtml : path.join(root, "..", "audit-restore.html");

fs.mkdirSync(dist, { recursive: true });

// 1) tool UI -> dist/index.html
fs.copyFileSync(toolHtml, path.join(dist, "index.html"));

// 2) icon + manifest + license + readme -> dist (npm publish runs from dist)
for (const f of ["icon.svg", "package.json", "LICENSE", "README.md"]) {
  const src = path.join(root, f);
  if (fs.existsSync(src)) fs.copyFileSync(src, path.join(dist, f));
}

console.log("Built dist/ →", dist);
console.log("  index.html, icon.svg, package.json, LICENSE, README.md");
