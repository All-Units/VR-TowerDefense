using UnityEngine;
using UnityEngine.Events;

public class ManaModule : MonoBehaviour
{
    [SerializeField] private float maxMana;
    public UnityEvent<float> onManaChange;

    private float currentMana
    {
        get => _mana;
        set
        {
            _mana = value;
            onManaChange?.Invoke(_mana/maxMana);
        }
    }
    private float _mana ;

    private void Start()
    {
        currentMana = maxMana;
    }


    public void CollectMana(float mana)
    {
        currentMana = Mathf.Min(maxMana, currentMana + mana);
    }

    public bool TryUseMana(float mana)
    {
        if (currentMana < mana) return false;

        currentMana = Mathf.Max(0, currentMana - mana);
        return true;
    }
}