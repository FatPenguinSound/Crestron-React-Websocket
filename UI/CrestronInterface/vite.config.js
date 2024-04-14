import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import { viteSingleFile } from 'vite-plugin-singlefile';

export default ({ mode }) => {

  return defineConfig({
    plugins: [
      react(),
      viteSingleFile(),
    ],
    base: './',
  });
};