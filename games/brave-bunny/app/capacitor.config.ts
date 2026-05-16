import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'com.omeryasir.bravebunny',
  appName: 'Brave Bunny',
  webDir: 'dist',
  ios: {
    contentInset: 'never',
    scrollEnabled: false,
    backgroundColor: '#1a0d2e',
    path: '../ios',
  },
};

export default config;
