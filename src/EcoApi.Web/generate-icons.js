import sharp from 'sharp';
import { resolve } from 'path';

const publicDir = resolve('public');

async function generateIcons() {
  const logo = resolve(publicDir, 'logo.svg');
  const maskable = resolve(publicDir, 'maskable-icon.svg');

  // Generate 192x192
  await sharp(logo).resize(192, 192).png().toFile(resolve(publicDir, 'pwa-192x192.png'));
  
  // Generate 512x512
  await sharp(logo).resize(512, 512).png().toFile(resolve(publicDir, 'pwa-512x512.png'));

  // Generate Maskable 512x512
  await sharp(maskable).resize(512, 512).png().toFile(resolve(publicDir, 'maskable-icon-512x512.png'));

  console.log('PNG Icons generated successfully!');
}

generateIcons().catch(console.error);
