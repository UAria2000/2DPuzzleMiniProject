using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [System.Serializable]
    public class Rule
    {
        public string name;

        public List<string> requireItems = new();
        public List<string> forbidItems = new();

        public int timeCost = 0;

        public string setFlag;      // 단순화를 위해 1개만
        public string requireFlag;  // 단순화를 위해 1개만

        public string triggerEndingId; // 비어있으면 엔딩 없음
    }

    public List<Rule> rules = new();

    public void Interact()
    {
        foreach (var r in rules)
        {
            if (!string.IsNullOrEmpty(r.requireFlag) && !FlagStore.I.Has(r.requireFlag))
                continue;

            bool ok = true;

            foreach (var it in r.requireItems)
                if (!GameManager.I.HasItem(it)) ok = false;

            foreach (var it in r.forbidItems)
                if (GameManager.I.HasItem(it)) ok = false;

            if (!ok) continue;

            TimeManager.I.Spend(r.timeCost);

            if (!string.IsNullOrEmpty(r.setFlag))
                FlagStore.I.Set(r.setFlag);

            if (!string.IsNullOrEmpty(r.triggerEndingId))
                EndingManager.I.Trigger(r.triggerEndingId);

            return;
        }

        Debug.Log("조건 불충족: 아무 규칙도 실행되지 않음");
    }
}
