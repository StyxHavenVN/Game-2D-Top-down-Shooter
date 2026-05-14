using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LevelUpManager : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject levelUpPanel;
    public TextMeshProUGUI[] buttonTexts;

    [Header("Player Stats Reference")]
    public PlayerStats playerStats;

    public enum UpgradeType { ATK, DEF, HP }

    private struct UpgradeOption
    {
        public string title;
        public UpgradeType type;
        public string description;
    }

    private List<UpgradeOption> allUpgrades = new List<UpgradeOption>();
    private UpgradeOption[] currentChoices = new UpgradeOption[3];

    void Start()
    {
        levelUpPanel.SetActive(false);

        // Đã đổi toàn bộ sang Tiếng Anh
        allUpgrades.Add(new UpgradeOption { title = "STRENGTH", type = UpgradeType.ATK, description = "+10% Damage" });
        allUpgrades.Add(new UpgradeOption { title = "DEFENSE", type = UpgradeType.DEF, description = "-10% Damage Taken" });
        allUpgrades.Add(new UpgradeOption { title = "VITALITY", type = UpgradeType.HP, description = "+20 Max HP" });
    }

    public void TriggerLevelUp()
    {
        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);

        List<UpgradeOption> pool = new List<UpgradeOption>(allUpgrades);
        for (int i = 0; i < 3; i++)
        {
            int randomIndex = Random.Range(0, pool.Count);
            currentChoices[i] = pool[randomIndex];

            // In chữ ra nút
            buttonTexts[i].text = currentChoices[i].title + "\n<size=80%>" + currentChoices[i].description + "</size>";
            pool.RemoveAt(randomIndex);
        }
    }

    public void SelectUpgrade(int buttonIndex)
    {
        UpgradeOption chosenUpgrade = currentChoices[buttonIndex];

        switch (chosenUpgrade.type)
        {
            case UpgradeType.ATK:
                playerStats.atkMultiplier += 0.1f;
                Debug.Log("Upgraded ATK");
                break;
            case UpgradeType.DEF:
                playerStats.defMultiplier += 0.1f;
                Debug.Log("Upgraded DEF");
                break;
            case UpgradeType.HP:
                playerStats.UpgradeMaxHP(20);
                Debug.Log("Upgraded HP");
                break;
        }

        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}