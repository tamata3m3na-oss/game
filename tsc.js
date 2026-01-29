#!/usr/bin/env node
const { spawnSync } = require("child_process");
const path = require("path");

const tscPath = path.resolve(__dirname, "backend", "node_modules", ".bin", "tsc");
const args = process.argv.slice(2);
const hasProjectFlag = args.some((arg) => arg === "-p" || arg === "--project");
const hasHelpFlag = args.some(
  (arg) => arg === "-h" || arg === "--help" || arg === "-v" || arg === "--version"
);

const finalArgs = hasProjectFlag || hasHelpFlag
  ? args
  : ["-p", path.resolve(__dirname, "backend", "tsconfig.json"), ...args];

const result = spawnSync(tscPath, finalArgs, {
  stdio: "inherit",
});

process.exit(result.status ?? 0);
