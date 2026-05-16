#!/usr/bin/env node
// Compresses raw .glb assets via gltf-transform: meshopt + prune.
// Usage: node compress.mjs
// Reads from ../../assets-raw/quaternius/, writes to ../../app/assets/glb/.

import { NodeIO } from '@gltf-transform/core';
import { ALL_EXTENSIONS, EXTMeshoptCompression } from '@gltf-transform/extensions';
import { meshopt, prune } from '@gltf-transform/functions';
import { MeshoptEncoder, MeshoptDecoder } from 'meshoptimizer';
import { readdir, mkdir } from 'node:fs/promises';
import { join, dirname, basename } from 'node:path';
import { fileURLToPath } from 'node:url';

const __dirname = dirname(fileURLToPath(import.meta.url));
const RAW_DIR = join(__dirname, '../../assets-raw/quaternius');
const OUT_DIR = join(__dirname, '../../app/public/assets/glb');

await mkdir(OUT_DIR, { recursive: true });
await MeshoptDecoder.ready;
await MeshoptEncoder.ready;

const io = new NodeIO()
  .registerExtensions(ALL_EXTENSIONS)
  .registerDependencies({
    'meshopt.decoder': MeshoptDecoder,
    'meshopt.encoder': MeshoptEncoder,
  });

const files = (await readdir(RAW_DIR)).filter((f) => f.endsWith('.glb'));
console.log(`Found ${files.length} glb files in ${RAW_DIR}`);

for (const file of files) {
  console.log(`\n=== Compressing ${file} ===`);
  const doc = await io.read(join(RAW_DIR, file));
  await doc.transform(prune(), meshopt({ encoder: MeshoptEncoder }));
  const outPath = join(OUT_DIR, basename(file));
  await io.write(outPath, doc);
  console.log(`  → ${outPath}`);
}

console.log('\nDone.');
