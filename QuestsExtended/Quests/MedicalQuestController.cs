using EFT;
using EFT.HealthSystem;
using QuestsExtended.Models;
using QuestsExtended.Utils;
using UnityEngine;

namespace QuestsExtended.Quests;

internal class MedicalQuestController 
    : AbstractCustomQuestController
{
    public MedicalQuestController(QuestExtendedController questExtendedController)
        : base(questExtendedController)
    {
        _player.ActiveHealthController.EffectRemovedEvent += HandleRemoveHealthCondition;
        _player.ActiveHealthController.HealthChangedEvent += HandleHealthChange;
        _player.ActiveHealthController.BodyPartDestroyedEvent += HandleBodyPartDestroyed;
        _player.ActiveHealthController.BodyPartRestoredEvent += HandleBodyPartRestored;
        _player.ActiveHealthController.DiedEvent += HandleDie;
    }
    
    public void OnDestroy()
    {
        _player.ActiveHealthController.EffectRemovedEvent -= HandleRemoveHealthCondition;
        _player.ActiveHealthController.HealthChangedEvent -= HandleHealthChange;
        _player.ActiveHealthController.BodyPartDestroyedEvent -= HandleBodyPartDestroyed;
        _player.ActiveHealthController.BodyPartRestoredEvent -= HandleBodyPartRestored;
        _player.ActiveHealthController.DiedEvent -= HandleDie;
    }
    
    private void HandleRemoveHealthCondition(IEffect effect)
    {
        if (RE.FractureType.IsInstanceOfType(effect))
        {
            HandleRemoveFracture(effect);
            return;
        }
        
        if (RE.LightBleedType.IsInstanceOfType(effect))
        {
            HandleRemoveLightBleed(effect);
            return;
        }
        
        if (RE.HeavyBleedType.IsInstanceOfType(effect))
        {
            HandleRemoveHeavyBleed(effect);
            return;
        }
    }

    private void HandleHealthChange(EBodyPart bodyPart, float change, DamageInfo damage)
    {
        if (change.Positive())
        {
            HandleHealthGain(bodyPart, change, damage);
        }
        
        if (change.Negative())
        {
            HandleHealthLoss(bodyPart, change, damage);
        }
    }
    
    private void HandleRemoveFracture(IEffect effect)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.FixFracture);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, effect.BodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }
    
    private void HandleRemoveLightBleed(IEffect effect)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.FixLightBleed);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, effect.BodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }
    
    private void HandleRemoveHeavyBleed(IEffect effect)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.FixHeavyBleed);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, effect.BodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }

    private void HandleHealthLoss(EBodyPart bodyPart, float change, DamageInfo damage)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.HealthLoss);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, Mathf.Abs(change));
        }
    }

    private void HandleHealthGain(EBodyPart bodyPart, float change, DamageInfo damage)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.HealthGain);

        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, change);
        }
    }
    
    private void HandleBodyPartDestroyed(EBodyPart bodyPart, EDamageType damageType)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.DestroyBodyPart);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }

    private void HandleBodyPartRestored(EBodyPart bodyPart, ValueStruct value)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.RestoreBodyPart);
        
        foreach (var condition in conditions)
        {
            if (CheckBaseMedicalConditions(condition, bodyPart)) 
                IncrementCondition(condition, 1f);
        }
    }
    
    private void HandleDie(EDamageType damageType)
    {
        var conditions = _questController.GetActiveConditions(EQuestCondition.Die);
        
        foreach (var condition in conditions)
        {
            if (condition.CustomCondition.DamageTypes is not null)
            {
                if (!condition.CustomCondition.DamageTypes.Contains(damageType)) 
                    continue;

                if (CheckBaseMedicalConditions(condition, EBodyPart.Common))
                {
                    IncrementCondition(condition, 1f);
                }
                
                continue;
            }
            
            if (CheckBaseMedicalConditions(condition, EBodyPart.Common)) 
                IncrementCondition(condition, 1f);
        }
    }
}