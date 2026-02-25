using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CityStageController : NetworkBehaviour
{
    [Header("Stages")]
    [SerializeField] private GameObject[] stages;

    [Header("UI")]
    [SerializeField] private Button mainButton;
    [SerializeField] private Button choiceA;
    [SerializeField] private Button choiceB;
    [SerializeField] private TMP_Text descriptionText;

    private NetworkVariable<int> currentStage =
        new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
                                    NetworkVariableWritePermission.Server);

    private NetworkVariable<int> crowdingChoice =
        new NetworkVariable<int>(-1); // -1 none, 0 housing, 1 industry

    void Start()
    {
        if (IsClient)
        {
            mainButton.onClick.AddListener(RequestAdvanceStage);
            choiceA.onClick.AddListener(() => RequestChoice(0));
            choiceB.onClick.AddListener(() => RequestChoice(1));
        }

        currentStage.OnValueChanged += (_, __) => ApplyStage();
        crowdingChoice.OnValueChanged += (_, __) => ApplyStage();
    }

    public override void OnNetworkSpawn()
    {
        ApplyStage();
    }

    // =========================
    // SERVER AUTHORITY
    // =========================

    void RequestAdvanceStage()
    {
        AdvanceStageServerRpc();
    }

    void RequestChoice(int choice)
    {
        MakeChoiceServerRpc(choice);
    }

    [ServerRpc(RequireOwnership = false)]
    void AdvanceStageServerRpc()
    {
        if (currentStage.Value >= stages.Length - 1)
            return;

        currentStage.Value++;
    }

    [ServerRpc(RequireOwnership = false)]
    void MakeChoiceServerRpc(int choice)
    {
        if (crowdingChoice.Value != -1)
            return;

        crowdingChoice.Value = choice;
    }

    // =========================
    // CLIENT REACTION
    // =========================

    void ApplyStage()
    {
        for (int i = 0; i < stages.Length; i++)
            stages[i].SetActive(i == currentStage.Value);

        UpdateUI();
        UpdateChoices();
    }

    void UpdateUI()
    {
        switch (currentStage.Value)
        {
            case 0:
                descriptionText.text = "The land is empty.";
                mainButton.GetComponentInChildren<TMP_Text>().text = "Begin development";
                break;
            case 1:
                descriptionText.text = "The city begins to grow.";
                mainButton.GetComponentInChildren<TMP_Text>().text = "Expand";
                break;
            case 2:
                descriptionText.text = "A difficult decision must be made.";
                mainButton.GetComponentInChildren<TMP_Text>().text = "Proceed";
                break;
        }
    }

    void UpdateChoices()
    {
        bool showChoices = currentStage.Value == 2 && crowdingChoice.Value == -1;

        choiceA.gameObject.SetActive(showChoices);
        choiceB.gameObject.SetActive(showChoices);
    }
}
