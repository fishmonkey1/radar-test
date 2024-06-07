[System.Serializable]
public class DamageInfo
{

    public enum AreaDamage
    {
        //I might add more later, but just these for now
        POINT, //Only apply to target hit
        SPHERE //Check area in a sphere and do damage based on falloff provided
    }

    public float Damage;
    public float SphereRadius;
    public AreaDamage AreaDamageType; //What kind of area does this damage do
    //Editor useable curve for determining what damage is done at x distance from the center
    public UnityEngine.AnimationCurve DamageFalloffCurve;

}
