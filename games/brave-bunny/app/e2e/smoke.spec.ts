import { test, expect } from '@playwright/test';

test('app boots, lobby renders, play starts run', async ({ page }) => {
  const errors: string[] = [];
  page.on('pageerror', (e) => errors.push(e.message));
  page.on('console', (msg) => {
    if (msg.type() === 'error') errors.push(msg.text());
  });

  await page.goto('/');
  // Boot screen (1.5s)
  await page.waitForTimeout(2500);

  // Lobby visible
  await expect(page.getByText(/Brave Bunny/i)).toBeVisible();
  await expect(page.getByRole('button', { name: /PLAY/i })).toBeVisible();

  // Click PLAY
  await page.getByRole('button', { name: /PLAY/i }).click();
  await page.waitForTimeout(1500);

  // In-run: canvas + joystick + HUD
  await expect(page.locator('[data-testid="game-canvas"]')).toBeVisible();
  await expect(page.locator('[data-testid="joystick"]')).toBeVisible();
  await page.screenshot({ path: 'test-results/in-run.png' });

  expect(errors).toEqual([]);
});
