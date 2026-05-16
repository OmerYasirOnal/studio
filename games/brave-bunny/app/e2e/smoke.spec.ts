import { test, expect } from '@playwright/test';

test('app boots and renders R3F canvas', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (e) => errors.push(e.message));
  page.on('console', (msg) => {
    if (msg.type() === 'error') errors.push(msg.text());
  });

  await page.goto('/');

  await expect(page.locator('[data-testid="game-canvas"]')).toBeVisible();

  await page.waitForTimeout(500);
  expect(errors).toEqual([]);
});
