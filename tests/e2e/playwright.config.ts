import { defineConfig, devices } from '@playwright/test';

/**
 * Playwright configuration for WEX Corporate Payments E2E tests.
 * Base URL defaults to local dev server; override with BASE_URL env var in CI.
 *
 * Run locally:   npm test  (requires Angular dev server running on port 4200)
 * Run in CI:     BASE_URL=https://uat.wex.example.com npm run test:ci
 */
export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  workers: process.env['CI'] ? 1 : undefined,

  reporter: [
    ['html', { outputFolder: 'playwright-report', open: 'never' }],
    ['list'],
  ],

  use: {
    baseURL: process.env['BASE_URL'] ?? 'http://localhost:4200',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },

  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    // Mobile viewports
    {
      name: 'mobile-chrome',
      use: { ...devices['Pixel 5'] },
    },
  ],

  // Automatically start Angular dev server when running locally
  // Comment out in CI where the server is already running
  // webServer: {
  //   command: 'npm run start:local',
  //   cwd: '../../src/WEX.UI',
  //   url: 'http://localhost:4200',
  //   reuseExistingServer: !process.env['CI'],
  //   timeout: 120 * 1000,
  // },
});
