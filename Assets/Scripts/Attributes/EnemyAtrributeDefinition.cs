using UnityEngine;

// Defining a uniform Attribute class. This allows for 1 list of attributes in enemy definition. 
public abstract class EnemyAttributeDefinition : ScriptableObject
{
    public abstract void ApplyTo(EnemyController Enemy);
}