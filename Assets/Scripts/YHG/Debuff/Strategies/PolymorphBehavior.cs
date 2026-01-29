using UnityEngine;

//변이, 원하는 모델로
public class PolymorphBehavior : IDebuffBehavior
{
    private float originalSpeed;
    private GameObject animalInstance;

    public void OnEnter(IDebuffable target, DebuffInfo info)
    {
        originalSpeed = target.GetOriginalSpeed();
        target.SetModelVisibility(false);
        GameObject selectedModel = null;

        if (info.PolymorphPrefabs != null && info.PolymorphPrefabs.Length > 0)
        {
            //랜덤
            int randomIndex = Random.Range(0, info.PolymorphPrefabs.Length);
            selectedModel = info.PolymorphPrefabs[randomIndex];
        }
        else if (info.VisualPrefab != null)
        {
            selectedModel = info.VisualPrefab;
        }
        if (selectedModel != null)
        {
            animalInstance = Object.Instantiate(selectedModel, target.transform.position, target.transform.rotation);
            animalInstance.transform.SetParent(target.transform);
        }

        float penalty = (info.Value > 0f) ? info.Value : 0.5f;
        float speedMultiplier = Mathf.Clamp01(1.0f - penalty);

        target.SetSpeed(originalSpeed * speedMultiplier); 
    }

    public void OnExecute(IDebuffable target) {    }

    public void OnExit(IDebuffable target)
    {
        target.SetModelVisibility(true);
        if (animalInstance != null)
        {
            Object.Destroy(animalInstance);
        }
        target.SetSpeed(originalSpeed);
    }
}
