import { test, expect } from '@playwright/test';

test('app boots and renders hero', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (e) => errors.push(e.message));
  page.on('console', (msg) => {
    if (msg.type() === 'error') errors.push(msg.text());
  });

  await page.goto('/');
  await expect(page.locator('[data-testid="game-canvas"]')).toBeVisible();
  await page.waitForTimeout(2000); // model load
  await expect(page.locator('[data-testid="joystick"]')).toBeVisible();
  await page.screenshot({ path: 'test-results/hero-loaded.png' });
  expect(errors).toEqual([]);
});
