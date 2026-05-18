using System;

namespace YG
{
    public partial class SavesYG
    {
        public int gold = 0;
        public int totalClicks = 0;
        public int currentLevel = 1;
        public int clicksForNextLevel = 800;
        public int goldPerClick = 1;
        public float goldPerSecond = 0f;
        public int[] upgradeLevels = new int[0];

        public bool[] kittensPurchased = new bool[0];
        public int[] kittensSpriteIndex = new int[0];
        public float[] kittensGoldPerSecond = new float[0];

        public bool allColorsPurchased = false;
        public int selectedColorIndex = -1;

        public int GetUpgradeLevel(int index)
        {
            if (index < 0 || index >= upgradeLevels.Length) return 0;
            return upgradeLevels[index];
        }

        public void SetUpgradeLevel(int index, int level)
        {
            if (index < 0) return;
            if (index >= upgradeLevels.Length) Array.Resize(ref upgradeLevels, index + 1);
            upgradeLevels[index] = level;
        }

        public bool GetKittenPurchased(int index)
        {
            if (index < 0 || index >= kittensPurchased.Length) return false;
            return kittensPurchased[index];
        }

        public void SetKittenPurchased(int index, bool purchased)
        {
            if (index < 0) return;
            if (index >= kittensPurchased.Length) Array.Resize(ref kittensPurchased, index + 1);
            kittensPurchased[index] = purchased;
        }

        public int GetKittenSpriteIndex(int index)
        {
            if (index < 0 || index >= kittensSpriteIndex.Length) return 0;
            return kittensSpriteIndex[index];
        }

        public void SetKittenSpriteIndex(int index, int spriteIndex)
        {
            if (index < 0) return;
            if (index >= kittensSpriteIndex.Length) Array.Resize(ref kittensSpriteIndex, index + 1);
            kittensSpriteIndex[index] = spriteIndex;
        }

        public float GetKittenGoldPerSecond(int index)
        {
            if (index < 0 || index >= kittensGoldPerSecond.Length) return 0f;
            return kittensGoldPerSecond[index];
        }

        public void SetKittenGoldPerSecond(int index, float gps)
        {
            if (index < 0) return;
            if (index >= kittensGoldPerSecond.Length) Array.Resize(ref kittensGoldPerSecond, index + 1);
            kittensGoldPerSecond[index] = gps;
        }
    }
}